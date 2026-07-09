using System.Diagnostics;
using zosproxy.DTO;

namespace zosproxy.Service;

public class LocalCobolGateway
{
    private static readonly SemaphoreSlim CobolLock = new(1, 1);

    private readonly string LegacyCobolDirectory;
    private readonly string requestPath;
    private readonly string responsePath;
    private readonly string executablePath;

    public LocalCobolGateway(IWebHostEnvironment environment)
    {
        LegacyCobolDirectory = Path.GetFullPath(
                Path.Combine(environment.ContentRootPath, "..", "LegacyCobol")
            );

        requestPath = Path.Combine(LegacyCobolDirectory, "io", "request.txt");
        responsePath = Path.Combine(LegacyCobolDirectory, "io", "response.txt");

        var nameExutable = OperatingSystem.IsWindows() ? "alfa.exe" : "alfa";
        executablePath = Path.Combine(LegacyCobolDirectory, nameExutable);
    }

    public Task<ClientResponseDTO> ConsultarAsync(string id)
    {
        var request =
                Fixed("CONSULTAR", 10) +
                Fixed(id, 6) +
                Fixed("", 11) +
                Fixed("", 80);

        return ExecuteCobolAsync(request);
    }

    public Task<ClientResponseDTO> AtualizarAsync(string id, UpdateClientDTO clientup)
    {
        var request =
                Fixed("ATUALIZAR", 10) +
                Fixed(id, 6) +
                Fixed(clientup.Number, 11) +
                Fixed(clientup.Email, 80);

        return ExecuteCobolAsync(request);
    }

    private async Task<ClientResponseDTO> ExecuteCobolAsync(string requestLine)
    {
        await CobolLock.WaitAsync();

        try
        {
            ValidateCobolFiles();

            Directory.CreateDirectory(Path.GetDirectoryName(requestPath)!);
            File.WriteAllText(requestPath, requestLine + Environment.NewLine);

            if (File.Exists(responsePath))
            {
                File.Delete(responsePath);
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                WorkingDirectory = LegacyCobolDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(processInfo);

            if (process is null)
            {
                return Error("0500", "Nao foi possivel iniciar o programa COBOL.");
            }

            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            if (process.ExitCode != 0)
            {
                return Error(
                    "0500",
                    $"Programa COBOL terminou com erro. ExitCode={process.ExitCode}. {stderr} {stdout}".Trim()
                );
            }

            if (!File.Exists(responsePath))
            {
                return Error("0500", "Programa COBOL nao gerou o arquivo response.txt.");
            }

            var responseLine = await File.ReadAllTextAsync(responsePath);

            return ParseCobolResponse(responseLine);
        }
        catch (Exception ex)
        {
            return Error("0500", $"Erro ao executar COBOL: {ex.Message}");
        }
        finally
        {
            CobolLock.Release();
        }
    }

    private static ClientResponseDTO ParseCobolResponse(string responseLine)
    {
        var parts = responseLine.Trim().Split('|');

        if (parts.Length < 6)
        {
            return Error("0500", $"Resposta COBOL invalida: {responseLine}");
        }

        var codigoRetorno = parts[0].Trim();
        var mensagem = parts[1].Trim();
        var id = parts[2].Trim();
        var nome = parts[3].Trim();
        var telefone = parts[4].Trim();
        var email = parts[5].Trim();

        ClientDTO? cliente = null;

        if (!string.IsNullOrWhiteSpace(id))
        {
            cliente = new ClientDTO
            {
                Id = id,
                Name = nome,
                Number = telefone,
                Email = email
            };
        }

        return new ClientResponseDTO
        {
            Success = codigoRetorno == "0000",
            ResponseCode = codigoRetorno,
            Message = mensagem,
            Client = cliente
        };
    }

    private void ValidateCobolFiles()
    {
        if (!Directory.Exists(LegacyCobolDirectory))
        {
            throw new DirectoryNotFoundException(
                $"Diretorio LegacyCobol nao encontrado: {LegacyCobolDirectory}"
            );
        }

        if (!File.Exists(executablePath))
        {
            throw new FileNotFoundException(
                $"Executavel COBOL nao encontrado. Compile com: cobc -x -free -o alfa ALFA.cbl",
                executablePath
            );
        }
    }

    private static string Fixed(string value, int length)
    {
        value ??= "";

        value = value.Trim();

        if (value.Length > length)
        {
            value = value[..length];
        }

        return value.PadRight(length);
    }
    private static ClientResponseDTO Error(string codigoRetorno, string mensagem)
    {
        return new ClientResponseDTO
        {
            Success = false,
            ResponseCode = codigoRetorno,
            Message = mensagem
        };
    }

}

using System.Net.Http.Json;
using WebInterface.DTO;

namespace WebInterface.Service;

public class ClientCobolAPI
{
    private readonly HttpClient httpClient;

    public ClientCobolAPI(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<ClienteResponse?> ConsultarAsync(string codigo)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<ClienteResponse>($"/api/clientes/{codigo}");
        }
        catch (Exception ex)
        {
            return new ClienteResponse
            {
                Sucesso = false,
                CodigoRetorno = "0500",
                Mensagem = $"Erro ao consultar cliente: {ex.Message}"
            };
        }
    }

    public async Task<ClienteResponse?> AtualizarContatoAsync(
        string codigo,
        AtualizarClienteDTO request
    )
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync($"/api/clientes/{codigo}/contato", request);
            return await response.Content.ReadFromJsonAsync<ClienteResponse>();
        }
        catch (Exception ex)
        {
            return new ClienteResponse
            {
                Sucesso = false,
                CodigoRetorno = "0500",
                Mensagem = $"Erro ao atualizar contato: {ex.Message}"
            };
        }
    }
}



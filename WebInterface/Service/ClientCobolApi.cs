using System.Net.Http.Json;
using WebInterface.DTO;

namespace WebInterface.Service;

public class ClientCobolApi
{
    private readonly HttpClient httpClient;

    public ClientCobolApi(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<ClienteResponse?> ConsultarAsync(string id)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<ClienteResponse>($"/api/clientes/{id}");
        }
        catch (Exception ex)
        {
            return new ClienteResponse
            {
                Success = false,
                ResponseCode = "0500",
                Message = $"Erro ao consultar cliente: {ex.Message}"
            };
        }
    }

    public async Task<ClienteResponse?> AtualizarContatoAsync(
        string id,
        AtualizarClienteDTO request
    )
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync($"/api/clientes/{id}/contato", request);
            return await response.Content.ReadFromJsonAsync<ClienteResponse>();
        }
        catch (Exception ex)
        {
            return new ClienteResponse
            {
                Success = false,
                ResponseCode = "0500",
                Message = $"Erro ao atualizar contato: {ex.Message}"
            };
        }
    }
}

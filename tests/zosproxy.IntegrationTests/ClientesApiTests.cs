using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace zosproxy.IntegrationTests;

public class ClientesApiTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient httpClient;

    public ClientesApiTests()
    {
        var baseUrl = Environment.GetEnvironmentVariable("ZOSPROXY_BASE_URL")
                      ?? "http://localhost:5005";

        httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    [Fact]
    public async Task ConsultarClienteExistente_DeveRetornarCliente()
    {
        var response = await httpClient.GetAsync("/api/clientes/000001");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ClientResponse>(JsonOptions);

        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.Equal("0000", body.ResponseCode);
        Assert.Equal("Cliente encontrado.", body.Message);
        Assert.NotNull(body.Client);
        Assert.Equal("000001", body.Client.Id);
        Assert.False(string.IsNullOrWhiteSpace(body.Client.Name));
        Assert.False(string.IsNullOrWhiteSpace(body.Client.Email));
        Assert.False(string.IsNullOrWhiteSpace(body.Client.Number));
    }

    [Fact]
    public async Task ConsultarClienteInexistente_DeveRetornarNotFound()
    {
        var response = await httpClient.GetAsync("/api/clientes/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ClientResponse>(JsonOptions);

        Assert.NotNull(body);
        Assert.False(body.Success);
        Assert.Equal("0404", body.ResponseCode);
        Assert.Equal("Cliente nao encontrado.", body.Message);
    }

    [Fact]
    public async Task AtualizarContato_DevePersistirAlteracao()
    {
        var updateRequest = new UpdateClientRequest
        {
            Email = "teste.integracao@alfa.local",
            Number = "62911112222"
        };

        var updateResponse = await httpClient.PutAsJsonAsync(
            "/api/clientes/000001/contato",
            updateRequest,
            JsonOptions
        );

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updateBody = await updateResponse.Content.ReadFromJsonAsync<ClientResponse>(JsonOptions);

        Assert.NotNull(updateBody);
        Assert.True(updateBody.Success);
        Assert.Equal("0000", updateBody.ResponseCode);
        Assert.NotNull(updateBody.Client);
        Assert.Equal("000001", updateBody.Client.Id);
        Assert.Equal(updateRequest.Email, updateBody.Client.Email);
        Assert.Equal(updateRequest.Number, updateBody.Client.Number);

        var consultResponse = await httpClient.GetAsync("/api/clientes/000001");

        Assert.Equal(HttpStatusCode.OK, consultResponse.StatusCode);

        var consultBody = await consultResponse.Content.ReadFromJsonAsync<ClientResponse>(JsonOptions);

        Assert.NotNull(consultBody);
        Assert.True(consultBody.Success);
        Assert.NotNull(consultBody.Client);
        Assert.Equal(updateRequest.Email, consultBody.Client.Email);
        Assert.Equal(updateRequest.Number, consultBody.Client.Number);
    }

    private sealed class ClientResponse
    {
        public bool Success { get; set; }
        public string ResponseCode { get; set; } = "";
        public string Message { get; set; } = "";
        public ClientDto? Client { get; set; }
    }

    private sealed class ClientDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Number { get; set; } = "";
    }

    private sealed class UpdateClientRequest
    {
        public string Email { get; set; } = "";
        public string Number { get; set; } = "";
    }
}

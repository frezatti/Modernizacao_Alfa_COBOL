using Microsoft.AspNetCore.Mvc;
using zosproxy.DTO;
using zosproxy.Service;

namespace zosproxy.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly LocalCobolGateway cobolGateway;

    public ClientesController(LocalCobolGateway cobolGateway)
    {
        this.cobolGateway = cobolGateway;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClientResponseDTO>> Consultar(string id)
    {
        var response = await cobolGateway.ConsultarAsync(id);

        return response.ResponseCode switch
        {
            "0000" => Ok(response),
            "0404" => NotFound(response),
            "0422" => BadRequest(response),
            _ => StatusCode(500, response)
        };
    }

    [HttpPut("{id}/contato")]
    public async Task<ActionResult<ClientResponseDTO>> AtualizarContato(string id, [FromBody] UpdateClientDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.Number) ||
            string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new ClientResponseDTO
            {
                Success = false,
                ResponseCode = "0422",
                Message = "Telefone e e-mail sao obrigatorios."
            });
        }

        var response = await cobolGateway.AtualizarAsync(id, request);

        return response.ResponseCode switch
        {
            "0000" => Ok(response),
            "0404" => NotFound(response),
            "0422" => BadRequest(response),
            _ => StatusCode(500, response)
        };
    }
}

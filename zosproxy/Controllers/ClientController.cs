using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zosproxy.DTO;

namespace zosproxy.Controllers;

[ApiController]
[Route("zos/client")]
public class ClientController : Controller
{
    [AllowAnonymous]
    [HttpGet("zos/client/fetch/{id:int}")]
    public async Task<ClientDTO> GetClient()
    {
        return new ClientDTO();
    }
    
    [AllowAnonymous]
    [HttpPut("zos/client/put/{id:int}")]
    public async Task<ClientResponseDTO> UpdateClient([FromBody]ClientDTO client)
    {
        return new ClientResponseDTO();
    }
}
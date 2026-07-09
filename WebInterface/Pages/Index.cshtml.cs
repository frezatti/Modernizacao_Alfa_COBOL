using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebInterface.DTO;
using WebInterface.Service;

namespace WebInterface.Pages;

public class IndexModel : PageModel
{
    private readonly ClientCobolApi clientCobolApi;

    public IndexModel(ClientCobolApi clientCobolApi)
    {
        this.clientCobolApi = clientCobolApi;
    }

    [BindProperty]
    public string Id { get; set; } = "";

    [BindProperty]
    public string Number { get; set; } = "";

    [BindProperty]
    public string Email { get; set; } = "";

    public ClienteResponse? Resultado { get; set; }

    public string? Erro { get; set; }

    public void OnGet()
    {
    }

    public async Task OnPostConsultarAsync()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            Erro = "Informe o ID do cliente.";
            return;
        }

        Resultado = await clientCobolApi.ConsultarAsync(Id);

        if (Resultado?.Client is not null)
        {
            Number = Resultado.Client.Number;
            Email = Resultado.Client.Email;
        }
    }

    public async Task OnPostAtualizarAsync()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            Erro = "Informe o ID do cliente.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Number) || string.IsNullOrWhiteSpace(Email))
        {
            Erro = "Informe telefone e e-mail para atualizar o contato.";
            return;
        }

        Resultado = await clientCobolApi.AtualizarContatoAsync(
            Id,
            new AtualizarClienteDTO
            {
                Number = Number,
                Email = Email
            }
        );
    }
}

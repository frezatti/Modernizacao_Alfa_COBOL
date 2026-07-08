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
    public string Codigo { get; set; } = "";

    [BindProperty]
    public string Telefone { get; set; } = "";

    [BindProperty]
    public string Email { get; set; } = "";

    public ClienteResponse? Resultado { get; set; }

    public string? Erro { get; set; }

    public void OnGet()
    {
    }

    public async Task OnPostConsultarAsync()
    {
        if (string.IsNullOrWhiteSpace(Codigo))
        {
            Erro = "Informe o código do cliente.";
            return;
        }

        Resultado = await clientCobolApi.ConsultarAsync(Codigo);

        if (Resultado?.Cliente is not null)
        {
            Telefone = Resultado.Cliente.Telefone;
            Email = Resultado.Cliente.Email;
        }
    }

    public async Task OnPostAtualizarAsync()
    {
        if (string.IsNullOrWhiteSpace(Codigo))
        {
            Erro = "Informe o código do cliente.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Telefone) || string.IsNullOrWhiteSpace(Email))
        {
            Erro = "Informe telefone e e-mail para atualizar o contato.";
            return;
        }

        Resultado = await clientCobolApi.AtualizarContatoAsync(
            Codigo,
            new AtualizarClienteDTO
            {
                Telefone = Telefone,
                Email = Email
            }
        );
    }
}
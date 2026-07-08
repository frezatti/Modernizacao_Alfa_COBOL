using System.ComponentModel.DataAnnotations;

namespace zosproxy.DTO;

public class UpdateClientDTO
{
    [Required]
    [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Telefone deve conter 10 ou 11 números.")]
    public string Number { get; set; } = "";

    [Required]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = ""; 
}
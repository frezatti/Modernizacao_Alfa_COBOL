namespace WebInterface.DTO;

public class ClienteDto
{
    public int? Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Telefone { get; set; } = "";
    public string Email { get; set; } = "";
}

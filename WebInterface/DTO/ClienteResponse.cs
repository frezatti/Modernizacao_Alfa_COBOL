namespace WebInterface.DTO;

public class ClienteResponse
{
    public bool Sucesso { get; set; }
    public string CodigoRetorno { get; set; } = "";
    public string Mensagem { get; set; } = "";
    public ClienteDto? Cliente { get; set; }
}

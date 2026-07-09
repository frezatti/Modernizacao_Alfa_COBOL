namespace WebInterface.DTO;

public class ClienteResponse
{
    public bool Success { get; set; }
    public string ResponseCode { get; set; } = "";
    public string Message { get; set; } = "";
    public ClienteDto? Client { get; set; }
}

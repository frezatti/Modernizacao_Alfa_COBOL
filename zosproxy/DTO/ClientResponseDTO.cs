namespace zosproxy.DTO;

public class ClientResponseDTO
{
    public bool Success {get; set;}
    public string ResponseCode { get; set; } = "";
    public string Message { get; set; } = "";
    public ClientDTO? Client { get; set; }
}
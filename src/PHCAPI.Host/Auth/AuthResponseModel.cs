namespace PHCAPI.Host.Auth;

public class AuthResponseModel
{
    public string Token { get; set; } = string.Empty;
    public DateTime? Expiration { get; set; }
    public bool Allowed { get; set; }
    public string OutputResponse { get; set; } = string.Empty;
}

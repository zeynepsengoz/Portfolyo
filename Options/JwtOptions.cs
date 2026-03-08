namespace Portfolyo.Options;

public class JwtOptions
{
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string Secret { get; set; } = "";
    public int ExpiresMinutes { get; set; } = 120;
}
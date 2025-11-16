namespace WooliesX.Products.Application.Features.Auth.Login;

public class JwtOptions
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "WooliesX.Products";
    public string Audience { get; set; } = "WooliesX.Products.Audience";
    public int ExpiresMinutes { get; set; } = 60;
}

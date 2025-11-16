using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using WooliesX.Products.Application.Features.Auth.Login;


namespace WooliesX.Products.Api.Endpoints.Features.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth");

        group.MapGet("", (ClaimsPrincipal user) =>
        {
            var name = user.Identity?.Name ?? "anonymous";
            return Results.Ok(new { authenticated = user.Identity?.IsAuthenticated == true, user = name });
        })
        .RequireAuthorization()
        .WithName("CheckAuth");

        // Login endpoint: validates against configured BasicAuth credentials and returns a token
        group.MapPost("/login", (LoginRequest? req, IOptions<BasicAuthOptions> basic, IOptions<JwtOptions> jwtOptions, IValidator<LoginRequest> validator) =>
        {
            if (req is null)
            {
                throw new MissingBodyException();
            }

            var validation = validator.Validate(req);
            if (!validation.IsValid)
            {
                var dict = validation.Errors
                    .GroupBy(e => e.PropertyName ?? "request", StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray(), StringComparer.OrdinalIgnoreCase);
                throw new ValidationException(dict);
            }

            var expectedUser = basic.Value.Username;
            var expectedPass = basic.Value.Password;

            if (string.Equals(req.Username, expectedUser, StringComparison.Ordinal) &&
                string.Equals(req.Password, expectedPass, StringComparison.Ordinal))
            {
                var jwt = jwtOptions.Value;
                byte[] keyBytes;
                if (!string.IsNullOrWhiteSpace(jwt.Key))
                {
                    keyBytes = Encoding.UTF8.GetBytes(jwt.Key);
                }
                else
                {
                    // Derive a deterministic key from BasicAuth creds if Jwt:Key is not configured
                    using var sha = SHA256.Create();
                    var basis = $"{expectedUser}:{expectedPass}";
                    keyBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(basis));
                }

                var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
                var claims = new[]
                {
                    new System.Security.Claims.Claim(ClaimTypes.Name, req.Username)
                };
                var token = new JwtSecurityToken(
                    issuer: jwt.Issuer,
                    audience: jwt.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(jwt.ExpiresMinutes > 0 ? jwt.ExpiresMinutes : 60),
                    signingCredentials: creds);
                var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
                return Results.Ok(new { tokenType = "Bearer", accessToken });
            }

            return Results.Unauthorized();
        })
        .AllowAnonymous()
        .WithName("Login");

        return app;
    }
}

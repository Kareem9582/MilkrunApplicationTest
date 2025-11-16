using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WooliesX.Products.Application.Features.Auth.Login;
using Microsoft.IdentityModel.Tokens;
using WooliesX.Products.Infrastructure.Contracts;
using WooliesX.Products.Infrastructure.Persistance;
using WooliesX.Products.Application.Features.Products.Queries.GetProducts;
using WooliesX.Products.Application.Features.Products.Commands.CreateProduct;
using FluentValidation;
using AutoMapper;
using WooliesX.Products.Application.Features.Products.Mapping;

namespace WooliesX.Products.Api.Extensions;

public static class ServiceRegistrationExtensions
{
    public static WebApplicationBuilder AddAppServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(o =>
        {
            var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
            byte[] signingKeyBytes;

            var configuredKey = jwt.Key;
            if (!string.IsNullOrWhiteSpace(configuredKey))
            {
                signingKeyBytes = Encoding.UTF8.GetBytes(configuredKey);
            }
            else
            {
                // Derive a deterministic key from BasicAuth credentials if provided
                var user = builder.Configuration["BasicAuth:Username"] ?? string.Empty;
                var pass = builder.Configuration["BasicAuth:Password"] ?? string.Empty;
                if (string.IsNullOrEmpty(user) && string.IsNullOrEmpty(pass))
                {
                    throw new InvalidOperationException("Jwt:Key is not configured and cannot be derived. Set 'Jwt:Key' or provide BasicAuth credentials.");
                }
                using var sha = SHA256.Create();
                signingKeyBytes = sha.ComputeHash(Encoding.UTF8.GetBytes($"{user}:{pass}")); // 32 bytes
            }

            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = string.IsNullOrWhiteSpace(jwt.Issuer) ? null : jwt.Issuer,
                ValidAudience = string.IsNullOrWhiteSpace(jwt.Audience) ? null : jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes)
            };
        });

        builder.Services.AddAuthorization();
        builder.Services.AddSingleton<IProductsRepository, JsonSeededInMemoryProductsRepository>();
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetProductsQuery).Assembly));
        builder.Services.AddAutoMapper(typeof(ProductMappingProfile).Assembly);
        builder.Services.AddValidatorsFromAssemblyContaining<CreateProductCommandValidator>();

        return builder;
    }
}

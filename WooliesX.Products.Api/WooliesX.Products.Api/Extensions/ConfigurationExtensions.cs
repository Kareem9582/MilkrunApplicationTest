
using WooliesX.Products.Application.Features.Auth.Login;
using WooliesX.Products.Infrastructure.Persistance;

namespace WooliesX.Products.Api.Extensions;

public static class ConfigurationExtensions
{
    public static WebApplicationBuilder AddAppConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<DataOptions>(builder.Configuration.GetSection("Data"));
        builder.Services.Configure<BasicAuthOptions>(builder.Configuration.GetSection("BasicAuth"));
        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
        return builder;
    }
}

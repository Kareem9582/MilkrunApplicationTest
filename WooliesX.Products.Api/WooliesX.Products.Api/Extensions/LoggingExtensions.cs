using Serilog;

namespace WooliesX.Products.Api.Extensions;

public static class LoggingExtensions
{
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((ctx, services, cfg) =>
            cfg.ReadFrom.Configuration(ctx.Configuration)
               .ReadFrom.Services(services)
        );
        return builder;
    }
}

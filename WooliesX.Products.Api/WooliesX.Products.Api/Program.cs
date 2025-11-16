

var builder = WebApplication.CreateBuilder(args)
    .ConfigureLogging()
    .AddAppConfiguration()
    .AddIpRateLimiting()
    .AddAppServices();

var app = builder.Build()
    .UseAppMiddleware()
    .MapAppEndpoints();

app.Run();

public partial class Program { }

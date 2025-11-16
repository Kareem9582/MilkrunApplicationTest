using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace WooliesX.Products.Api.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            var sutRoot = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!; // tests/ProductsDemoApplication.Tests
            // App project folder is the parent of the tests folder in this repo layout
            var appProjectDir = Directory.GetParent(sutRoot.FullName)!.FullName;
            var productsPath = Path.Combine(appProjectDir, "Products.json");

            // Configure BasicAuth for tests via env or sensible defaults
            var envUser = Environment.GetEnvironmentVariable("BasicAuth__Username") ?? "test_user";
            var envPass = Environment.GetEnvironmentVariable("BasicAuth__Password") ?? "test_password";

            var jwtKey = Environment.GetEnvironmentVariable("Jwt__Key") ?? "dev-test-secret-key-please-change";

            var dict = new Dictionary<string, string?>
            {
                ["Data:ProductsPath"] = "Products.json",
                ["BasicAuth:Username"] = envUser,
                ["BasicAuth:Password"] = envPass,
                ["Jwt:Key"] = jwtKey,
                ["Jwt:Issuer"] = "ProductsDemoApplication",
                ["Jwt:Audience"] = "ProductsDemoApplicationAudience",
                ["Jwt:ExpiresMinutes"] = "60"
            };

            configBuilder.AddInMemoryCollection(dict!);
        });
    }
}

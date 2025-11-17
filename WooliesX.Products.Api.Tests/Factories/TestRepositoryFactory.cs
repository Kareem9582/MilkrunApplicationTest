using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WooliesX.Products.Infrastructure.Persistance;

namespace WooliesX.Products.Api.Tests.Factories;

public static class TestRepositoryFactory
{
    public static JsonSeededInMemoryProductsRepository CreateProductsRepository(string? jsonContent = null)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "ProductsRepoTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var jsonPath = Path.Combine(tempDir, "Products.json");
        if (jsonContent != null)
        {
            File.WriteAllText(jsonPath, jsonContent);
        }
        var options = Options.Create(new DataOptions { ProductsPath = jsonPath });
        var env = new TestHostEnvironment(Directory.GetCurrentDirectory());
        ILogger<JsonSeededInMemoryProductsRepository> logger = NullLogger<JsonSeededInMemoryProductsRepository>.Instance;
        return new JsonSeededInMemoryProductsRepository(options, env, logger);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }

        public TestHostEnvironment(string contentRootPath)
        {
            ContentRootPath = contentRootPath;
            ContentRootFileProvider = new PhysicalFileProvider(contentRootPath);
        }
    }
}

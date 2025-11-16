using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace WooliesX.Products.Api.Tests;

public class ProductsApiTests
{
    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    private static HttpClient CreateClient() => new CustomWebApplicationFactory().CreateClient();

    [Fact]
    public async Task GetProducts_ReturnsOkAndData()
    {
        var client = CreateClient();
        var resp = await client.GetAsync("/products");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await resp.Content.ReadAsStringAsync();
        json.Should().Contain("iPhone 9");
    }

    [Fact]
    public async Task GetProducts_FilterByCategory_Works()
    {
        var client = CreateClient();
        var resp = await client.GetAsync("/products?category=smartphones");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await resp.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        var items = doc.RootElement.GetProperty("items");
        items.GetArrayLength().Should().BeGreaterThan(0);
        foreach (var el in items.EnumerateArray())
        {
            el.GetProperty("category").GetString().Should().Be("smartphones");
        }
    }

    [Fact]
    public async Task GetProducts_Paging_Works()
    {
        var client = CreateClient();
        var r1 = await client.GetAsync("/products?page=1&pageSize=2");
        var r2 = await client.GetAsync("/products?page=2&pageSize=2");
        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
        var j1 = JsonDocument.Parse(await r1.Content.ReadAsStringAsync());
        var j2 = JsonDocument.Parse(await r2.Content.ReadAsStringAsync());
        j1.RootElement.GetProperty("items").GetArrayLength().Should().Be(2);
        j2.RootElement.GetProperty("items").GetArrayLength().Should().Be(2);
        var firstId = j1.RootElement.GetProperty("items")[0].GetProperty("id").GetInt32();
        var secondPageFirstId = j2.RootElement.GetProperty("items")[0].GetProperty("id").GetInt32();
        firstId.Should().NotBe(secondPageFirstId);
    }

    [Fact]
    public async Task GetProducts_SortByPriceDesc_Works()
    {
        var client = CreateClient();
        var resp = await client.GetAsync("/products?sortBy=price&order=desc&page=1&pageSize=5");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("items");
        items.GetArrayLength().Should().BeGreaterThan(1);
        var prev = decimal.MaxValue;
        foreach (var el in items.EnumerateArray())
        {
            var price = el.GetProperty("price").GetDecimal();
            price.Should().BeLessOrEqualTo(prev);
            prev = price;
        }
    }

    [Fact]
    public async Task GetProducts_SearchQ_Works()
    {
        var client = CreateClient();
        var resp = await client.GetAsync("/products?q=surface");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("items");
        items.EnumerateArray().Any(e => e.GetProperty("brand").GetString()!.ToLower().Contains("microsoft")).Should().BeTrue();
    }

    [Fact]
    public async Task GetProducts_MultiCategoryFilter_Works()
    {
        var client = CreateClient();
        var resp = await client.GetAsync("/products?category=laptops,smartphones&pageSize=100");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("items");
        items.GetArrayLength().Should().BeGreaterThan(0);
        foreach (var el in items.EnumerateArray())
        {
            var c = el.GetProperty("category").GetString();
            new[] { "laptops", "smartphones" }.Should().Contain(c);
        }
    }

    [Fact]
    public async Task GetProductById_ReturnsOk()
    {
        var client = CreateClient();
        var resp = await client.GetAsync("/products/1");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await resp.Content.ReadAsStringAsync();
        json.Should().Contain("iPhone 9");
    }

    [Fact]
    public async Task GetProductById_NotFound()
    {
        var client = CreateClient();
        var resp = await client.GetAsync("/products/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProductById_InvalidId_Returns400()
    {
        var client = CreateClient();
        var resp = await client.GetAsync("/products/-1");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var payload = Json("{\"title\":\"New Item\",\"price\":10}");
        var resp = await client.PostAsync("/products", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_InvalidRequest_Returns400()
    {
        var client = CreateClient();
        await AddBearerAuth(client);
        var payload = Json("{\"title\":\"\",\"price\":0}");
        var resp = await client.PostAsync("/products", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_WithoutBody_Returns400()
    {
        var client = CreateClient();
        await AddBearerAuth(client);
        var resp = await client.PostAsync("/products", content: null);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_ValidRequest_Returns201()
    {
        var client = CreateClient();
        await AddBearerAuth(client);
        var body = new { title = "Surface Pro 10 Test", description = "Latest model", price = 1599.00m, brand = "Microsoft", category = "laptops" };
        var payload = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var resp = await client.PostAsync("/products", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Post_DuplicateTitleBrand_Returns409()
    {
        var client = CreateClient();
        await AddBearerAuth(client);
        var body = new { title = "iPhone 9", description = "dup", price = 1m, brand = "Apple" };
        var payload = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var resp = await client.PostAsync("/products", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Put_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var body = new { title = "iPhone 9", description = "Update", price = 550m, brand = "Apple" };
        var payload = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var resp = await client.PutAsync("/products/1", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Put_InvalidRequest_Returns400()
    {
        var client = CreateClient();
        await AddBearerAuth(client);
        var body = new { title = "", description = new string('x', 101), price = 0m, brand = "Apple" };
        var payload = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var resp = await client.PutAsync("/products/1", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_WithoutBody_Returns400()
    {
        var client = CreateClient();
        await AddBearerAuth(client);
        var resp = await client.PutAsync("/products/1", content: null);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_NotFound_Returns404()
    {
        var client = CreateClient();
        await AddBearerAuth(client);
        var body = new { title = "DoesNotExist", description = "Update", price = 10m, brand = "NoBrand" };
        var payload = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var resp = await client.PutAsync("/products/999999", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_Duplicate_Returns409()
    {
        var client = CreateClient();
        await AddBearerAuth(client);
        // Update product 1 (iPhone 9, Apple) to duplicate product 2 (iPhone X, Apple)
        var body = new { title = "iPhone X", description = "dup", price = 549m, brand = "Apple" };
        var payload = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var resp = await client.PutAsync("/products/1", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Put_Valid_Returns200_AndUpdates()
    {
        var client = CreateClient();
        await AddBearerAuth(client);
        var body = new { title = "iPhone 9", description = "Updated desc", price = 550m, brand = "Apple" };
        var payload = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var resp = await client.PutAsync("/products/1", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var check = await client.GetAsync("/products/1");
        var json = await check.Content.ReadAsStringAsync();
        json.Should().Contain("\"price\":550");
        json.Should().Contain("Updated desc");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var client = CreateClient();
        var (user, pass) = GetBasicAuthCredentials();
        var body = new { username = user, password = pass };
        var payload = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var resp = await client.PostAsync("/auth/login", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        json.RootElement.GetProperty("tokenType").GetString().Should().Be("Bearer");
        json.RootElement.TryGetProperty("accessToken", out var tokenProp).Should().BeTrue();
        tokenProp.GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        var client = CreateClient();
        var body = new { username = "wrong", password = "creds" };
        var payload = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var resp = await client.PostAsync("/auth/login", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithoutBody_Returns400()
    {
        var client = CreateClient();
        var resp = await client.PostAsync("/auth/login", content: null);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Delete endpoint removed by request; tests omitted

    private static async Task AddBearerAuth(HttpClient client)
    {
        var (user, pass) = GetBasicAuthCredentials();
        var payload = new StringContent(JsonSerializer.Serialize(new { username = user, password = pass }), Encoding.UTF8, "application/json");
        var resp = await client.PostAsync("/auth/login", payload);
        resp.EnsureSuccessStatusCode();
        var token = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement.GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static (string Username, string Password) GetBasicAuthCredentials()
    {

        var envUser = Environment.GetEnvironmentVariable("BasicAuth__Username");
        var envPass = Environment.GetEnvironmentVariable("BasicAuth__Password");
        if (!string.IsNullOrWhiteSpace(envUser) && !string.IsNullOrWhiteSpace(envPass))
            return (envUser!, envPass!);

        return ("test_user", "test_password");
    }

    private static StringContent Json(string raw) => new(raw, Encoding.UTF8, "application/json");
}

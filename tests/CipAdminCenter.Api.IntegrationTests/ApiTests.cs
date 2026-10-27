using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CipAdminCenter.Api.IntegrationTests;

public class ApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<JsonElement> PostAsync(string url, object body, HttpStatusCode expected)
    {
        var response = await _client.PostAsJsonAsync(url, body);
        Assert.Equal(expected, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    [Fact]
    public async Task Health_endpoint_reports_healthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task OpenApi_document_is_served()
    {
        var document = await _client.GetFromJsonAsync<JsonElement>("/openapi/v1.json");

        Assert.True(document.GetProperty("paths").TryGetProperty("/api/v1/customers", out _));
    }

    [Fact]
    public async Task Customer_hierarchy_can_be_created_and_retrieved()
    {
        var customer = await PostAsync("/api/v1/customers", new { name = "Customer" }, HttpStatusCode.Created);
        var customerId = customer.GetProperty("id").GetGuid();
        var tenant = await PostAsync($"/api/v1/customers/{customerId}/tenants", new { name = "Tenant" }, HttpStatusCode.Created);
        var facility = await PostAsync($"/api/v1/tenants/{tenant.GetProperty("id").GetGuid()}/facilities", new { name = "Facility" }, HttpStatusCode.Created);
        await PostAsync($"/api/v1/facilities/{facility.GetProperty("id").GetGuid()}/units", new { name = "Unit" }, HttpStatusCode.Created);

        var fetched = await _client.GetFromJsonAsync<JsonElement>($"/api/v1/customers/{customerId}");

        Assert.Equal("Customer", fetched.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Invalid_requests_return_problem_details()
    {
        var invalid = await _client.PostAsJsonAsync("/api/v1/customers", new { name = "" });
        var missing = await _client.GetAsync($"/api/v1/customers/{Guid.NewGuid()}");
        var orphan = await _client.PostAsJsonAsync($"/api/v1/customers/{Guid.NewGuid()}/tenants", new { name = "T" });

        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, orphan.StatusCode);
    }

    [Fact]
    public async Task Availability_requires_an_entitlement_and_unknown_levels_are_rejected()
    {
        var customer = await PostAsync("/api/v1/customers", new { name = "C" }, HttpStatusCode.Created);
        var customerId = customer.GetProperty("id").GetGuid();
        var tenantId = (await PostAsync($"/api/v1/customers/{customerId}/tenants", new { name = "T" }, HttpStatusCode.Created)).GetProperty("id").GetGuid();
        var solutionId = (await PostAsync("/api/v1/solutions", new { code = "S1", name = "Solution" }, HttpStatusCode.Created)).GetProperty("id").GetGuid();
        var url = $"/api/v1/contexts/tenant/{tenantId}/solutions/{solutionId}/availability";

        Assert.Equal(HttpStatusCode.BadRequest, (await _client.PutAsync(url, null)).StatusCode);

        await PostAsync($"/api/v1/customers/{customerId}/entitlements",
            new { scopeKind = "Solution", solutionId, validFrom = "2020-01-01" }, HttpStatusCode.OK);

        Assert.Equal(HttpStatusCode.OK, (await _client.PutAsync(url, null)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await _client.PutAsync($"/api/v1/contexts/customer/{tenantId}/solutions/{solutionId}/availability", null)).StatusCode);
    }
}

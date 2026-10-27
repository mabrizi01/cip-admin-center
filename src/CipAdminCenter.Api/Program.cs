using System.Text.Json.Serialization;
using CipAdminCenter.Api;
using CipAdminCenter.Api.Endpoints;
using CipAdminCenter.Application;
using CipAdminCenter.Infrastructure;
using CipAdminCenter.Integration;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddIntegration();

builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddValidation();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

await app.Services.ApplyMigrationsIfConfiguredAsync();

app.MapOpenApi();
app.MapHealthChecks("/health");

var api = app.MapGroup("/api/v1");
api.MapOrganisationEndpoints();
api.MapCatalogueEndpoints();
api.MapContextEndpoints();

app.Run();

public partial class Program;

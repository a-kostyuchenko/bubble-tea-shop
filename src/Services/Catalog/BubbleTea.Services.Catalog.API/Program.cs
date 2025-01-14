using Asp.Versioning;
using Asp.Versioning.Builder;
using BubbleTea.Common.Application;
using BubbleTea.Common.Infrastructure;
using BubbleTea.Common.Infrastructure.Configuration;
using BubbleTea.Common.Presentation.Endpoints;
using BubbleTea.Services.Catalog.API;
using BubbleTea.Services.Catalog.API.Extensions;
using Scalar.AspNetCore;
using BubbleTea.ServiceDefaults;
using BubbleTea.Services.Catalog.API.Infrastructure.Database;
using BubbleTea.Services.Catalog.API.Infrastructure.Database.Constants;
using BubbleTea.Services.Catalog.API.OpenTelemetry;
using AssemblyReference = BubbleTea.Services.Catalog.API.AssemblyReference;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.AddServiceDefaults();

builder.Services.AddApplication([AssemblyReference.Assembly]);

string databaseConnection = builder.Configuration.GetConnectionStringOrThrow("catalog-db");
string redisConnection = builder.Configuration.GetConnectionStringOrThrow("cache");
string queueConnection = builder.Configuration.GetConnectionStringOrThrow("queue");

builder.Services.AddInfrastructure(
    DiagnosticsConfig.ServiceName,
    [],
    databaseConnection,
    redisConnection,
    queueConnection);

builder.AddDatabase<CatalogDbContext>("cart-db", Schemas.Catalog);

builder.AddAzureBlobClient("blobs");

builder.Services.AddCatalogModule(builder.Configuration);

WebApplication app = builder.Build();

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

RouteGroupBuilder versionedGroup = app
    .MapGroup("api/v{version:apiVersion}")
    .WithApiVersionSet(apiVersionSet);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    
    app.UseCors(policyBuilder => policyBuilder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
}

app.UseBackgroundJobs();

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.MapDefaultEndpoints();
app.MapEndpoints(versionedGroup);

await app.RunAsync();

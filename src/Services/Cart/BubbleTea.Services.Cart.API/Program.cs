using Asp.Versioning;
using Asp.Versioning.Builder;
using BubbleTea.Common.Application;
using BubbleTea.Common.Infrastructure;
using BubbleTea.Common.Infrastructure.Configuration;
using BubbleTea.Common.Presentation.Endpoints;
using BubbleTea.Services.Cart.API;
using BubbleTea.Services.Cart.API.Extensions;
using Scalar.AspNetCore;
using BubbleTea.ServiceDefaults;
using BubbleTea.Services.Cart.API.Infrastructure.Database;
using BubbleTea.Services.Cart.API.Infrastructure.Database.Constants;
using BubbleTea.Services.Cart.API.OpenTelemetry;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.AddServiceDefaults();

builder.Services.AddApplication([AssemblyReference.Assembly]);

string databaseConnection = builder.Configuration.GetConnectionStringOrThrow("cart-db");
string redisConnection = builder.Configuration.GetConnectionStringOrThrow("cache");
string queueConnection = builder.Configuration.GetConnectionStringOrThrow("queue");

builder.Services.AddInfrastructure(
    DiagnosticsConfig.ServiceName,
    [CartModule.ConfigureConsumers(redisConnection)],
    databaseConnection,
    redisConnection,
    queueConnection);

builder.AddDatabase<CartDbContext>("cart-db", Schemas.Cart);

builder.Services.AddCartModule(builder.Configuration);

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

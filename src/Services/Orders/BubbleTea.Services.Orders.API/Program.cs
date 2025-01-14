using Asp.Versioning;
using Asp.Versioning.Builder;
using BubbleTea.Common.Application;
using BubbleTea.Common.Infrastructure;
using BubbleTea.Common.Infrastructure.Configuration;
using BubbleTea.Common.Presentation.Endpoints;
using BubbleTea.Services.Orders.API;
using BubbleTea.Services.Orders.API.Extensions;
using Scalar.AspNetCore;
using BubbleTea.ServiceDefaults;
using BubbleTea.Services.Orders.API.Infrastructure.Database;
using BubbleTea.Services.Orders.API.Infrastructure.Database.Constants;
using BubbleTea.Services.Orders.API.OpenTelemetry;
using AssemblyReference = BubbleTea.Services.Orders.API.AssemblyReference;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.AddServiceDefaults();

builder.Services.AddApplication([AssemblyReference.Assembly]);

string databaseConnection = builder.Configuration.GetConnectionStringOrThrow("order-db");
string redisConnection = builder.Configuration.GetConnectionStringOrThrow("cache");
string queueConnection = builder.Configuration.GetConnectionStringOrThrow("queue");

builder.Services.AddInfrastructure(
    DiagnosticsConfig.ServiceName,
    [OrdersModule.ConfigureConsumers(redisConnection)],
    databaseConnection,
    redisConnection,
    queueConnection);

builder.AddDatabase<OrderingDbContext>("order-db", Schemas.Order);

builder.Services.AddOrdersModule(builder.Configuration);

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

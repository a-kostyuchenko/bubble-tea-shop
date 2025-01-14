using Asp.Versioning;
using Asp.Versioning.Builder;
using BubbleTea.Common.Application;
using BubbleTea.Common.Infrastructure;
using BubbleTea.Common.Infrastructure.Configuration;
using BubbleTea.Common.Presentation.Endpoints;
using BubbleTea.Services.Payment.API.Extensions;
using Scalar.AspNetCore;
using BubbleTea.ServiceDefaults;
using BubbleTea.Services.Payment.API.OpenTelemetry;
using BubbleTea.Services.Payment.Infrastructure;
using BubbleTea.Services.Payment.Infrastructure.Database;
using BubbleTea.Services.Payment.Infrastructure.Database.Constants;
using AssemblyReference = BubbleTea.Services.Payment.Application.AssemblyReference;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.AddServiceDefaults();

builder.Services.AddApplication([AssemblyReference.Assembly]);

string databaseConnection = builder.Configuration.GetConnectionStringOrThrow("payment-db");
string redisConnection = builder.Configuration.GetConnectionStringOrThrow("cache");
string queueConnection = builder.Configuration.GetConnectionStringOrThrow("queue");

builder.Services.AddInfrastructure(
    DiagnosticsConfig.ServiceName,
    [PaymentModule.ConfigureConsumers],
    databaseConnection,
    redisConnection,
    queueConnection);

builder.AddDatabase<PaymentDbContext>("payment-db", Schemas.Payment);

builder.Services.AddPaymentModule(builder.Configuration);

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddDocumentation();

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

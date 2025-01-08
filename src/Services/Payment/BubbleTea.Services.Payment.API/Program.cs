using Asp.Versioning;
using Asp.Versioning.Builder;
using BubbleTea.Services.Payment.API;
using BubbleTea.Services.Payment.API.Extensions;
using BubbleTea.Services.Payment.Infrastructure;
using Scalar.AspNetCore;
using BubbleTea.ServiceDefaults;
using BubbleTea.ServiceDefaults.Endpoints;
using BubbleTea.Services.Payment.Application;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.AddServiceDefaults();

builder.AddDatabase();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation();

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

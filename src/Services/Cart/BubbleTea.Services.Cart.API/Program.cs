using Asp.Versioning;
using Asp.Versioning.Builder;
using BubbleTea.Services.Cart.API;
using BubbleTea.Services.Cart.API.Extensions;
using Scalar.AspNetCore;
using BubbleTea.ServiceDefaults;
using BubbleTea.ServiceDefaults.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.AddDatabase();
builder.AddServiceDefaults();
builder.Services.AddCartServices(builder.Configuration);

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

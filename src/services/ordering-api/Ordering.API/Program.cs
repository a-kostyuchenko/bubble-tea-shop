using Asp.Versioning;
using Asp.Versioning.Builder;
using Ordering.API;
using Ordering.API.Extensions;
using Ordering.API.Infrastructure.Database;
using Scalar.AspNetCore;
using ServiceDefaults;
using ServiceDefaults.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.AddDatabase();
builder.Services.AddOrderingServices(builder.Configuration);
builder.AddServiceDefaults();

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
    app.ApplyMigrations<OrderingDbContext>();
}

app.UseBackgroundJobs();

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.MapDefaultEndpoints();
app.MapEndpoints(versionedGroup);

await app.RunAsync();

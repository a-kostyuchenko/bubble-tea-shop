using Asp.Versioning;
using Asp.Versioning.Builder;
using Catalog.API;
using Catalog.API.Database;
using Catalog.API.Database.Constants;
using Catalog.API.Extensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using ServiceDefaults;
using ServiceDefaults.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddServiceDefaults();

builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(AssemblyReference.Assembly);
});

builder.Services.AddValidatorsFromAssembly(AssemblyReference.Assembly, includeInternalTypes: true);

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpoints(AssemblyReference.Assembly);

builder.AddNpgsqlDbContext<CatalogDbContext>(
    "catalog-db",
    _ => {},
    optionsBuilder =>
    {
        optionsBuilder.UseSnakeCaseNamingConvention();

        optionsBuilder.UseNpgsql(contextOptionsBuilder =>
            contextOptionsBuilder.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Catalog));
    });

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
    app.UseSwagger();
    app.UseSwaggerUI();
    
    app.ApplyMigrations();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.MapDefaultEndpoints();
app.MapEndpoints(versionedGroup);

app.Run();

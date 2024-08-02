using Asp.Versioning;
using Asp.Versioning.Builder;
using Catalog.API.Database;
using Catalog.API.Database.Constants;
using Catalog.API.Extensions;
using Catalog.API.Outbox;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServiceDefaults;
using ServiceDefaults.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<CatalogDbContext>(
    "catalog-db",
    _ => {},
    optionsBuilder =>
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
        
        optionsBuilder.UseNpgsql(contextOptionsBuilder =>
            contextOptionsBuilder.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Catalog));
    });

builder.AddNpgsqlDataSource("catalog-db");

builder.Services.ConfigureServices(builder.Configuration);

builder.Services.TryAddSingleton<InsertOutboxMessagesInterceptor>();


builder.Services.AddMassTransit(configurator =>
{
    configurator.SetKebabCaseEndpointNameFormatter();
    
    configurator.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("queue")!);
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
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

app.UseBackgroundJobs();

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.MapDefaultEndpoints();
app.MapEndpoints(versionedGroup);

app.Run();

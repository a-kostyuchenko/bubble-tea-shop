using Catalog.API.Database;
using Catalog.API.Extensions;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<CatalogDbContext>(
    "catalog-db",
    _ => {},
    optionsBuilder => optionsBuilder.UseSnakeCaseNamingConvention());

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    app.ApplyMigrations();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.MapDefaultEndpoints();

app.Run();

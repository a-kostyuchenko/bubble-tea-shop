using AppHost.Extensions;
using Aspirant.Hosting;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresDatabaseResource> catalogDb = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin()
    .WithHealthCheck()
    .AddDatabase("catalog-db");

builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(catalogDb)
    .WaitFor(catalogDb);

builder.Build().Run();

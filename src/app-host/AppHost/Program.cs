IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresDatabaseResource> catalogDb = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin()
    .AddDatabase("catalog-db");

builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(catalogDb);

builder.Build().Run();

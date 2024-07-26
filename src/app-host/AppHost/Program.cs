IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Catalog_API>("catalog-api");

builder.Build().Run();

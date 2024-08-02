using AppHost.Extensions;
using Aspirant.Hosting;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> database = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin()
    .WithHealthCheck();
    
IResourceBuilder<PostgresDatabaseResource> catalogDb = database.AddDatabase("catalog-db");

IResourceBuilder<RabbitMQServerResource> queue = builder
    .AddRabbitMQ("queue")
    .WithHealthCheck()
    .WithManagementPlugin();


builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(catalogDb)
    .WithReference(queue)
    .WaitFor(catalogDb)
    .WaitFor(queue);

builder.Build().Run();

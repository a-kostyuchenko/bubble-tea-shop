using AppHost.Extensions;
using Aspirant.Hosting;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ParameterResource> pgUser = builder.AddParameter("DatabaseUser");
IResourceBuilder<ParameterResource> pgPassword = builder.AddParameter("DatabasePassword", secret: true);

IResourceBuilder<PostgresServerResource> databaseServer = builder
    .AddPostgres("database-server", pgUser, pgPassword)
    .WithDataVolume()
    .WithHealthCheck()
    .WithPgAdmin();
    
IResourceBuilder<PostgresDatabaseResource> catalogDb = databaseServer.AddDatabase("catalog-db");

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

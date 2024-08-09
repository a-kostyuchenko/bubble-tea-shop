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
IResourceBuilder<PostgresDatabaseResource> cartDb = databaseServer.AddDatabase("cart-db");
IResourceBuilder<PostgresDatabaseResource> orderDb = databaseServer.AddDatabase("order-db");

IResourceBuilder<RabbitMQServerResource> queue = builder
    .AddRabbitMQ("queue")
    .WithHealthCheck()
    .WithManagementPlugin();


builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(catalogDb)
    .WithReference(queue)
    .WaitFor(catalogDb)
    .WaitFor(queue);

builder.AddProject<Projects.Cart_API>("cart-api")
    .WithReference(cartDb)
    .WithReference(queue)
    .WaitFor(cartDb)
    .WaitFor(queue);

builder.AddProject<Projects.Ordering_API>("order-api")
    .WithReference(orderDb)
    .WithReference(queue)
    .WaitFor(orderDb)
    .WaitFor(queue);

builder.Build().Run();

using Aspire.Hosting.Azure;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ParameterResource> pgUsername = builder.AddParameter("Username", secret: true);
IResourceBuilder<ParameterResource> pgPassword = builder.AddParameter("Password", secret: true);

IResourceBuilder<AzurePostgresFlexibleServerResource> postgres = builder
    .AddAzurePostgresFlexibleServer("postgres")
    .WithPasswordAuthentication(pgUsername, pgPassword)
    .RunAsContainer(resourceBuilder =>
    {
        resourceBuilder
            .WithDataVolume()
            .WithPgAdmin()
            .WithLifetime(ContainerLifetime.Persistent);
    });
    

IResourceBuilder<AzureStorageResource> storage = builder
    .AddAzureStorage("storage")
    .RunAsEmulator(resourceBuilder => resourceBuilder
        .WithLifetime(ContainerLifetime.Persistent)
        .WithDataVolume());
    
IResourceBuilder<AzureBlobStorageResource> blobs = storage.AddBlobs("blobs");
    
IResourceBuilder<AzurePostgresFlexibleServerDatabaseResource> catalogDb = postgres.AddDatabase("catalog-db");
IResourceBuilder<AzurePostgresFlexibleServerDatabaseResource> cartDb = postgres.AddDatabase("cart-db");
IResourceBuilder<AzurePostgresFlexibleServerDatabaseResource> orderDb = postgres.AddDatabase("order-db");
IResourceBuilder<AzurePostgresFlexibleServerDatabaseResource> paymentDb = postgres.AddDatabase("payment-db");

IResourceBuilder<RabbitMQServerResource> queue = builder
    .AddRabbitMQ("queue")
    .WithManagementPlugin();

IResourceBuilder<RedisResource> cache = builder
    .AddRedis("cache")
    .WithRedisInsight()
    .WithDataVolume();

IResourceBuilder<ProjectResource> migrator = builder.AddProject<Projects.MigrationService>("migrator")
    .WithReference(catalogDb)
    .WithReference(cartDb)
    .WithReference(orderDb)
    .WithReference(paymentDb)
    .WaitFor(catalogDb)
    .WaitFor(cartDb)
    .WaitFor(orderDb)
    .WaitFor(paymentDb);

IResourceBuilder<ProjectResource> catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(catalogDb)
    .WithReference(queue)
    .WithReference(blobs)
    .WaitFor(catalogDb)
    .WaitFor(queue)
    .WaitForCompletion(migrator);

IResourceBuilder<ProjectResource> cartApi = builder.AddProject<Projects.Cart_API>("cart-api")
    .WithReference(cartDb)
    .WithReference(queue)
    .WithReference(cache)
    .WaitFor(cartDb)
    .WaitFor(queue)
    .WaitFor(cache)
    .WaitForCompletion(migrator);

IResourceBuilder<ProjectResource> orderingApi = builder.AddProject<Projects.Ordering_API>("ordering-api")
    .WithReference(orderDb)
    .WithReference(queue)
    .WithReference(cache)
    .WaitFor(orderDb)
    .WaitFor(queue)
    .WaitFor(cache)
    .WaitForCompletion(migrator);

IResourceBuilder<ProjectResource> paymentApi = builder.AddProject<Projects.Payment_API>("payment-api")
    .WithReference(paymentDb)
    .WithReference(queue)
    .WaitFor(paymentDb)
    .WaitFor(queue)
    .WaitForCompletion(migrator);

builder.AddProject<Projects.BubbleTeaShop_Gateway>("gateway")
    .WithExternalHttpEndpoints()
    .WithReference(catalogApi)
    .WithReference(cartApi)
    .WithReference(orderingApi)
    .WithReference(paymentApi)
    .WaitFor(catalogApi)
    .WaitFor(cartApi)
    .WaitFor(orderingApi)
    .WaitFor(paymentApi);

await builder.Build().RunAsync();

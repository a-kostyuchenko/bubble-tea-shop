using Aspire.Hosting.Azure;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ParameterResource> pgUsername = builder.AddParameter("Username");
IResourceBuilder<ParameterResource> pgPassword = builder.AddParameter("Password", secret: true);

IResourceBuilder<PostgresServerResource> postgres = builder
    .AddPostgres("postgres", pgUsername, pgPassword)
    .WithDataVolume()
    .WithPgAdmin();

IResourceBuilder<AzureStorageResource> storage = builder
    .AddAzureStorage("storage")
    .RunAsEmulator(resourceBuilder => resourceBuilder
        .WithLifetime(ContainerLifetime.Persistent)
        .WithDataVolume());
    
IResourceBuilder<AzureBlobStorageResource> blobs = storage.AddBlobs("blobs");
    
IResourceBuilder<PostgresDatabaseResource> catalogDb = postgres.AddDatabase("catalog-db");
IResourceBuilder<PostgresDatabaseResource> cartDb = postgres.AddDatabase("cart-db");
IResourceBuilder<PostgresDatabaseResource> orderDb = postgres.AddDatabase("order-db");
IResourceBuilder<PostgresDatabaseResource> paymentDb = postgres.AddDatabase("payment-db");

IResourceBuilder<RabbitMQServerResource> queue = builder
    .AddRabbitMQ("queue")
    .WithManagementPlugin();

IResourceBuilder<RedisResource> cache = builder
    .AddRedis("cache")
    .WithRedisInsight()
    .WithDataVolume();

IResourceBuilder<ProjectResource> migrator = builder.AddProject<Projects.BubbleTea_MigrationService>("migrator")
    .WithReference(catalogDb)
    .WithReference(cartDb)
    .WithReference(orderDb)
    .WithReference(paymentDb)
    .WaitFor(catalogDb)
    .WaitFor(cartDb)
    .WaitFor(orderDb)
    .WaitFor(paymentDb);

IResourceBuilder<ProjectResource> catalogApi = builder.AddProject<Projects.BubbleTea_Services_Catalog_API>("catalog-api")
    .WithReference(catalogDb)
    .WithReference(queue)
    .WithReference(blobs)
    .WithReference(cache)
    .WaitFor(catalogDb)
    .WaitFor(queue)
    .WaitFor(blobs)
    .WaitFor(catalogDb)
    .WaitForCompletion(migrator);

IResourceBuilder<ProjectResource> cartApi = builder.AddProject<Projects.BubbleTea_Services_Cart_API>("cart-api")
    .WithReference(cartDb)
    .WithReference(queue)
    .WithReference(cache)
    .WaitFor(cartDb)
    .WaitFor(queue)
    .WaitFor(cache)
    .WaitForCompletion(migrator);

IResourceBuilder<ProjectResource> orderingApi = builder.AddProject<Projects.BubbleTea_Services_Orders_API>("ordering-api")
    .WithReference(orderDb)
    .WithReference(queue)
    .WithReference(cache)
    .WaitFor(orderDb)
    .WaitFor(queue)
    .WaitFor(cache)
    .WaitForCompletion(migrator);

IResourceBuilder<ProjectResource> paymentApi = builder.AddProject<Projects.BubbleTea_Services_Payment_API>("payment-api")
    .WithReference(paymentDb)
    .WithReference(queue)
    .WithReference(cache)
    .WaitFor(paymentDb)
    .WaitFor(queue)
    .WaitFor(cache)
    .WaitForCompletion(migrator);

builder.AddProject<Projects.BubbleTea_Gateway>("gateway")
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

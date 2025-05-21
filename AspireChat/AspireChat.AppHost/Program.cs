//======================================================================
// ASPIRE CHAT APPLICATION HOST
//======================================================================
// This file defines the architecture of our entire application.
// The AppHost project orchestrates all services, databases, and
// infrastructure components that make up our application.

// Create the distributed application builder - this is the foundation
// of any Aspire application

var builder = DistributedApplication.CreateBuilder(args);

// Define the Azure Container App environment where our apps will be deployed
// This creates a logical group for our application components in Azure
var appHost = builder.AddAzureContainerAppEnvironment("aspire-chat");

//======================================================================
// INFRASTRUCTURE SERVICES
//======================================================================

// 1. REDIS CACHE
// Add Redis for caching, session storage, and pub/sub messaging
// This runs as a Docker container locally during development
var cache = builder.AddRedis("cache")
    .WithRedisInsight(); // Adds Redis Insight UI for easy cache inspection (local dev only)


// 2. SQL SERVER DATABASE
// Add SQL Server for persistent data storage
// Uses a Docker container for local development to avoid needing a real SQL Server
var sqlServer = builder.AddAzureSqlServer("sql")
    .RunAsContainer();

// Create a database instance on our SQL Server
// This will be used by our application for data storage
var database = sqlServer.AddDatabase("db");

// 3. AZURE STORAGE
// Add Azure Storage for storing files, blobs, and other unstructured data
// Uses the Azurite emulator for local development to simulate Azure Storage
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator();

// Enable blob storage specifically - we'll use this for file uploads
// This creates a blob container within our storage account
var blobStorage = storage.AddBlobs("blobs");

//======================================================================
// APP PARAMETERS AND SECRETS
//======================================================================
var jwtKey = builder.AddParameter("jwt-key", true);

//======================================================================
// APPLICATION COMPONENTS
//======================================================================

// 1. API SERVICE
// Add our backend API project that will provide data to our frontend
#pragma warning disable ASPIRECOMPUTE001
var api = builder.AddProject<Projects.AspireChat_Api>("api")
    //Add Secrets and Environment variables
    .WithEnvironment("JWT_KEY", jwtKey)

    // Connect the API to our infrastructure services
    // WithReference() gives the API connection info for the service
    // WaitFor() ensures the API won't start until these services are ready
    .WithReference(blobStorage).WaitFor(blobStorage)
    .WithReference(cache).WaitFor(cache)
    .WithReference(database).WaitFor(database)
    
    // Run on Azure Container Apps
    .WithComputeEnvironment(appHost);


// 2. WEB FRONTEND
// Add our web frontend project (Blazor app that users will interact with)
var web = builder.AddProject<Projects.AspireChat_Web>("web")
    // Make the web frontend publicly accessible when deployed
    .WithExternalHttpEndpoints()

    // Connect the web app to the services it needs
    // Note how the web app depends on both the cache AND the API
    .WithReference(cache).WaitFor(cache)
    .WithReference(api).WaitFor(api)
    
    // Run on Azure Container Apps
    .WithComputeEnvironment(appHost);

#pragma warning restore ASPIRECOMPUTE001
//======================================================================
// BUILD AND RUN
//======================================================================

// Build the application definition and run it
// This starts all the services in the correct order based on dependencies
builder.Build().Run();
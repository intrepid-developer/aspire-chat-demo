//======================================================================
// ASPIRE CHAT APPLICATION HOST
//======================================================================
// This file defines the architecture of our entire application.
// The AppHost project orchestrates all services, databases, and
// infrastructure components that make up our application.

// Create the distributed application builder - this is the foundation
// of any Aspire application
var builder = DistributedApplication.CreateBuilder(args);

//======================================================================
// DEPLOYMENT CONFIGURATION
//======================================================================

// Set up Azure as our deployment target using the Aspire CLI
// Note: This is in preview, so we disable the warning for now
#pragma warning disable ASPIREAZURE001
builder.AddAzurePublisher();
#pragma warning restore ASPIREAZURE001

// Define the Azure Container App environment where our apps will be deployed
// This creates a logical group for our application components in Azure
builder.AddAzureContainerAppEnvironment("aspire-chat");

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
var api = builder.AddProject<Projects.AspireChat_Api>("api")
    // Add a health check endpoint so Aspire can monitor the API's status
    .WithHttpsHealthCheck("/health")
    
    //Add Secrets and Evironment variables
    .WithEnvironment("JWT_KEY", jwtKey)
    
    // DEPLOYMENT NOTE:
    // When deployed to Azure, services are private by default
    // Uncomment the line below to make the API accessible from the internet
    //.WithExternalHttpEndpoints()
    
    // Connect the API to our infrastructure services
    // WithReference() gives the API connection info for the service
    // WaitFor() ensures the API won't start until these services are ready
    .WithReference(cache).WaitFor(cache)
    .WithReference(database).WaitFor(database);


// 2. WEB FRONTEND
// Add our web frontend project (Blazor app that users will interact with)
builder.AddProject<Projects.AspireChat_Web>("web")
    // Make the web frontend publicly accessible when deployed
    .WithExternalHttpEndpoints()
    // Add a health check endpoint for monitoring
    .WithHttpsHealthCheck("/health")
    // Connect the web app to the services it needs
    // Note how the web app depends on both the cache AND the API
    .WithReference(cache).WaitFor(cache)
    .WithReference(api).WaitFor(api);

//======================================================================
// BUILD AND RUN
//======================================================================

// Build the application definition and run it
// This starts all the services in the correct order based on dependencies
builder.Build().Run();
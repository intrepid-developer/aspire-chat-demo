//======================================================================
// ASPIRE CHAT APPLICATION HOST
//======================================================================
// This file defines the architecture of our entire application.
// The AppHost project orchestrates all services, databases, and
// infrastructure components that make up our application.

// Create the distributed application builder - this is the foundation
// of any Aspire application

using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

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
    .RunAsContainer(config => config.WithLifetime(ContainerLifetime.Persistent));

// Create a database instance on our SQL Server
// This will be used by our application for data storage
var database = sqlServer.AddDatabase("db");

// 3. AZURE STORAGE
// Add Azure Storage for storing files, blobs, and other unstructured data
// Uses the Azurite emulator for local development to simulate Azure Storage
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(config => config.WithLifetime(ContainerLifetime.Persistent));

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

//======================================================================
// CUSTOM RESOURCE COMMANDS
//======================================================================
// These commands appear in the Aspire dashboard (under the "..." actions for the resource)
// and are also exposed to the CLI (`aspire resource db seed-demo-data`) and to any
// MCP-capable AI agent connected via `aspire agent mcp`.
//
// This is the foundation for closing the feedback loop: an agent can discover these
// commands, understand what they do from the description, invoke them, and observe the
// structured result — all without a human pasting terminal output.

database.WithCommand(
    name: "seed-demo-data",
    displayName: "Seed Demo Data",
    executeCommand: async (ExecuteCommandContext context) =>
    {
        var connectionString = await ((IResourceWithConnectionString)database.Resource).GetConnectionStringAsync()
            ?? throw new InvalidOperationException("Unable to get the database connection string.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("demo123");

        const string insertUsers = @"
            IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
            BEGIN
                INSERT INTO Users (Name, Email, PasswordHash, CreatedAt, UpdatedAt)
                VALUES (@Name, @Email, @PasswordHash, GETUTCDATE(), GETUTCDATE());
            END";

        const string insertGroup = @"
            IF NOT EXISTS (SELECT 1 FROM Groups WHERE Name = 'Demo Team')
            BEGIN
                INSERT INTO Groups (Name, CreatedAt, UpdatedAt)
                VALUES ('Demo Team', GETUTCDATE(), GETUTCDATE());
            END
            SELECT Id FROM Groups WHERE Name = 'Demo Team';";

        await using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
        await connection.OpenAsync(context.CancellationToken);

        // Seed users
        foreach (var (name, email) in new[] { ("Alice Demo", "alice@demo.local"), ("Bob Demo", "bob@demo.local"), ("Charlie Demo", "charlie@demo.local") })
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = insertUsers;
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
            await cmd.ExecuteNonQueryAsync(context.CancellationToken);
            context.Logger.LogInformation("Ensured demo user exists: {Email}", email);
        }

        // Seed one group and capture its Id
        int groupId;
        await using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = insertGroup;
            var result = await cmd.ExecuteScalarAsync(context.CancellationToken);
            groupId = Convert.ToInt32(result);
        }

        // Seed a couple of chat messages in the demo group (using a real user id)
        const string insertChat = @"
            IF NOT EXISTS (SELECT 1 FROM Chats WHERE Message = @Message AND GroupId = @GroupId)
            BEGIN
                INSERT INTO Chats (Message, Name, UserId, GroupId, CreatedAt, UpdatedAt)
                VALUES (@Message, @Name, (SELECT Id FROM Users WHERE Email = @UserEmail), @GroupId, GETUTCDATE(), GETUTCDATE());
            END";

        var sampleMessages = new[]
        {
            ("Hello from the seeded demo data!", "Alice Demo", "alice@demo.local"),
            ("This is what the chat looks like with real messages.", "Bob Demo", "bob@demo.local")
        };

        foreach (var (msg, displayName, userEmail) in sampleMessages)
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = insertChat;
            cmd.Parameters.AddWithValue("@Message", msg);
            cmd.Parameters.AddWithValue("@Name", displayName);
            cmd.Parameters.AddWithValue("@UserEmail", userEmail);
            cmd.Parameters.AddWithValue("@GroupId", groupId);
            await cmd.ExecuteNonQueryAsync(context.CancellationToken);
        }

        return new ExecuteCommandResult
        {
            Success = true,
            Message = "Demo data seeded. You can now log in as alice@demo.local (or bob/charlie) with password 'demo123' and see the Demo Team group with messages."
        };
    },
    commandOptions: new CommandOptions
    {
        Description = "Seeds the database with three demo users, a 'Demo Team' group, and a couple of example chat messages so the application is immediately usable.",
        ConfirmationMessage = "Seed demo users, group and messages into the SQL database?",
        IconName = "DatabaseArrowUp"
    });

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
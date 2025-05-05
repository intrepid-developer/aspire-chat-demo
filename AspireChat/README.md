WIP BLOG NOT LIVE YET, SUBJECT TO CHANGE

# Aspire Chat Demo

A modern, real-time chat application built with .NET Aspire and Azure, designed to demonstrate cloud-native application development patterns. This project is part of a blog series exploring how to leverage .NET Aspire with Azure services.

## 🌟 Overview

This chat application showcases the power of .NET Aspire to simplify building distributed, cloud-native applications. It demonstrates how to integrate various Azure services with a modern .NET stack to create a scalable, resilient real-time application.

## 🏗️ Architecture

The application follows a microservices-inspired architecture with:

- **API Backend**: FastEndpoints-based API handling chat messages and user data
- **Web Frontend**: Blazor Server application providing the user interface
- **Infrastructure**: Redis for caching and pub/sub, SQL Server for data persistence, and Azure Storage for file attachments

![Architecture Diagram (placeholder)]()

## 🛠️ Technologies

- **.NET Aspire**: Cloud-ready stack for building observable, production-ready distributed applications
- **FastEndpoints**: High-performance API framework that promotes REPR pattern (Request-Endpoint-Response)
- **Blazor Server**: Interactive web UI framework using C# instead of JavaScript
- **Entity Framework Core**: ORM for data access and database operations
- **Redis**: In-memory data store for caching and pub/sub messaging
- **Azure SQL Database**: Managed relational database service
- **Azure Blob Storage**: Scalable object storage for images and file attachments
- **Azure Container Apps**: Hosting environment for containerized applications

## 📊 Project Structure

- **AspireChat.AppHost**: Orchestrates all services and infrastructure components
- **AspireChat.Api**: Backend API service using FastEndpoints
- **AspireChat.Web**: Blazor Server frontend application
- **AspireChat.ServiceDefaults**: Shared service configuration and defaults

## 🚀 Getting Started

### Prerequisites

- .NET 9 SDK
- Docker Desktop
- Visual Studio 2022 or later / VS Code with C# Dev Kit
- Azure subscription (for deployment)
- Aspire Cli

### Local Development

1. Clone the repository:
   ```
   git clone https://github.com/yourusername/aspire-chat-demo.git
   cd aspire-chat-demo
   ```

2. Install the Aspire Cli
    ```
    dotnet tool install --global aspire.cli --prerelease
    ```
3. Run the application with Aspire dashboard (from the AppHost directory):
   ```
   aspire run
   ```

4Navigate to the Aspire dashboard (automatically opened) to view and manage your services

### Deployment to Azure

The blog series will cover step-by-step deployment to Azure using:
- Azure Container Apps for hosting services
- Azure SQL Database for data persistence
- Redis Cache (docker)
- Azure Blob Storage for file storage

## ✨ Features

- Real-time messaging with SignalR
- User authentication and authorization
- File and image sharing
- Message threading and reactions
- Chat history and search
- User presence indicators

## 📝 Blog Series

This project is being built as part of a comprehensive blog series on using .NET Aspire with Azure. Topics covered include:

1. Introduction to .NET Aspire and project setup
2. Building the API with FastEndpoints
3. Creating the Blazor Server frontend
4. Adding real-time communication
5. Integrating Azure services (SQL, Redis, Storage)
6. Deploying to Azure Container Apps
7. Monitoring and observability
8. Performance tuning and scaling

Stay tuned for links to each blog post as they are published!

## 🔄 Development Roadmap

- [ ] Basic chat functionality
- [ ] User authentication
- [ ] File sharing capabilities
- [ ] Advanced UI features
- [ ] Azure deployment
- [ ] Monitoring and observability

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgements

- Microsoft .NET team for Aspire
- The FastEndpoints and Blazor teams
- MudBlazor team for the amazing UI components
- All contributors and the .NET community

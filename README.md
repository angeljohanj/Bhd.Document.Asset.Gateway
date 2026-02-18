# BHD Document Asset Gateway

This service acts as a proxy for managing document uploads and searching metadata. It orchestrates asynchronous uploads to an internal publisher.

## Business Case
- **Asynchronous Upload**: Receives files, persists metadata, and delegates upload to a background worker.
- **Search capabilities**: Allows searching documents based on metadata (Filename, Date, Status, etc.).
- **Hexagonal Architecture**: Logic is isolated from external dependencies.

## Technologies
- .NET 8 (ASP.NET Core Web API)
- Entity Framework Core with SQLite
- Background Workers (Hosted Services)
- Docker & Docker Compose
- xUnit & Moq for testing

## How to Run

### Option 1: Docker (Recommended)
The easiest way to run the application is using Docker Compose. This starts the API and sets up the environment automatically.

```bash
docker-compose up --build
```
The API will be available at: http://localhost:5000/swagger

### Option 2: Local .NET CLI
Ensure you have the .NET 8 SDK installed.

1. Build the solution:
   ```bash
   dotnet build
   ```
2. Run the API:
   ```bash
   dotnet run --project src/Gateway.Api/Gateway.Api.csproj
   ```
3. Launch Swagger UI at: http://localhost:5000/swagger

## Testing
To run all unit and integration tests:
```bash
dotnet test
```

## API Specification
The API follows the standard defined in `openapi.yml`.
- **POST** `/api/bhd/mgmt/1/documents/actions/upload`: Accepts a document and returns `202 Accepted`.
- **GET** `/api/bhd/mgmt/1/documents/`: Search and filter uploaded documents.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY *.sln .
COPY src/Gateway.Domain/*.csproj src/Gateway.Domain/
COPY src/Gateway.Application/*.csproj src/Gateway.Application/
COPY src/Gateway.Infrastructure/*.csproj src/Gateway.Infrastructure/
COPY src/Gateway.Api/*.csproj src/Gateway.Api/
COPY tests/Gateway.UnitTests/*.csproj tests/Gateway.UnitTests/
COPY tests/Gateway.IntegrationTests/*.csproj tests/Gateway.IntegrationTests/

RUN dotnet restore

COPY . .
RUN dotnet publish src/Gateway.Api/Gateway.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Gateway.Api.dll"]

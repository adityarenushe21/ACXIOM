# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["AcxiomCRM.sln", "./"]
COPY ["src/AcxiomCRM.Domain/AcxiomCRM.Domain.csproj", "src/AcxiomCRM.Domain/"]
COPY ["src/AcxiomCRM.Application/AcxiomCRM.Application.csproj", "src/AcxiomCRM.Application/"]
COPY ["src/AcxiomCRM.Infrastructure/AcxiomCRM.Infrastructure.csproj", "src/AcxiomCRM.Infrastructure/"]
COPY ["src/AcxiomCRM.Api/AcxiomCRM.Api.csproj", "src/AcxiomCRM.Api/"]
COPY ["tests/AcxiomCRM.UnitTests/AcxiomCRM.UnitTests.csproj", "tests/AcxiomCRM.UnitTests/"]
COPY ["tests/AcxiomCRM.IntegrationTests/AcxiomCRM.IntegrationTests.csproj", "tests/AcxiomCRM.IntegrationTests/"]

RUN dotnet restore "src/AcxiomCRM.Api/AcxiomCRM.Api.csproj"

# Copy full source and publish
COPY . .
WORKDIR "/src/src/AcxiomCRM.Api"
RUN dotnet publish "AcxiomCRM.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Run as non-root user (app user built into modern .NET images)
USER app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AcxiomCRM.Api.dll"]

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["OrkinosaiCMS.sln", "./"]
COPY ["src/OrkinosaiCMS.Web/OrkinosaiCMS.Web.csproj", "src/OrkinosaiCMS.Web/"]
COPY ["src/OrkinosaiCMS.Core/OrkinosaiCMS.Core.csproj", "src/OrkinosaiCMS.Core/"]
COPY ["src/OrkinosaiCMS.Infrastructure/OrkinosaiCMS.Infrastructure.csproj", "src/OrkinosaiCMS.Infrastructure/"]
COPY ["src/OrkinosaiCMS.Modules.Abstractions/OrkinosaiCMS.Modules.Abstractions.csproj", "src/OrkinosaiCMS.Modules.Abstractions/"]
COPY ["src/OrkinosaiCMS.Shared/OrkinosaiCMS.Shared.csproj", "src/OrkinosaiCMS.Shared/"]
COPY ["src/Modules/OrkinosaiCMS.Modules.Content/OrkinosaiCMS.Modules.Content.csproj", "src/Modules/OrkinosaiCMS.Modules.Content/"]

# Restore dependencies
RUN dotnet restore "OrkinosaiCMS.sln"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/OrkinosaiCMS.Web"
RUN dotnet build "OrkinosaiCMS.Web.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "OrkinosaiCMS.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create non-root user
RUN groupadd -r mosaic && useradd -r -g mosaic mosaic

# Copy published app
COPY --from=publish /app/publish .

# Set ownership
RUN chown -R mosaic:mosaic /app

# Switch to non-root user
USER mosaic

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "OrkinosaiCMS.Web.dll"]

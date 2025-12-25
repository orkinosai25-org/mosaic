# Isolated Mosaic CMS Dockerfile
# This dockerfile is for isolated deployment testing of Mosaic CMS

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["MosaicCMS.sln", "./"]
COPY ["src/MosaicCMS/MosaicCMS.csproj", "src/MosaicCMS/"]

# Restore dependencies
RUN dotnet restore "MosaicCMS.sln"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/MosaicCMS"
RUN dotnet build "MosaicCMS.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "MosaicCMS.csproj" -c Release -o /app/publish /p:UseAppHost=false

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

ENTRYPOINT ["dotnet", "MosaicCMS.dll"]

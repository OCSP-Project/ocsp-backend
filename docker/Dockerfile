FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/OCSP.API/OCSP.API.csproj", "src/OCSP.API/"]
COPY ["src/OCSP.Application/OCSP.Application.csproj", "src/OCSP.Application/"]
COPY ["src/OCSP.Domain/OCSP.Domain.csproj", "src/OCSP.Domain/"]
COPY ["src/OCSP.Infrastructure/OCSP.Infrastructure.csproj", "src/OCSP.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/OCSP.API/OCSP.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/OCSP.API"
RUN dotnet publish "/src/src/OCSP.API/OCSP.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set entry point
ENTRYPOINT ["dotnet", "OCSP.API.dll"]
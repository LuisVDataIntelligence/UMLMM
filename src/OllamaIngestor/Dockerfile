FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["UMLMM.sln", "./"]
COPY ["src/UMLMM.Core/UMLMM.Core.csproj", "src/UMLMM.Core/"]
COPY ["src/UMLMM.Infrastructure/UMLMM.Infrastructure.csproj", "src/UMLMM.Infrastructure/"]
COPY ["src/OllamaIngestor/OllamaIngestor.csproj", "src/OllamaIngestor/"]

# Restore dependencies
RUN dotnet restore "src/OllamaIngestor/OllamaIngestor.csproj"

# Copy source code
COPY ["src/", "src/"]

# Build
WORKDIR "/src/src/OllamaIngestor"
RUN dotnet build "OllamaIngestor.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "OllamaIngestor.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install Ollama CLI (optional - if you want to run Ollama in the container)
# RUN curl -fsSL https://ollama.com/install.sh | sh

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OllamaIngestor.dll"]

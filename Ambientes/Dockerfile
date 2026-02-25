# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copiar archivos de proyecto
COPY ["src/Ambientes.API/Ambientes.API.csproj", "Ambientes.API/"]
COPY ["src/Ambientes.Services/Ambientes.Services.csproj", "Ambientes.Services/"]
COPY ["src/Ambientes.Data/Ambientes.Data.csproj", "Ambientes.Data/"]

# Restaurar dependencias
RUN dotnet restore "Ambientes.API/Ambientes.API.csproj"

# Copiar código fuente completo
COPY ["src/", "."]

# Cambiar al directorio de la API y compilar
WORKDIR "/src/Ambientes.API"
RUN dotnet build "Ambientes.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Ambientes.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Instalar PostgreSQL client tools (opcional, útil para debugging)
RUN apt-get update && \
    apt-get install -y postgresql-client && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copiar los archivos publicados desde el stage anterior
COPY --from=publish /app/publish .

# Crear directorio para logs
RUN mkdir -p /app/logs

# Exponer puerto
EXPOSE 8080 8443

# Healthcheck
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD dotnet /app/health-check.dll || exit 1

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Comando de inicio
ENTRYPOINT ["dotnet", "Ambientes.API.dll"]

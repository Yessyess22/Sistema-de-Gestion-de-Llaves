# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar archivos de proyecto y restaurar dependencias
COPY src/SistemaGestionLlaves/*.csproj ./src/SistemaGestionLlaves/
RUN dotnet restore ./src/SistemaGestionLlaves/SistemaGestionLlaves.csproj

# Copiar el resto del código fuente
COPY src/ ./src/

# Publicar en modo Release
RUN dotnet publish ./src/SistemaGestionLlaves/SistemaGestionLlaves.csproj \
    -c Release \
    -o /app/publish

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar artefactos publicados
COPY --from=build /app/publish .

# Exponer puerto
EXPOSE 8080

# Punto de entrada
ENTRYPOINT ["dotnet", "SistemaGestionLlaves.dll"]

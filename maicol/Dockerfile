FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar csproj y restaurar dependencias
COPY ["SistemaWeb.csproj", "./"]
RUN dotnet restore "./SistemaWeb.csproj"

# Copiar todo y publicar
COPY . .
RUN dotnet publish "SistemaWeb.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "SistemaWeb.dll"]

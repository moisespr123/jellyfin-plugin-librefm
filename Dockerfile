FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder
WORKDIR /src

COPY *.sln .
COPY Jellyfin.Plugin.Libre/*.csproj ./Jellyfin.Plugin.Libre/
RUN dotnet restore

COPY . .

# Publish the plugin in Release configuration
RUN dotnet publish "Jellyfin.Plugin.Libre/Jellyfin.Plugin.Libre.csproj" -c Release -o /app/publish

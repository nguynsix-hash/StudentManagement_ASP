FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ConnectDB/ConnectDB.csproj", "ConnectDB/"]
RUN dotnet restore "ConnectDB/ConnectDB.csproj"
COPY . .
WORKDIR "/src/ConnectDB"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV USE_INMEMORY_DB=true
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

ENTRYPOINT ["sh", "-c", "dotnet ConnectDB.dll --urls http://0.0.0.0:${PORT:-8080}"]

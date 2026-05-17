FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LogServer.csproj", "./"]
RUN dotnet restore "./LogServer.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "LogServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LogServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LogServer.dll"]

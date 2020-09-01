FROM mcr.microsoft.com/dotnet/core/runtime:3.1.7-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /src
COPY ["Rooster.App/Rooster.App.csproj", "Rooster.App/"]
COPY ["Rooster.DependencyInjection/Rooster.DependencyInjection.csproj", "Rooster.DependencyInjection/"]
COPY ["Rooster.SqlServer/Rooster.SqlServer.csproj", "Rooster.SqlServer/"]
COPY ["Rooster/Rooster.csproj", "Rooster/"]
COPY ["Rooster.MongoDb/Rooster.MongoDb.csproj", "Rooster.MongoDb/"]
COPY ["Rooster.Slack/Rooster.Slack.csproj", "Rooster.Slack/"]
RUN dotnet restore "Rooster.App/Rooster.App.csproj"
COPY . .
WORKDIR "/src/Rooster.App"
RUN dotnet build "Rooster.App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Rooster.App.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rooster.App.dll"]

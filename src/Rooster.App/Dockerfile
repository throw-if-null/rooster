FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
ADD ./Rooster.App/VERSION .
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src
COPY ["Rooster.App/Rooster.App.csproj", "Rooster.App/"]
COPY ["Rooster.SqlServer/Rooster.SqlServer.csproj", "Rooster.SqlServer/"]
COPY ["Rooster/Rooster.csproj", "Rooster/"]
COPY ["Rooster.MongoDb/Rooster.MongoDb.csproj", "Rooster.MongoDb/"]
COPY ["Rooster.Mock/Rooster.Mock.csproj", "Rooster.Mock/"]
COPY ["Rooster.AppInsights/Rooster.AppInsights.csproj", "Rooster.AppInsights/"]
COPY ["Rooster.Slack/Rooster.Slack.csproj", "Rooster.Slack/"]
COPY ["Rooster.HealthCheck/Rooster.HealthCheck.csproj", "Rooster.HealthCheck/"]
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

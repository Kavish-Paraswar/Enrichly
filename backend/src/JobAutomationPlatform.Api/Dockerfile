FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
WORKDIR /src/src/JobAutomationPlatform.Api
RUN dotnet restore JobAutomationPlatform.Api.csproj
RUN dotnet publish JobAutomationPlatform.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "JobAutomationPlatform.Api.dll"]

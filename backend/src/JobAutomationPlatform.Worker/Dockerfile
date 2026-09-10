FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
WORKDIR /src/src/JobAutomationPlatform.Worker
RUN dotnet restore JobAutomationPlatform.Worker.csproj
RUN dotnet publish JobAutomationPlatform.Worker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "JobAutomationPlatform.Worker.dll"]

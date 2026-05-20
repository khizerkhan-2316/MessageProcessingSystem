FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /repo

COPY . .

RUN dotnet publish "src/MessageProcessingSystem.Consumer/MessageProcessingSystem.Consumer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MessageProcessingSystem.Consumer.dll"]
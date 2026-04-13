FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY EventDrivenCommon/EventDrivenCommon.csproj EventDrivenCommon/
COPY EventDrivenApp/EventDrivenApp.csproj EventDrivenApp/
RUN dotnet restore EventDrivenApp/EventDrivenApp.csproj

COPY EventDrivenCommon/ EventDrivenCommon/
COPY EventDrivenApp/ EventDrivenApp/
RUN dotnet publish EventDrivenApp/EventDrivenApp.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "EventDrivenApp.dll"]

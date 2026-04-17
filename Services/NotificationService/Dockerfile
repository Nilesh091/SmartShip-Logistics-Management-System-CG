# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY . .
RUN dotnet restore ./Services/NotificationService/NotificationService.API/NotificationService.API.csproj
RUN dotnet publish ./Services/NotificationService/NotificationService.API/NotificationService.API.csproj -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "NotificationService.API.dll"]

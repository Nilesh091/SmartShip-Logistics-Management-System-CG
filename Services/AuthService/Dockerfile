# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY . .
RUN dotnet restore ./Services/AuthService/AuthService.API/AuthService.API.csproj
RUN dotnet publish ./Services/AuthService/AuthService.API/AuthService.API.csproj -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "AuthService.API.dll"]

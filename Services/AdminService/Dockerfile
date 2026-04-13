# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY . .
RUN dotnet restore ./Services/AdminService/AdminService.API/AdminService.API.csproj
RUN dotnet publish ./Services/AdminService/AdminService.API/AdminService.API.csproj -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "AdminService.API.dll"]

# SmartShip Gateway - Setup & Debugging Guide

## Overview

The SmartShip Gateway is an API Gateway built with **Ocelot** that aggregates requests from multiple microservices and provides unified Swagger documentation via **SwaggerForOcelot**.

## Architecture

### Gateway Configuration

- **Port**: `5166` (HTTP) / `7102` (HTTPS)
- **Framework**: .NET 10.0
- **Primary Features**:
  - API routing via Ocelot
  - JWT Bearer token authentication
  - CORS support
  - Swagger UI for Gateway + Aggregated service documentation

### Microservices Routing

| Service          | Upstream Path | Port | Downstream                     |
| ---------------- | ------------- | ---- | ------------------------------ |
| Auth Service     | `/auth`       | 5045 | `localhost:5045/api/auth`      |
| Admin Service    | `/admin`      | 5127 | `localhost:5127/api/admin`     |
| Tracking Service | `/tracking`   | 5062 | `localhost:5062/api/tracking`  |
| Shipment Service | `/shipments`  | 5286 | `localhost:5286/api/shipments` |

## Features Configured

### ✅ 1. JWT Authentication

- **Scheme**: Bearer tokens
- **Validation**: Issuer, Audience, Lifetime, Signing Key
- **Configuration**: `appsettings.json` (JWT settings)
- **Protected Routes**: `/admin/*`, `/tracking/*`, `/shipments/*`
- **Public Route**: `/auth/*` (for login/token generation)

### ✅ 2. CORS

- **Policy**: AllowAll (for development)
- Allows all origins, methods, and headers

### ✅ 3. Swagger Documentation

- **Gateway Swagger**: `http://localhost:5166/swagger`
- **Aggregated Services**: `http://localhost:5166/swagger/docs`
- **Services Exposed**:
  - `auth.json` → Auth Service
  - `admin.json` → Admin Service
  - `tracking.json` → Tracking Service
  - `shipments.json` → Shipment Service

### ✅ 4. Middleware Pipeline

1. CORS
2. Routing
3. Authentication
4. Authorization
5. Ocelot (Gateway routing)

## Files Modified/Created

### Gateway.API/Program.cs

✅ Added JWT Bearer authentication with configuration-driven secrets
✅ Added Ocelot middleware
✅ Added SwaggerForOcelot aggregation
✅ Configured middleware pipeline with proper ordering

### Gateway.API/appsettings.json

✅ Added JWT configuration section (Issuer, Key, Audience)
✅ Made secrets externalized from hardcoded values

### Gateway.API/appsettings.Development.json

✅ Updated with Debug logging for Ocelot
✅ Added development-specific JWT settings

### Gateway.API/ocelot.json

✅ Added routes to expose service Swagger endpoints
✅ Routes for `/swagger/docs/` endpoints for SwaggerForOcelot aggregation

### Gateway.API/Gateway.API.csproj

✅ Added `Microsoft.OpenApi` package
✅ Added `MMLib.SwaggerForOcelot` package
✅ Added `Swashbuckle.AspNetCore` package
✅ Added `Microsoft.AspNetCore.Authentication.JwtBearer` package

## Setup Instructions

### 1. Install Dependencies

```bash
cd Gateway/Gateway.API
dotnet restore
```

### 2. Configuration

Update `appsettings.json` and `appsettings.Development.json` with real JWT secrets:

```json
"Jwt": {
  "Key": "your-min-32-character-secret-key",
  "Issuer": "SmartShipGateway",
  "Audience": "SmartShipAPI"
}
```

### 3. Run Services

Ensure all microservices are running on their configured ports:

```bash
# Terminal 1: Auth Service
cd Services/AuthService/AuthService.API
dotnet run --launch-profile http

# Terminal 2: Admin Service
cd Services/AdminService/AdminService.API
dotnet run

# Terminal 3: Tracking Service
cd Services/TrackingService/TrackingService.API
dotnet run

# Terminal 4: Shipment Service
cd Services/ShipmentService/ShipmentService.API
dotnet run
```

### 4. Start Gateway

```bash
cd Gateway/Gateway.API
dotnet run --launch-profile http
```

### 5. Access Endpoints

**Gateway Swagger UI** (single service):

```
http://localhost:5166/swagger
```

**Aggregated Swagger** (all services):

```
http://localhost:5166/swagger/docs
```

**Health Check/Test**:

```bash
# Login (public endpoint)
curl -X POST http://localhost:5166/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"test","password":"test"}'

# Get token, then use for protected endpoints
curl -X GET http://localhost:5166/admin/users \
  -H "Authorization: Bearer {token}"
```

## Debugging

### Issue: Service Not Found

- Check if service is running on configured port
- Verify `ocelot.json` port mappings
- Check firewall settings

### Issue: JWT Token Invalid

- Ensure JWT Key is consistent across Gateway and Auth Service
- Verify token hasn't expired
- Check Issuer and Audience match configuration

### Issue: Swagger Not Aggregating

- Verify each service has Swagger enabled
- Check ocelot.json routes for `/swagger/v1/swagger.json`
- Ensure services return proper OpenAPI specs

### Logs

Enable debug logging in `appsettings.Development.json`:

```json
"Logging": {
  "LogLevel": {
    "Ocelot": "Debug",
    "Microsoft.AspNetCore": "Debug"
  }
}
```

## Security Notes

⚠️ **Development Only**:

- CORS: AllowAll (restrict in production)
- HTTPS: Optional in development
- JWT Key: Change from default value

🔒 **Production Checklist**:

- Use environment variables for JWT secrets
- Enable HTTPS only
- Restrict CORS origins
- Implement rate limiting
- Add request logging/monitoring
- Use API keys or OAuth2 for service-to-service auth

## Next Steps

1. Implement service-to-service authentication (if needed)
2. Add rate limiting policies
3. Implement request/response logging
4. Add health check endpoints
5. Configure API versioning
6. Add request validation middleware

# SmartShip Logistics Management System - Swagger Endpoints Analysis

**Last Updated:** April 10, 2026  
**Project:** SmartShip Gateway with Microservices Architecture

---

## Overview

The SmartShip system uses a microservices architecture with an API Gateway (Ocelot) that routes requests to downstream services. Each service has Swagger/OpenAPI documentation enabled, and the Gateway now includes comprehensive Swagger documentation.

### Base URLs

- **Gateway:** `http://localhost:5166`
- **Auth Service:** `http://localhost:5045`
- **Admin Service:** `http://localhost:5127`
- **Tracking Service:** `http://localhost:5062`
- **Shipment Service:** `http://localhost:5286`

---

## Gateway API Routes

### Swagger Documentation Access

- **Gateway Swagger UI:** `http://localhost:5166/swagger/index.html` or `http://localhost:5166/`
- **Gateway Swagger JSON:** `http://localhost:5166/swagger/v1/swagger.json`

### Routing Configuration (Ocelot)

The Gateway routes requests as follows:

| Gateway Path   | Downstream Service | Port |
| -------------- | ------------------ | ---- |
| `/auth/*`      | Auth Service       | 5045 |
| `/admin/*`     | Admin Service      | 5127 |
| `/tracking/*`  | Tracking Service   | 5062 |
| `/shipments/*` | Shipment Service   | 5286 |

---

## Service Endpoints

### 1. Authentication Service (Auth Service)

**Swagger URL:** `http://localhost:5045/swagger/index.html`

#### Key Endpoints:

- **POST** `/api/auth/signup` - Register a new user
  - Request: Username, Email, Password, (optional) Role
  - Response: Success message

- **POST** `/api/auth/login` - Authenticate user
  - Request: Email, Password
  - Response: Access Token, Refresh Token

- **POST** `/api/auth/refresh` - Get new access token
  - Query Param: `token` (refresh token)
  - Response: New Access Token

- **POST** `/api/auth/revoke` - Invalidate refresh token
  - Query Param: `token` (refresh token)
  - Response: Success message

- **GET** `/api/auth/admin/users` - Get all users (ADMIN only)
  - Auth: Bearer Token (ADMIN role required)
  - Response: List of users

- **PUT** `/api/auth/admin/users/{userId}` - Update user role (ADMIN only)
  - Auth: Bearer Token (ADMIN role required)
  - Request: New role
  - Response: Updated user

#### Via Gateway:

- `POST /auth/signup`
- `POST /auth/login`
- `POST /auth/refresh?token={token}`
- `POST /auth/revoke?token={token}`
- `GET /auth/admin/users` (with Bearer token)
- `PUT /auth/admin/users/{userId}` (with Bearer token)

#### Security:

- **JWT Bearer Token** for protected endpoints
- Token validation includes: Issuer, Audience, Signature, Expiry

---

### 2. Admin Service

**Swagger URL:** `http://localhost:5127/swagger/index.html`

#### Key Endpoints:

- **GET** `/api/Admin/dashboard` - Get admin monitoring dashboard (ADMIN only)
  - Auth: Bearer Token (ADMIN role required)
  - Response: Dashboard metrics and statistics

- **GET** `/api/admin/shipments/exceptions` - Get exception shipments (ADMIN only)
  - Auth: Bearer Token (ADMIN role required)
  - Response: List of delayed, failed, or stuck shipments

- **GET** `/api/admin/shipments` - Get all shipments (ADMIN only)
  - Auth: Bearer Token (ADMIN role required)
  - Response: Complete shipment list

- **PUT** `/api/admin/shipments/{shipmentId}/resolve` - Resolve shipment exception (ADMIN only)
  - Auth: Bearer Token (ADMIN role required)
  - Path Param: `shipmentId` (UUID)
  - Response: Updated shipment

- **GET** `/api/admin/users` - Get all users (ADMIN only)
  - Auth: Bearer Token (ADMIN role required)
  - Response: List of users with roles and status

- **PUT** `/api/admin/users/{userId}` - Update user role/status (ADMIN only)
  - Auth: Bearer Token (ADMIN role required)
  - Path Param: `userId` (UUID)
  - Request: `{ "role": "CUSTOMER|ADMIN", "status": "ACTIVE|INACTIVE" }`
  - Response: Updated user

- **GET** `/api/admin/reports` - Get analytics and statistics (ADMIN only)
  - Auth: Bearer Token (ADMIN role required)
  - Response: System reports

#### Via Gateway:

- `GET /admin/dashboard` (with Bearer token)
- `GET /admin/shipments/exceptions` (with Bearer token)
- `GET /admin/shipments` (with Bearer token)
- `PUT /admin/shipments/{shipmentId}/resolve` (with Bearer token)
- `GET /admin/users` (with Bearer token)
- `PUT /admin/users/{userId}` (with Bearer token)
- `GET /admin/reports` (with Bearer token)

#### Security:

- **JWT Bearer Token** required
- **ADMIN role** verification via claims

---

### 3. Tracking Service

**Swagger URL:** `http://localhost:5062/swagger/index.html`

#### Key Endpoints:

- **GET** `/api/Tracking/{trackingId}` - Get tracking details
  - Path Param: `trackingId` (UUID or tracking number)
  - Auth: Bearer Token (optional for public tracking)
  - Response: Shipment tracking information

- **GET** `/api/Tracking/shipment/{shipmentId}` - Get all tracking records for a shipment
  - Path Param: `shipmentId` (UUID)
  - Auth: Bearer Token
  - Response: List of tracking updates

- **POST** `/api/Tracking` - Create new tracking record
  - Auth: Bearer Token
  - Request: Shipment metadata
  - Response: Created tracking record

- **PUT** `/api/Tracking/{trackingId}` - Update tracking status
  - Auth: Bearer Token
  - Path Param: `trackingId`
  - Request: New status, location, timestamp
  - Response: Updated tracking record

#### Via Gateway:

- `GET /tracking/{trackingId}`
- `GET /tracking/shipment/{shipmentId}`
- `POST /tracking`
- `PUT /tracking/{trackingId}`

#### Security:

- **JWT Bearer Token** (admin/customer based on role)
- Public tracking available for certain endpoints

---

### 4. Shipment Service

**Swagger URL:** `http://localhost:5286/swagger/index.html`

#### Key Endpoints:

- **GET** `/api/shipments` - List all shipments
  - Auth: Bearer Token
  - Query Params: `skip`, `take` (pagination)
  - Response: Paginated shipment list

- **GET** `/api/shipments/{shipmentId}` - Get shipment details
  - Auth: Bearer Token
  - Path Param: `shipmentId` (UUID)
  - Response: Complete shipment information

- **POST** `/api/shipments` - Create new shipment
  - Auth: Bearer Token
  - Request: Shipment details, recipient info, items
  - Response: Created shipment with ID

- **PUT** `/api/shipments/{shipmentId}` - Update shipment
  - Auth: Bearer Token
  - Path Param: `shipmentId`
  - Request: Updated shipment details
  - Response: Updated shipment

- **DELETE** `/api/shipments/{shipmentId}` - Cancel shipment
  - Auth: Bearer Token
  - Path Param: `shipmentId`
  - Response: Success message

#### Via Gateway:

- `GET /shipments`
- `GET /shipments/{shipmentId}`
- `POST /shipments`
- `PUT /shipments/{shipmentId}`
- `DELETE /shipments/{shipmentId}`

#### Security:

- **JWT Bearer Token** required
- **CUSTOMER role** for personal shipments
- **ADMIN role** for all shipments

---

## Swagger Security Configuration

### JWT Bearer Scheme (All Services & Gateway)

```json
{
  "type": "http",
  "scheme": "bearer",
  "bearerFormat": "JWT",
  "description": "Enter 'Bearer' [space] and then your valid token"
}
```

### Token Structure

```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "email@example.com",
    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "CUSTOMER|ADMIN",
    "UserId": "uuid",
    "exp": 1775796330,
    "iss": "AuthService",
    "aud": "AuthServiceAPI"
  }
}
```

---

## Sample Request/Response Flows

### Flow 1: User Authentication via Gateway

```bash
# 1. Login through Gateway
POST http://localhost:5166/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}

# Response
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 3600
}

# 2. Create shipment (using token from step 1)
POST http://localhost:5166/shipments
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Content-Type: application/json

{
  "recipientName": "John Doe",
  "recipientEmail": "john@example.com",
  "recipientPhone": "+1234567890",
  "deliveryAddress": "123 Main St, City, State 12345"
}

# Response
{
  "id": "d425bdd7-1399-4ecc-8940-3a85e6909c0b",
  "status": "PENDING",
  "createdAt": "2026-04-10T10:30:00Z"
}

# 3. Track shipment
GET http://localhost:5166/tracking/d425bdd7-1399-4ecc-8940-3a85e6909c0b
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...

# Response
{
  "shipmentId": "d425bdd7-1399-4ecc-8940-3a85e6909c0b",
  "status": "PENDING",
  "currentLocation": "Distribution Center - City, State",
  "lastUpdate": "2026-04-10T10:35:00Z"
}
```

### Flow 2: Admin Dashboard Access

```bash
# 1. Login as admin
POST http://localhost:5166/auth/login
Content-Type: application/json

{
  "email": "admin@smartship.com",
  "password": "Admin@123"
}

# Response includes admin token with ADMIN role

# 2. Access admin dashboard
GET http://localhost:5166/admin/dashboard
Authorization: Bearer <ADMIN_TOKEN>

# Response
{
  "totalShipments": 1250,
  "activeShipments": 450,
  "delayedShipments": 23,
  "failedShipments": 5,
  "systemHealth": "HEALTHY",
  "averageDeliveryTime": "2.5 days"
}

# 3. Check exceptions
GET http://localhost:5166/admin/shipments/exceptions
Authorization: Bearer <ADMIN_TOKEN>

# Response
{
  "exceptions": [
    {
      "shipmentId": "123e4567-e89b-12d3-a456-426614174000",
      "status": "DELAYED",
      "reason": "Weather condition at origin",
      "createdAt": "2026-04-09T14:30:00Z"
    }
  ]
}
```

---

## Gateway Swagger Additions

The Gateway now provides a unified Swagger UI at `http://localhost:5166/` that documents:

✅ **JWT Bearer Authentication** - Configured to support all protected endpoints  
✅ **Routing Information** - Clear documentation of downstream service routes  
✅ **Security Schemes** - Global JWT security requirement  
✅ **Gateway Metadata** - Version, description, and support contact

### Accessing Gateway Swagger

1. **UI:** Navigate to `http://localhost:5166/`
2. **JSON:** Available at `http://localhost:5166/swagger/v1/swagger.json`
3. **Development Only** - Swagger is disabled in Production builds

---

## Configuration Files

### Gateway Configuration

- **Program.cs** - Swagger setup with JWT scheme
- **ocelot.json** - Route definitions to downstream services
- **appsettings.json** - JWT and connection settings

### Service Configurations

Each service has similar swagger setup in their respective `Program.cs`:

- JWT security definition
- Bearer token configuration
- API metadata and versioning

---

## Integration Notes

- The Gateway acts as a **single entry point** for all API requests
- Each service maintains **independent Swagger documentation**
- Direct service access: Use service base URLs for direct API calls
- Gateway access: Use gateway URL with routed paths
- All services use **consistent JWT validation** for security
- Swagger UI available only in **Development environment**

---

## Future Enhancements

1. **API Gateway Swagger Aggregation** - Consider tools like Swashbuckle.AspNetCore.SwaggerGen to aggregate all downstream service specs
2. **API Versioning** - Implement API version management across services
3. **Rate Limiting Documentation** - Add rate limit documentation to Swagger
4. **Authentication Flow Diagrams** - Add OpenAPI examples for OAuth2 flows
5. **Webhook Documentation** - If webhooks are implemented, document in Swagger

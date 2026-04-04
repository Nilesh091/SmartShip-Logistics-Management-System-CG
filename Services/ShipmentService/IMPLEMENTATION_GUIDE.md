# ShipmentService Implementation Guide

## 📋 Overview

ShipmentService is a complete microservice implementation following the **Clean Architecture pattern** with full support for shipment lifecycle management, JWT authentication, and role-based authorization.

## 🏗️ Architecture

```
ShipmentService
├── ShipmentService.Domain
│   ├── Entities/
│   │   ├── Shipment.cs
│   │   ├── Address.cs
│   │   └── Package.cs
│   └── Enums/
│       └── ShipmentStatus.cs
├── ShipmentService.Application
│   ├── DTOs/
│   │   ├── CreateShipmentDto.cs
│   │   ├── ShipmentResponseDto.cs
│   │   ├── AddressDto.cs
│   │   ├── PackageDto.cs
│   │   └── UpdateShipmentStatusDto.cs
│   └── Services/
│       ├── IShipmentService.cs
│       └── ShipmentService.cs
├── ShipmentService.Infrastructure
│   ├── Persistence/
│   │   └── ShipmentDbContext.cs
│   ├── Repositories/
│   │   ├── IShipmentRepository.cs
│   │   └── ShipmentRepository.cs
│   └── Migrations/
│       └── [Database migrations]
└── ShipmentService.API
    ├── Controllers/
    │   └── ShipmentController.cs
    ├── Program.cs
    ├── appsettings.json
    └── appsettings.Development.json
```

## 🔄 Shipment Lifecycle

The shipment follows this state machine:

```
DRAFT → BOOKED → PICKED_UP → IN_TRANSIT → OUT_FOR_DELIVERY → DELIVERED
```

**Status Constants:**

- `DRAFT` - Initial status (Customer can edit or cancel)
- `BOOKED` - Confirmed by customer
- `PICKED_UP` - Collected from sender
- `IN_TRANSIT` - On the way
- `OUT_FOR_DELIVERY` - With delivery agent
- `DELIVERED` - Successfully delivered

## 📡 API Endpoints

### Customer Endpoints (Requires `CUSTOMER` role)

#### 1. Create Shipment

```
POST /api/shipments
Authorization: Bearer {jwt_token}

Body:
{
  "sender": {
    "name": "John Doe",
    "street": "123 Main St",
    "city": "New York",
    "state": "NY",
    "zipCode": "10001"
  },
  "receiver": {
    "name": "Jane Smith",
    "street": "456 Oak Ave",
    "city": "Los Angeles",
    "state": "CA",
    "zipCode": "90001"
  },
  "package": {
    "weight": 5.5,
    "description": "Electronics shipment"
  }
}

Response: 201 Created
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "Shipment created successfully."
}
```

#### 2. Get My Shipments

```
GET /api/shipments/my
Authorization: Bearer {jwt_token}

Response: 200 OK
{
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "userId": "user-id",
      "status": "BOOKED",
      "createdAt": "2026-04-04T10:30:00Z",
      "updatedAt": "2026-04-04T11:00:00Z",
      "senderAddress": { ... },
      "receiverAddress": { ... },
      "package": { ... }
    }
  ],
  "count": 1
}
```

#### 3. Get Shipment Details

```
GET /api/shipments/{id}
Authorization: Bearer {jwt_token}

Response: 200 OK
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "user-id",
  "status": "BOOKED",
  "createdAt": "2026-04-04T10:30:00Z",
  "updatedAt": "2026-04-04T11:00:00Z",
  "senderAddress": { ... },
  "receiverAddress": { ... },
  "package": { ... }
}
```

#### 4. Book Shipment (Draft → Booked)

```
PUT /api/shipments/{id}/book
Authorization: Bearer {jwt_token}

Response: 200 OK
{
  "message": "Shipment booked successfully."
}
```

#### 5. Cancel Shipment

```
DELETE /api/shipments/{id}
Authorization: Bearer {jwt_token}

Response: 200 OK
{
  "message": "Shipment cancelled successfully."
}
```

### Admin Endpoints (Requires `ADMIN` role)

#### 6. Get All Shipments

```
GET /api/shipments/admin/all
Authorization: Bearer {jwt_token}

Response: 200 OK
{
  "data": [ ... ],
  "count": 10
}
```

#### 7. Update Shipment Status

```
PUT /api/shipments/{id}/status
Authorization: Bearer {jwt_token}

Body:
{
  "status": "IN_TRANSIT"
}

Response: 200 OK
{
  "message": "Shipment status updated successfully."
}
```

## 🔐 Authentication & Authorization

### JWT Token Claims

The service expects JWT tokens with the following claims:

```json
{
  "UserId": "user-id-guid",
  "role": "CUSTOMER|ADMIN",
  "email": "user@example.com",
  "exp": 1234567890
}
```

### Required Headers

```
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

### Role-Based Access Control

- **CUSTOMER**: Can create, view, and manage their own shipments
- **ADMIN**: Can view all shipments and update their status

## 🗄️ Database Setup

### Connection String

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=ShipmentServiceDb;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

### Running Migrations

The application automatically runs migrations on startup. To manually create the database:

```bash
cd ShipmentService.API
dotnet ef database update --project ../ShipmentService.Infrastructure
```

### Database Tables

- **Shipments**: Main shipment records with user and status tracking
- **Addresses**: Sender and receiver address details
- **Packages**: Package weight and description

## 🚀 Getting Started

### Prerequisites

- .NET 10.0 or later
- SQL Server 2019 or later
- Visual Studio Code or Visual Studio

### Installation

1. **Restore NuGet packages:**

   ```bash
   cd ShipmentService
   dotnet restore
   ```

2. **Set up the database:**

   ```bash
   cd ShipmentService.API
   dotnet ef database update --project ../ShipmentService.Infrastructure
   ```

3. **Update JWT secret in appsettings.json** (Production only):

   ```json
   {
     "JwtSettings": {
       "SecretKey": "your-production-secret-key-minimum-32-characters"
     }
   }
   ```

4. **Run the application:**
   ```bash
   dotnet run
   ```

The service will be available at `https://localhost:7182` (or check launchSettings.json)

## 📝 Dependency Injection

All services are registered in `Program.cs`:

```csharp
// Database
builder.Services.AddDbContext<ShipmentDbContext>(options =>
    options.UseSqlServer(connectionString)
);

// Repositories
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();

// Services
builder.Services.AddScoped<IShipmentService, ShipmentService>();

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { ... });
```

## 🧪 Testing

### Example: Create Shipment (Postman)

1. **Get JWT Token** (from your Auth Service)
2. **Set Authorization Header:**
   ```
   Authorization: Bearer {your_jwt_token}
   ```
3. **POST** `https://localhost:7182/api/shipments`
4. **Body:**
   ```json
   {
     "sender": {
       "name": "Sender Name",
       "street": "123 Main",
       "city": "NY",
       "state": "NY",
       "zipCode": "10001"
     },
     "receiver": {
       "name": "Receiver Name",
       "street": "456 Oak",
       "city": "LA",
       "state": "CA",
       "zipCode": "90001"
     },
     "package": {
       "weight": 10.5,
       "description": "Test package"
     }
   }
   ```

## 🛠️ Configuration

### CORS Policy

The service allows requests from any origin in development:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

**For Production**: Restrict to specific domains.

### Logging

Logging is configured for console and debug output:

- **Development**: Verbose logging for EF Core
- **Production**: Only warnings and errors

## ⚠️ Important Rules

✅ **Security:**

- Always extract `UserId` from JWT, never trust frontend
- CUSTOMER role can only access/modify their own shipments
- ADMIN role required for status updates
- Validate shipment ownership before operations

✅ **Business Logic:**

- Status transitions follow the state machine
- Only Draft/Booked shipments can be cancelled
- Weight and address validation on creation
- Timestamps automatically managed

✅ **Data Validation:**

- All DTOs are validated before processing
- Required fields: Name, Street, City, State, ZipCode for addresses
- Package weight must be > 0
- Invalid status transitions are rejected

## 📦 Deployment

### Docker (Optional)

Create a `Dockerfile` in the API project root:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet build -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "ShipmentService.API.dll"]
```

Build and run:

```bash
docker build -t shipmentservice .
docker run -p 8080:80 shipmentservice
```

## 📚 Key Classes

| Class                | Purpose               |
| -------------------- | --------------------- |
| `Shipment`           | Core domain entity    |
| `ShipmentStatus`     | Status constants/enum |
| `ShipmentDbContext`  | EF Core DbContext     |
| `ShipmentRepository` | Data access layer     |
| `ShipmentService`    | Business logic        |
| `ShipmentController` | API endpoints         |

## 🔗 Gateway Routes

For API Gateway integration:

```json
{
  "UpstreamPathTemplate": "/gateway/shipments/{everything}",
  "DownstreamPathTemplate": "/api/shipments/{everything}",
  "DownstreamScheme": "https",
  "DownstreamHostAndPorts": [
    {
      "Host": "localhost",
      "Port": 7182
    }
  ]
}
```

## 🐛 Troubleshooting

### Database Connection Issues

- Verify SQL Server is running
- Check connection string in appsettings.json
- Ensure Windows Authentication or SQL credentials are correct

### JWT Token Errors

- Ensure token hasn't expired
- Verify token contains `UserId` claim
- Check secret key matches between services

### Authorization Failures

- Verify `CUSTOMER` or `ADMIN` role is in token
- Check token hasn't been tampered with
- Validate claim names match expectations

## 📖 Additional Resources

- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- [JWT Best Practices](https://tools.ietf.org/html/rfc7519)

---

**Version:** 1.0.0  
**Last Updated:** April 4, 2026  
**Status:** ✅ Complete and Production-Ready

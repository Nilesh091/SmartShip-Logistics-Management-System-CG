# TrackingService

## Architecture

The TrackingService follows a **Clean Architecture** pattern with 4 layers:

### 1. **Domain Layer** (`TrackingService.Domain`)

- Contains core business entities
- `TrackingEvent` - Entity representing a tracking event

### 2. **Infrastructure Layer** (`TrackingService.Infrastructure`)

- Database context and repository pattern
- RabbitMQ consumer for event processing
- Event models

**Key Components:**

- `TrackingDbContext` - EF Core DbContext
- `ITrackingRepository` - Repository interface
- `TrackingRepository` - Repository implementation
- `RabbitMQConsumer` - Consumes shipment status change events
- `ShipmentStatusChangedEvent` - Event model from message queue

### 3. **Application Layer** (`TrackingService.Application`)

- Business logic and use cases
- `ITrackingService` - Service interface
- `TrackingService` - Service implementation

### 4. **API Layer** (`TrackingService.API`)

- REST controllers and HTTP endpoints
- Dependency injection configuration
- `TrackingController` - Exposes tracking endpoints

## Flow

```
ShipmentService publishes "ShipmentStatusChanged" event
        ↓
RabbitMQ queue receives message
        ↓
TrackingService.RabbitMQConsumer consumes event
        ↓
Creates TrackingEvent entity
        ↓
TrackingRepository saves to database
        ↓
User calls GET /api/tracking/{shipmentId}
        ↓
TrackingController returns timeline (ordered by timestamp)
```

## Setup

### Prerequisites

- .NET 10.0
- SQL Server
- RabbitMQ (optional, service will start without it)

### Installation

1. **Restore NuGet Packages**

```bash
dotnet restore
```

2. **Update Database**

```bash
dotnet ef database update -p TrackingService.Infrastructure -s TrackingService.API
```

3. **Run the Service**

```bash
dotnet run --project TrackingService.API
```

## API Endpoints

### Get Tracking Timeline

```http
GET /api/tracking/{shipmentId}
```

**Response:**

```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "shipmentId": "123e4567-e89b-12d3-a456-426614174001",
    "status": "BOOKED",
    "location": "Warehouse A",
    "timestamp": "2026-04-04T10:00:00Z"
  },
  {
    "id": "223e4567-e89b-12d3-a456-426614174000",
    "shipmentId": "123e4567-e89b-12d3-a456-426614174001",
    "status": "IN_TRANSIT",
    "location": "Route A",
    "timestamp": "2026-04-04T14:30:00Z"
  }
]
```

## Configuration

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=TrackingServiceDb;User Id=sa;Password=your-password;"
  },
  "RabbitMQ": {
    "HostName": "your-rabbitmq-host"
  }
}
```

## Project Structure

```
TrackingService.Domain/
├── Entities/
│   └── TrackingEvent.cs

TrackingService.Infrastructure/
├── Data/
│   ├── TrackingDbContext.cs
│   ├── ITrackingRepository.cs
│   └── TrackingRepository.cs
├── Events/
│   └── ShipmentStatusChangedEvent.cs
└── Messaging/
    └── RabbitMQConsumer.cs

TrackingService.Application/
└── Services/
    ├── ITrackingService.cs
    └── TrackingService.cs

TrackingService.API/
├── Controllers/
│   └── TrackingController.cs
├── Program.cs
└── appsettings.json
```

## Key Concepts

### ShipmentService vs TrackingService

| Aspect  | ShipmentService  | TrackingService |
| ------- | ---------------- | --------------- |
| Purpose | Current state    | History         |
| Data    | Latest status    | All events      |
| Update  | Direct DB update | Event-driven    |

### Example

**ShipmentService DB:**
| ShipmentId | Status |
|------------|--------|
| 123 | IN_TRANSIT |

**TrackingService DB:**
| ShipmentId | Status | Timestamp |
|------------|--------|-----------|
| 123 | BOOKED | 2026-04-04 10:00 |
| 123 | PICKED_UP | 2026-04-04 11:30 |
| 123 | IN_TRANSIT | 2026-04-04 14:30 |

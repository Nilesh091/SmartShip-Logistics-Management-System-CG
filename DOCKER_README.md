# Docker Setup Guide - SmartShip Logistics Management System

## 🚀 Prerequisites

- Docker & Docker Compose installed
- Ports available: 5000-5004, 1433, 5672, 15672

## 📋 Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                    Gateway (5000)                        │
│              (Ocelot API Gateway with Swagger)           │
└──────────────┬──────────────────────────────────────────┘
               │
    ┌──────────┼──────────┬──────────┬──────────┐
    │          │          │          │          │
    ▼          ▼          ▼          ▼          ▼
┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌──────────┐
│ Auth   │ │ Admin  │ │Shipment│ │Tracking│ │   MSSQL  │
│Service │ │Service │ │Service │ │Service │ │ Database │
│(5001)  │ │(5002)  │ │(5003)  │ │(5004)  │ │ (1433)   │
└────────┘ └────────┘ └────────┘ └────────┘ └──────────┘
    │          │          │          │
    └──────────┼──────────┼──────────┘
               │
               ▼
        ┌──────────────┐
        │  RabbitMQ    │
        │ (5672, 15672)│
        └──────────────┘
```

## 🐳 Docker Service Names (for internal communication)

| Service          | Docker Name       | Local Port  | Internal Port |
| ---------------- | ----------------- | ----------- | ------------- |
| Auth Service     | `authservice`     | 5001        | 80            |
| Admin Service    | `adminservice`    | 5002        | 80            |
| Shipment Service | `shipmentservice` | 5003        | 80            |
| Tracking Service | `trackingservice` | 5004        | 80            |
| API Gateway      | `gateway`         | 5000        | 80            |
| MSSQL Server     | `mssql`           | 1433        | 1433          |
| RabbitMQ         | `rabbitmq`        | 5672, 15672 | 5672          |

## ▶️ How to Run

### Start All Services

```bash
docker-compose up --build
```

### Start Specific Services

```bash
# Only gateway and auth service
docker-compose up --build gateway authservice

# With detached mode
docker-compose up -d --build
```

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f authservice

# Follow logs
docker-compose logs -f --tail=100
```

### Stop Services

```bash
docker-compose down
```

### Remove Everything (including volumes)

```bash
docker-compose down -v
```

## 📊 Access Points

Once running, access the following:

- **API Gateway (Main Entry Point)**: http://localhost:5000
  - Swagger UI: http://localhost:5000/swagger
- **Individual Services** (for testing):
  - Auth Service: http://localhost:5001
  - Admin Service: http://localhost:5002
  - Shipment Service: http://localhost:5003
  - Tracking Service: http://localhost:5004

- **RabbitMQ Management UI**: http://localhost:15672
  - Username: `guest`
  - Password: `guest`

- **MSSQL Server**:
  - Server: `localhost,1433`
  - Username: `sa`
  - Password: `2004@Nilu`

## 🔧 Configuration Notes

### Internal Service Communication

When services need to communicate with each other inside Docker, use the **service name** with port 80:

- Example: `http://authservice:80/api/endpoint`

### Database Connections

All services connect to `mssql:1433` (Docker DNS):

```
Server=mssql,1433;Database=DatabaseName;User Id=sa;Password=2004@Nilu;TrustServerCertificate=True;
```

### RabbitMQ Hostname

Set to `rabbitmq` inside containers (docker-compose sets this via environment variables).

## 🔍 Troubleshooting

### Services Failing to Start

1. Check logs: `docker-compose logs service-name`
2. Verify database readiness: `docker-compose logs mssql`
3. Check port conflicts: `lsof -i :5000`

### Database Connection Issues

- Ensure MSSQL is healthy: `docker-compose ps` (should show "healthy")
- Wait for database to initialize: It takes ~30 seconds on first run

### Can't Access Swagger from Gateway

- Ensure all services are running
- Check if swagger endpoints are configured correctly
- Verify network connectivity

### Container Resource Issues

```bash
# Check Docker stats
docker stats

# Clean up unused resources
docker system prune
```

## 📝 File Structure

```
.
├── docker-compose.yml          # Main orchestration file
├── Gateway/
│   ├── Dockerfile              # Gateway build
│   └── Gateway.API/
│       └── ocelot.json         # Updated for Docker service names
├── Services/
│   ├── AuthService/
│   │   ├── Dockerfile
│   │   └── AuthService.API/
│   ├── AdminService/
│   │   ├── Dockerfile
│   │   └── AdminService.API/
│   ├── ShipmentService/
│   │   ├── Dockerfile
│   │   └── ShipmentService.API/
│   └── TrackingService/
│       ├── Dockerfile
│       └── TrackingService.API/
└── Shared/
```

## 🚨 Important Changes Made

✅ **Dockerfiles Created**

- All services have multi-stage Dockerfiles (build + runtime)

✅ **ocelot.json Updated**

- All localhost references changed to Docker service names
- All ports changed to 80 (internal Docker network)

✅ **appsettings.Development.json Updated**

- AdminService now routes to `http://shipmentservice:80` and `http://authservice:80`
- TrackingService now uses `rabbitmq` as RabbitMQ hostname

✅ **docker-compose.yml Created**

- Includes MSSQL, RabbitMQ, all 4 services, and gateway
- Proper networking and healthchecks
- Automatic database initialization via environment variables
- Service dependencies configured

## 🔐 Security Notes

⚠️ **For Development Only**

- Credentials are hardcoded in configs
- No SSL/HTTPS configured
- Default RabbitMQ credentials used
- All services allow CORS from any origin

For production:

- Use secrets management (Docker Secrets, Kubernetes Secrets)
- Enable HTTPS
- Change default credentials
- Restrict CORS policies
- Use non-sa database user with limited permissions

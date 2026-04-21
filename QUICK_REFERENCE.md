# 🚀 SmartShip Local Development - Quick Reference

## Prerequisites Quick Checklist

```
✅ .NET 10.0 SDK
✅ SQL Server 2019+ (or Express)
✅ RabbitMQ 3.x
✅ Visual Studio 2022 / VS Code
```

---

## Service Ports

```
Gateway              http://localhost:5166
Auth Service         http://localhost:5001
Admin Service        http://localhost:5002
Shipment Service     http://localhost:5003
Tracking Service     http://localhost:5004
Notification Service http://localhost:5005
RabbitMQ UI          http://localhost:15672
SQL Server           localhost:1433
```

---

## Quick Start Commands

### Windows

```cmd
# Start all services
start-local-services.bat

# Or individual services
cd Services\AuthService\AuthService.API
dotnet run
```

### macOS/Linux

```bash
# Make script executable
chmod +x start-local-services.sh

# Start all services
./start-local-services.sh

# Or individual services
cd Services/AuthService/AuthService.API
dotnet run
```

### Visual Studio

```
1. Open SmartShip.sln
2. Right-click Solution → Properties
3. Select "Multiple startup projects"
4. Set Action = "Start" for all services
5. Press F5
```

### VS Code

```
Ctrl+Shift+B → Select "Build All Services"
Ctrl+Shift+B → Select "Auth Service - Run" (etc)
```

---

## Database Setup

### Create Databases (SQL Server Management Studio)

```sql
CREATE DATABASE SmartLogisticsAuthServiceDb;
CREATE DATABASE SmartLogisticsShipmentServiceDb;
CREATE DATABASE SmartLogisticsTrackingServiceDb;
CREATE DATABASE SmartLogisticsAdminServiceDb;
```

### Connection String Reference

```
Server=localhost,1433;Database=<DbName>;User Id=sa;Password=2004@Nilu;TrustServerCertificate=True;
```

---

## Verify Everything Works

```bash
# Check SQL Server
sqlcmd -S localhost -U sa -P "2004@Nilu" -Q "SELECT @@VERSION"

# Check RabbitMQ
rabbitmqctl status

# Test Gateway
curl http://localhost:5166/health

# Access Swagger UIs
http://localhost:5166/swagger        # Gateway
http://localhost:5001/swagger        # Auth Service
http://localhost:5003/swagger        # Shipment Service
http://localhost:5004/swagger        # Tracking Service
http://localhost:5002/swagger        # Admin Service
```

---

## Configuration Files

| Service      | File                                                                                |
| ------------ | ----------------------------------------------------------------------------------- |
| Auth         | `Services/AuthService/AuthService.API/appsettings.Development.json`                 |
| Shipment     | `Services/ShipmentService/ShipmentService.API/appsettings.Development.json`         |
| Tracking     | `Services/TrackingService/TrackingService.API/appsettings.Development.json`         |
| Notification | `Services/NotificationService/NotificationService.API/appsettings.Development.json` |
| Gateway      | `Gateway/Gateway.API/ocelot.json`                                                   |

---

## Credentials

| Service               | User                  | Password            |
| --------------------- | --------------------- | ------------------- |
| SQL Server            | sa                    | 2004@Nilu           |
| RabbitMQ              | guest                 | guest               |
| Gmail (Notifications) | vivek250500@gmail.com | ugsg nzcc sumd eteb |

---

## Common Troubleshooting

| Issue                         | Solution                                           |
| ----------------------------- | -------------------------------------------------- |
| "Cannot connect to database"  | Check SQL Server is running, verify credentials    |
| "RabbitMQ refused connection" | Start RabbitMQ service, check port 5672            |
| "Port already in use"         | Kill process or change port in launchSettings.json |
| "NuGet packages missing"      | Run `dotnet restore SmartShip.sln`                 |
| "Service won't start"         | Check logs, verify all prerequisites, check ports  |

---

## Documentation Links

- 📖 **Full Setup Guide**: `LOCAL_ENVIRONMENT_SETUP.md`
- 🔄 **Migration Details**: `DOCKER_MIGRATION.md`
- ✅ **Setup Complete**: `LOCAL_SETUP_COMPLETE.md`

---

## Key Changes from Docker

| Item              | Docker                 | Local              |
| ----------------- | ---------------------- | ------------------ |
| RabbitMQ Hostname | `rabbitmq` (container) | `localhost`        |
| Database          | Supabase (remote)      | SQL Server (local) |
| Service Discovery | Docker DNS             | Localhost + ports  |
| Gateway Routes    | Container names        | `localhost:port`   |
| Startup Time      | 30-60s                 | 10-20s             |
| Resources         | 2-4 GB RAM             | 800MB-1.5GB        |

---

## Environment Variables

Create `.env` file in root directory:

```
DB_PASSWORD=2004@Nilu
SMTP_PASSWORD=ugsg nzcc sumd eteb
```

---

## Next Steps

1. ✅ Install prerequisites
2. ✅ Create SQL Server databases
3. ✅ Verify RabbitMQ is running
4. ✅ Run startup script or services manually
5. ✅ Access http://localhost:5166
6. ✅ Test API endpoints via Swagger

---

**Status**: ✅ Ready to Start Local Development

For detailed instructions, see `LOCAL_ENVIRONMENT_SETUP.md`

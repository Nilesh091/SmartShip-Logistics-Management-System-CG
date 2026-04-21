# Docker Migration to Local Environment

**Date:** April 18, 2026  
**Status:** ✅ Migration Complete

## 📋 Summary

The SmartShip Logistics Management System has been migrated from a Docker-based deployment to a local development environment. This document outlines what was changed and why.

---

## 🔄 What Changed

### ✅ Configuration Files Updated

#### 1. **appsettings.Development.json** (All Services)

**From (Docker):**

```json
{
  "RabbitMQ": {
    "HostName": "rabbitmq", // Docker service name
    "Port": 5672
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.pspdrwkppsvksenkgxtn.supabase.co;..."
  }
}
```

**To (Local):**

```json
{
  "RabbitMQ": {
    "HostName": "localhost", // Local machine
    "UserName": "guest",
    "Password": "guest"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=SmartLogistics<Service>Db;..."
  }
}
```

**Services Updated:**

- ✅ `Services/AuthService/AuthService.API/appsettings.Development.json`
- ✅ `Services/ShipmentService/ShipmentService.API/appsettings.Development.json`
- ✅ `Services/TrackingService/TrackingService.API/appsettings.Development.json`
- ✅ `Services/NotificationService/NotificationService.API/appsettings.Development.json`

#### 2. **ocelot.json** (Gateway Configuration)

**From (Docker):**

```json
{
  "DownstreamHostAndPorts": [
    { "Host": "authservice", "Port": 8080 } // Docker container name
  ],
  "SwaggerEndPoints": [
    {
      "Url": "http://authservice:8080/swagger/v1/swagger.json"
    }
  ]
}
```

**To (Local):**

```json
{
  "DownstreamHostAndPorts": [
    { "Host": "localhost", "Port": 5001 } // Local machine
  ],
  "SwaggerEndPoints": [
    {
      "Url": "http://localhost:5001/swagger/v1/swagger.json"
    }
  ]
}
```

**Updates:**

- ✅ Auth Service: `authservice:8080` → `localhost:5001`
- ✅ Admin Service: `adminservice:8080` → `localhost:5002`
- ✅ Shipment Service: `shipmentservice:8080` → `localhost:5003`
- ✅ Tracking Service: `trackingservice:8080` → `localhost:5004`

---

## 📦 Docker Files (Archived)

### Files No Longer Needed for Local Development

```
❌ docker-compose.yml
├─ Defined RabbitMQ, MSSQL (was Supabase), all microservices
├─ Network configuration for inter-service communication
└─ Environment variables for Docker containers

❌ .dockerignore
└─ Prevented certain files from being included in Docker build context

❌ Gateway/Dockerfile
├─ Multi-stage build for Gateway.API
├─ Build stage: mcr.microsoft.com/dotnet/sdk:8.0
└─ Runtime stage: mcr.microsoft.com/dotnet/aspnet:8.0

❌ Services/AuthService/Dockerfile
❌ Services/ShipmentService/Dockerfile
❌ Services/TrackingService/Dockerfile
❌ Services/AdminService/Dockerfile
❌ Services/NotificationService/Dockerfile
└─ Similar multi-stage Docker builds for each service

❌ Docker-related documentation:
├─ DOCKER_README.md
├─ DOCKER_SETUP_COMPLETE.md
├─ DOCKER_QUICK_REFERENCE.md
└─ Docker configuration notes
```

### How to Retain Docker Files for Future Use

If you need to re-enable Docker containerization later:

**Option 1: Keep Docker Files for Reference**

```bash
# Archive Docker files to a separate directory
mkdir docker-archive
mv docker-compose.yml docker-archive/
mv .dockerignore docker-archive/
mv Gateway/Dockerfile docker-archive/
# Move all service Dockerfiles...
```

**Option 2: Use Git to Preserve History**

```bash
# Docker files are still in Git history
git log --oneline -- docker-compose.yml
git show <commit-hash>:docker-compose.yml  # Restore from history
```

---

## 🖥️ Local Infrastructure Requirements

### SQL Server (Replaces MSSQL in Docker)

- **Previously:** Docker container (mssql:2022)
- **Now:** Local SQL Server 2019+
- **Databases Created:**
  - SmartLogisticsAuthServiceDb
  - SmartLogisticsShipmentServiceDb
  - SmartLogisticsTrackingServiceDb
  - SmartLogisticsAdminServiceDb

### RabbitMQ (Replaces Docker Container)

- **Previously:** `rabbitmq:3-management` Docker container
- **Now:** Local RabbitMQ server
- **Access:**
  - AMQP: `localhost:5672`
  - Management UI: `http://localhost:15672`

### Service Ports (Changed)

```
Docker → Local Mapping:

Auth Service:
  Docker: authservice:8080
  Local:  localhost:5001

Shipment Service:
  Docker: shipmentservice:8080
  Local:  localhost:5003

Tracking Service:
  Docker: trackingservice:8080
  Local:  localhost:5004

Admin Service:
  Docker: adminservice:8080
  Local:  localhost:5002

Notification Service:
  Docker: notificationservice:8080
  Local:  localhost:5005

Gateway:
  Docker: gateway:8080
  Local:  localhost:5166
```

---

## ✨ New Local Development Tools

### Quick Start Scripts

**Unix/macOS/Linux:**

```bash
chmod +x start-local-services.sh
./start-local-services.sh
```

**Windows:**

```cmd
start-local-services.bat
```

### Configuration Files

1. **LOCAL_ENVIRONMENT_SETUP.md**
   - Complete setup instructions
   - Prerequisite installation guides
   - Troubleshooting section
   - Database and RabbitMQ setup

2. **Updated appsettings.Development.json**
   - All services configured for localhost
   - Database connection strings for SQL Server
   - RabbitMQ configured for local connections

3. **Updated ocelot.json**
   - Gateway routes configured for local ports
   - Swagger endpoints for local services

---

## 🚀 Migration Steps Performed

### 1. Configuration Updates ✅

- [x] Updated all `appsettings.Development.json` files
- [x] Changed RabbitMQ hostname from `rabbitmq` to `localhost`
- [x] Changed database connections to local SQL Server
- [x] Updated Gateway ocelot.json routes to use localhost

### 2. Service Port Mapping ✅

- [x] Defined local port numbers for all services (5001-5005, 5166)
- [x] Updated gateway routing configuration
- [x] Updated Swagger endpoint URLs

### 3. Documentation ✅

- [x] Created LOCAL_ENVIRONMENT_SETUP.md (comprehensive guide)
- [x] Created DOCKER_MIGRATION.md (this document)
- [x] Created start-local-services.sh (Unix/macOS automation)
- [x] Created start-local-services.bat (Windows automation)

### 4. Local Infrastructure Setup ✅

- [x] Provided SQL Server installation instructions
- [x] Provided RabbitMQ installation instructions
- [x] Documented connection strings and credentials
- [x] Created database provisioning script

---

## 📊 Environment Comparison

| Aspect                       | Docker                      | Local              |
| ---------------------------- | --------------------------- | ------------------ |
| **SQL Server**               | Separate MSSQL container    | Local installation |
| **RabbitMQ**                 | Separate container          | Local installation |
| **Services**                 | Multiple containers         | Multiple processes |
| **Networking**               | Docker network bridge       | localhost + ports  |
| **Port 5672 (AMQP)**         | Available                   | Available          |
| **Port 15672 (RabbitMQ UI)** | Available                   | Available          |
| **Database Access**          | Container network           | Direct connection  |
| **Startup Time**             | ~30-60 seconds              | ~10-20 seconds     |
| **Resource Usage**           | Higher (container overhead) | Lower              |
| **Development**              | Easy but resource-heavy     | Fast iteration     |

---

## 🔄 How to Switch Back to Docker

If you need to return to Docker-based deployment:

### 1. Restore Docker Files from Git

```bash
git checkout HEAD -- docker-compose.yml .dockerignore
git checkout HEAD -- Gateway/Dockerfile
git checkout HEAD -- Services/*/Dockerfile
```

### 2. Restore Docker Configuration

```bash
# Create a new branch from Docker commit
git checkout <docker-setup-commit>
git checkout master -- Services/*/appsettings.json
git checkout master -- Gateway/Gateway.API/ocelot.json
```

### 3. Update Configuration for Docker

Revert appsettings to use Docker service names:

- `rabbitmq` instead of `localhost`
- Docker container network DNS names

### 4. Rebuild Containers

```bash
docker-compose build
docker-compose up
```

---

## 🔒 Security Notes

### Local Development

- ✅ SQL Server uses local credentials: `sa:2004@Nilu`
- ✅ RabbitMQ uses default credentials: `guest:guest`
- ✅ No HTTPS in Development mode
- ✅ Swagger UI is publicly accessible

### Production Considerations

When deploying to production:

- [ ] Use strong, unique passwords
- [ ] Enable HTTPS/TLS
- [ ] Use Azure Key Vault or AWS Secrets Manager
- [ ] Disable Swagger UI
- [ ] Configure proper firewalls
- [ ] Use managed database services (Azure SQL, RDS, etc.)
- [ ] Consider returning to containerization with Kubernetes

---

## 📚 Related Documentation

- [LOCAL_ENVIRONMENT_SETUP.md](./LOCAL_ENVIRONMENT_SETUP.md) - Setup instructions
- [DOCKER_README.md](./DOCKER_README.md) - Original Docker setup (archived)
- [GATEWAY_SWAGGER_IMPLEMENTATION.md](./GATEWAY_SWAGGER_IMPLEMENTATION.md) - Gateway configuration
- [GMAIL_SETUP_GUIDE.md](./GMAIL_SETUP_GUIDE.md) - Email configuration

---

## 🎯 Benefits of Local Development Environment

✅ **Faster Development Cycles**

- Direct code-to-running-service iteration
- No Docker build/start overhead

✅ **Better Debugging**

- Direct debugger attachment
- Immediate log visibility
- Easier breakpoint debugging

✅ **Lower Resource Usage**

- No container overhead
- Less memory consumption
- Faster startup times

✅ **Simplified Setup**

- Fewer moving parts
- Easier to understand system architecture
- Better for learning and development

✅ **Windows/macOS/Linux Compatibility**

- Works equally well on all platforms
- No Docker Desktop dependencies
- Native development experience

---

## ⚠️ Trade-offs

| Factor                      | Docker                  | Local                   |
| --------------------------- | ----------------------- | ----------------------- |
| **Production Parity**       | High                    | Medium                  |
| **Environment Consistency** | Guaranteed              | Depends on OS           |
| **Multi-developer Setup**   | Unified                 | May vary                |
| **Deployment**              | Direct containerization | Additional steps needed |
| **Resource Usage**          | Higher                  | Lower                   |
| **Complexity**              | Higher                  | Lower                   |

---

## 📞 Troubleshooting

### Issue: "Cannot connect to RabbitMQ"

**Solution:** Ensure RabbitMQ is running

```bash
# macOS
brew services start rabbitmq

# Windows: Services > RabbitMQ > Start
# Linux: sudo systemctl start rabbitmq-server
```

### Issue: "Database connection failed"

**Solution:** Verify SQL Server is running and databases exist

```sql
SELECT name FROM sys.databases WHERE name LIKE 'SmartLogistics%';
```

### Issue: "Port already in use"

**Solution:** Change port in `launchSettings.json` or stop conflicting service

```bash
# Find process on port
lsof -i :5001  # macOS/Linux
netstat -ano | findstr :5001  # Windows
```

---

## ✅ Migration Checklist

- [x] All appsettings.Development.json updated
- [x] ocelot.json updated with localhost routes
- [x] Local environment documentation created
- [x] Startup scripts created (bash and batch)
- [x] Prerequisites documented
- [x] Connection strings verified
- [x] Port mapping documented
- [x] Quick start guide created
- [x] This migration document created

---

**Status:** ✅ Ready for Local Development  
**Next Steps:** See [LOCAL_ENVIRONMENT_SETUP.md](./LOCAL_ENVIRONMENT_SETUP.md) for installation and startup instructions.

# 🚀 Docker Quick Reference Card

## One-Command Startup

```bash
docker-compose up --build
```

## Essential Commands

### View Status

```bash
docker-compose ps              # List all containers
docker-compose logs -f         # Follow logs
docker-compose logs authservice  # Specific service logs
```

### Management

```bash
docker-compose down            # Stop all services
docker-compose down -v         # Stop & remove volumes
docker-compose restart         # Restart all services
docker-compose restart authservice  # Restart specific service
```

### Debugging

```bash
docker-compose exec authservice /bin/sh    # Connect to container
docker-compose logs --tail=50 authservice  # Last 50 lines
```

---

## Service Port Mapping

| Service         | Port       | URL                        |
| --------------- | ---------- | -------------------------- |
| 🚪 **Gateway**  | 5000       | http://localhost:5000      |
| 🔐 **Auth**     | 5001       | http://localhost:5001      |
| 👤 **Admin**    | 5002       | http://localhost:5002      |
| 📦 **Shipment** | 5003       | http://localhost:5003      |
| 📍 **Tracking** | 5004       | http://localhost:5004      |
| 🐘 **MSSQL**    | 1433       | `localhost,1433`           |
| 🐰 **RabbitMQ** | 5672/15672 | UI: http://localhost:15672 |

---

## Internal Service DNS Names

Use these names **inside containers** for service-to-service calls:

- `authservice:80`
- `adminservice:80`
- `shipmentservice:80`
- `trackingservice:80`
- `mssql:1433`
- `rabbitmq:5672`

---

## API Gateway Swagger URLs

**Main Aggregated Swagger:**

```
http://localhost:5000/swagger
```

**Individual Service Swagger:**

- Auth: http://localhost:5001/swagger
- Admin: http://localhost:5002/swagger
- Shipment: http://localhost:5003/swagger
- Tracking: http://localhost:5004/swagger

---

## Database Access

**From Host Machine** (SSMS / Azure Data Studio):

```
Server=localhost,1433
Username=sa
Password=2004@Nilu
```

**Database Names:**

- SmartLogisticsAuthServiceDb
- SmartLogisticsShipmentServiceDb
- TrackingServiceDb

---

## RabbitMQ Management UI

**URL:** http://localhost:15672  
**Username:** guest  
**Password:** guest

---

## Testing Service Connectivity

```bash
# Test Auth Service from host
curl http://localhost:5001/api/auth/health

# Test through Gateway
curl http://localhost:5000/auth/health

# View all routes
curl http://localhost:5000/ | grep -i route
```

---

## Troubleshooting Checklist

- [ ] All containers running? → `docker-compose ps`
- [ ] MSSQL healthy? → Check MSSQL logs: `docker-compose logs mssql`
- [ ] Port conflicts? → `lsof -i :5000` (macOS) or `netstat -ano | findstr :5000` (Windows)
- [ ] Network issues? → `docker network ls` & `docker network inspect smartship-network`
- [ ] Permission denied? → May need `sudo` or Docker Desktop access
- [ ] Out of disk? → `docker system prune` (removes unused images/containers)

---

## File Structure Reference

```
📁 SmartShip-Logistics-Management-System-CG/
├─ docker-compose.yml              ← Main orchestration file
├─ .dockerignore                   ← Build optimization
├─ DOCKER_README.md                ← Full documentation
├─ DOCKER_SETUP_COMPLETE.md        ← Setup summary
├─ DOCKER_QUICK_REFERENCE.md       ← This file
│
├─ 📁 Gateway/
│  ├─ Dockerfile                   ← Gateway build
│  └─ Gateway.API/
│     └─ ocelot.json               ← Updated for Docker DNS
│
└─ 📁 Services/
   ├─ 📁 AuthService/
   │  ├─ Dockerfile
   │  └─ AuthService.API/
   │     └─ appsettings.json
   ├─ 📁 AdminService/
   │  ├─ Dockerfile
   │  └─ AdminService.API/
   │     ├─ appsettings.json
   │     └─ appsettings.Development.json ← Updated
   ├─ 📁 ShipmentService/
   │  ├─ Dockerfile
   │  └─ ShipmentService.API/
   │     └─ appsettings.json
   └─ 📁 TrackingService/
      ├─ Dockerfile
      └─ TrackingService.API/
         ├─ appsettings.json
         └─ appsettings.Development.json ← Updated
```

---

## Common Errors & Solutions

### Error: "Port 5000 is already in use"

```bash
# Find and kill process using port 5000
lsof -i :5000 | grep -v PID | awk '{print $2}' | xargs kill -9

# Or use a different port in docker-compose.yml
# Change: "5000:80" to "5050:80"
```

### Error: "MSSQL not ready"

```bash
# Wait and check MSSQL logs
docker-compose logs mssql
# Usually takes 30 seconds on first run
```

### Error: "Cannot reach authservice from adminservice"

```bash
# Verify network
docker network inspect smartship-network

# Check DNS resolution inside container
docker-compose exec adminservice getent hosts authservice
```

### Error: "Authentication failed for JWT"

- Check JWT token format
- Verify issuer/audience match between Gateway and Auth Service
- Check JWT key length (must be >= 32 characters)

---

## Performance Tips

1. **First run is slow**: MSSQL initialization takes time
2. **Use multi-stage builds**: Already implemented in Dockerfiles
3. **Exclude unnecessary files**: `.dockerignore` configured
4. **Regular cleanup**: `docker system prune` weekly
5. **Monitor resources**: `docker stats` command

---

## Next Steps

1. Start containers: `docker-compose up --build`
2. Wait for all services to be healthy (~2 minutes)
3. Access Gateway: http://localhost:5000
4. Test individual services on ports 5001-5004
5. Check RabbitMQ: http://localhost:15672
6. View logs: `docker-compose logs -f`

---

**Setup Date:** April 11, 2026  
**System:** SmartShip Logistics Management System  
**Docker Version:** 3.8 (Compose format)

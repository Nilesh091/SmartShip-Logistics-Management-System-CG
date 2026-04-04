# 🚀 ShipmentService - Quick Start Guide

## 5-Minute Setup

### Step 1: Prerequisites Check

```bash
dotnet --version          # Should be 10.0 or higher
```

### Step 2: Restore & Build

```bash
cd ShipmentService
dotnet restore
dotnet build
```

### Step 3: Database Setup

```bash
cd ShipmentService.API
dotnet ef database update --project ../ShipmentService.Infrastructure

# Or let it auto-migrate on first run
```

### Step 4: Run the API

```bash
dotnet run
# API available at https://localhost:7182
```

### Step 5: Test with Postman

1. Import `Postman_Collection.json`
2. Set `jwt_token` variable with your JWT token
3. Run POST `/api/shipments` to test

---

## 🔧 Configuration Reference

### JWT Token Claims Required

```json
{
  "UserId": "guid",
  "role": "CUSTOMER|ADMIN"
}
```

### Connection String (Windows Auth)

```
Server=.;Database=ShipmentServiceDb;Integrated Security=true;TrustServerCertificate=true;
```

### Connection String (SQL Auth)

```
Server=localhost;Database=ShipmentServiceDb;User Id=sa;Password=YourPassword;TrustServerCertificate=true;
```

---

## 📋 Key Endpoints

| Endpoint                     | Method | Auth           |
| ---------------------------- | ------ | -------------- |
| `/api/shipments`             | POST   | CUSTOMER       |
| `/api/shipments/my`          | GET    | CUSTOMER       |
| `/api/shipments/{id}`        | GET    | CUSTOMER/ADMIN |
| `/api/shipments/{id}/book`   | PUT    | CUSTOMER       |
| `/api/shipments/{id}`        | DELETE | CUSTOMER       |
| `/api/shipments/admin/all`   | GET    | ADMIN          |
| `/api/shipments/{id}/status` | PUT    | ADMIN          |

---

## 🐛 Troubleshooting

### Database Connection Failed

- Verify SQL Server is running
- Check connection string
- Ensure Windows Authentication or credentials

### JWT Token Invalid

- Verify token hasn't expired
- Check UserId claim exists
- Validate signature key matches

### Role Authorization Failed

- Confirm token contains role claim
- Verify role value (CUSTOMER or ADMIN)
- Check endpoint requires correct role

### Port Already in Use

- Check `appsettings.json` for port
- Or modify in `launchSettings.json`

---

## 🔐 Production Checklist

- [ ] Change JWT secret key
- [ ] Update database connection string
- [ ] Enable HTTPS
- [ ] Configure CORS to specific domains
- [ ] Set up database backups
- [ ] Configure centralized logging
- [ ] Enable database audit logs
- [ ] Set up monitoring alerts
- [ ] Create database indexes
- [ ] Test load capacity

---

## 📊 Endpoint Status Codes

| Code | Meaning                              |
| ---- | ------------------------------------ |
| 200  | Success                              |
| 201  | Created                              |
| 400  | Bad Request / Validation Error       |
| 401  | Unauthorized (Missing/Invalid Token) |
| 403  | Forbidden (Wrong Role/Resource)      |
| 404  | Not Found                            |
| 500  | Server Error                         |

---

## 📁 Project Structure at a Glance

```
ShipmentService/
├── Domain/              → Entities & Enums
├── Application/         → Business Logic & DTOs
├── Infrastructure/      → Database & Repository
├── API/                 → Controllers & Configuration
├── IMPLEMENTATION_GUIDE.md
├── IMPLEMENTATION_COMPLETE.md
├── Postman_Collection.json
└── ShipmentService.sln
```

---

## 🚀 Deployment Paths

### Local Development

```bash
dotnet run
```

### Docker (Build)

```bash
docker build -t shipmentservice:latest .
docker run -p 8080:80 shipmentservice:latest
```

### Azure / Cloud

1. Create App Service
2. Configure connection string
3. Set environment variables
4. Deploy from CI/CD pipeline

---

## 💡 Pro Tips

### Enable Detailed Logging

```json
"Logging": {
  "LogLevel": {
    "Default": "Debug",
    "Microsoft.EntityFrameworkCore": "Information"
  }
}
```

### Find Your Shipment Port

```bash
grep -i "applicationurl" ShipmentService.API/Properties/launchSettings.json
```

### Test JWT Endpoint

```bash
curl -X GET https://localhost:7182/api/shipments \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## 📞 Quick Links

- [Entity Framework Documentation](https://learn.microsoft.com/ef/core/)
- [ASP.NET Core Security](https://learn.microsoft.com/aspnet/core/security/)
- [SQL Server Best Practices](https://learn.microsoft.com/sql/sql-server/)

---

**Ready?** Start with: `dotnet restore && dotnet build && dotnet run`

**Questions?** Check `IMPLEMENTATION_GUIDE.md` for detailed documentation.

**Version**: 1.0.0 | **Status**: ✅ Production Ready

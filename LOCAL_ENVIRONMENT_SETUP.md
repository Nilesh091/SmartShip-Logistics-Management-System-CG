# SmartShip Local Environment Setup Guide

This guide provides step-by-step instructions for running the SmartShip Logistics Management System locally without Docker.

## ✅ Prerequisites

### Required Software

- **.NET 10.0 SDK** - [Download](https://dotnet.microsoft.com/download)
- **SQL Server 2019+** or **SQL Server Express** - [Download](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- **RabbitMQ Server 3.x** - [Download](https://www.rabbitmq.com/download.html)
- **Git** - [Download](https://git-scm.com/)
- **Visual Studio 2022** or **Visual Studio Code** with C# extensions

### Verify Installation

```bash
# Check .NET installation
dotnet --version

# Check SQL Server (Windows)
sqlcmd -S localhost -U sa -P "2004@Nilu" -Q "SELECT @@VERSION"
```

---

## 🗄️ Database Setup

### 1. SQL Server Configuration

#### On Windows (using SQL Server Management Studio):

1. Open **SQL Server Management Studio**
2. Connect to `localhost` (or `localhost,1433` with authentication)
3. Create the following databases:

```sql
-- Create databases
CREATE DATABASE SmartLogisticsAuthServiceDb;
CREATE DATABASE SmartLogisticsShipmentServiceDb;
CREATE DATABASE SmartLogisticsTrackingServiceDb;
CREATE DATABASE SmartLogisticsAdminServiceDb;

-- Verify creation
SELECT name FROM sys.databases WHERE name LIKE 'SmartLogistics%';
```

#### On macOS/Linux (using Docker or local SQL Server):

```bash
# Pull and run SQL Server in Docker (if Docker is available)
docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=2004@Nilu' \
  -p 1433:1433 --name sqlserver2019 \
  -d mcr.microsoft.com/mssql/server:2019-latest

# Then run the SQL script above
```

### 2. Connection String Reference

All services use the following connection string pattern:

```
Server=localhost,1433;Database=<DatabaseName>;User Id=sa;Password=2004@Nilu;TrustServerCertificate=True;
```

**Credentials:**

- **Server:** localhost,1433 (or localhost:1433)
- **Username:** sa
- **Password:** 2004@Nilu
- **Trust Certificate:** True (for local development)

---

## 🐰 RabbitMQ Setup

### 1. Install RabbitMQ

#### On Windows:

1. Download RabbitMQ installer from [rabbitmq.com](https://www.rabbitmq.com/download.html)
2. Run the installer
3. Ensure Erlang is installed (installer prompts for this)
4. RabbitMQ will start automatically

#### On macOS (using Homebrew):

```bash
brew install rabbitmq
brew services start rabbitmq
```

#### On Linux (Ubuntu/Debian):

```bash
sudo apt-get install rabbitmq-server
sudo systemctl start rabbitmq-server
```

### 2. Verify RabbitMQ is Running

```bash
# Windows Command Prompt
rabbitmqctl status

# macOS/Linux
sudo rabbitmqctl status

# Or access the Management UI
# http://localhost:15672
# Default credentials: guest / guest
```

### 3. RabbitMQ Configuration for Local Development

**Default Configuration (Used in Development):**

- **Hostname:** localhost
- **Port:** 5672 (AMQP)
- **Management UI Port:** 15672
- **Username:** guest
- **Password:** guest

**Enable Management Plugin (if not already enabled):**

```bash
# Windows
rabbitmq-plugins.bat enable rabbitmq_management

# macOS/Linux
sudo rabbitmq-plugins enable rabbitmq_management
```

Access Management UI: http://localhost:15672

---

## 🚀 Running the Services Locally

### Local Service Ports

```
API Gateway          → http://localhost:5166
Auth Service         → http://localhost:5001
Admin Service        → http://localhost:5002
Shipment Service     → http://localhost:5003
Tracking Service     → http://localhost:5004
Notification Service → http://localhost:5005
```

### Method 1: Using Visual Studio 2022

1. **Open the Solution**

   ```
   SmartShip.sln (Root level)
   ```

2. **Set Multiple Startup Projects**
   - Right-click Solution → Properties
   - Select "Multiple startup projects"
   - Set Action to "Start" for:
     - `Gateway.API`
     - `AuthService.API`
     - `ShipmentService.API`
     - `TrackingService.API`
     - `AdminService.API`
     - `NotificationService.API` (optional)

3. **Configure launchSettings.json (if needed)**
   Each service's `Properties/launchSettings.json` should specify:

   ```json
   "profiles": {
     "Development": {
       "commandName": "Project",
       "dotnetRunMessages": true,
       "applicationUrl": "http://localhost:<PORT>",
       "environmentVariables": {
         "ASPNETCORE_ENVIRONMENT": "Development"
       }
     }
   }
   ```

4. **Run**
   - Press `F5` or click **Start**

### Method 2: Using .NET CLI

**Terminal 1 - Auth Service:**

```bash
cd Services/AuthService/AuthService.API
dotnet run --configuration Development
```

**Terminal 2 - Shipment Service:**

```bash
cd Services/ShipmentService/ShipmentService.API
dotnet run --configuration Development
```

**Terminal 3 - Tracking Service:**

```bash
cd Services/TrackingService/TrackingService.API
dotnet run --configuration Development
```

**Terminal 4 - Admin Service:**

```bash
cd Services/AdminService/AdminService.API
dotnet run --configuration Development
```

**Terminal 5 - Notification Service (optional):**

```bash
cd Services/NotificationService/NotificationService.API
dotnet run --configuration Development
```

**Terminal 6 - Gateway:**

```bash
cd Gateway/Gateway.API
dotnet run --configuration Development
```

### Method 3: Using VS Code Tasks

Create `.vscode/tasks.json` for automated service startup (see section below).

---

## 🔧 Configuration Files

### Application Settings

#### Auth Service (`Services/AuthService/AuthService.API/appsettings.Development.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=SmartLogisticsAuthServiceDb;User Id=sa;Password=2004@Nilu;TrustServerCertificate=True;"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest"
  }
}
```

#### Shipment Service (`Services/ShipmentService/ShipmentService.API/appsettings.Development.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=SmartLogisticsShipmentServiceDb;User Id=sa;Password=2004@Nilu;TrustServerCertificate=True;"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest"
  }
}
```

#### Tracking Service (`Services/TrackingService/TrackingService.API/appsettings.Development.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=SmartLogisticsTrackingServiceDb;User Id=sa;Password=2004@Nilu;TrustServerCertificate=True;"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest"
  }
}
```

#### Notification Service (`Services/NotificationService/NotificationService.API/appsettings.Development.json`)

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest"
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "vivek250500@gmail.com",
    "Password": "ugsg nzcc sumd eteb",
    "FromEmail": "vivek250500@gmail.com",
    "FromName": "SmartShip Notifications"
  }
}
```

#### Gateway (`Gateway/Gateway.API/appsettings.Development.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Ocelot": "Debug"
    }
  }
}
```

#### Gateway Routes (`Gateway/Gateway.API/ocelot.json`)

All routes configured for `localhost` with local ports:

- Auth Service: `localhost:5001`
- Admin Service: `localhost:5002`
- Shipment Service: `localhost:5003`
- Tracking Service: `localhost:5004`

---

## 🔍 Verification Checklist

### Before Running Services

- [ ] .NET 10.0 SDK installed
- [ ] SQL Server running
- [ ] Databases created
- [ ] RabbitMQ running and accessible
- [ ] Connection strings match local configuration

### After Running Services

- [ ] Gateway accessible: http://localhost:5166
- [ ] Auth Service: http://localhost:5001/swagger
- [ ] Shipment Service: http://localhost:5003/swagger
- [ ] Tracking Service: http://localhost:5004/swagger
- [ ] Admin Service: http://localhost:5002/swagger
- [ ] RabbitMQ Management: http://localhost:15672
- [ ] Database connections verified in logs

---

## 🛠️ Troubleshooting

### SQL Server Connection Issues

```
Error: "Cannot open database 'SmartLogisticsAuthServiceDb' requested by login"
Solution: Verify database was created and connection string is correct
```

### RabbitMQ Connection Issues

```
Error: "Connection refused to localhost:5672"
Solution:
1. Verify RabbitMQ is running: rabbitmqctl status
2. Check firewall allows port 5672
3. Restart RabbitMQ service
```

### Service Won't Start

```
Error: "Address already in use"
Solution:
1. Check port not already in use: netstat -ano (Windows) or lsof -i :5001 (macOS/Linux)
2. Kill process using the port
3. Use different port in launchSettings.json
```

### Database Migrations

If using Entity Framework migrations:

```bash
# In the service directory
dotnet ef database update

# Or apply migrations manually through SQL Server Management Studio
```

---

## 📱 Testing the System

### 1. Gateway Health Check

```bash
curl http://localhost:5166/health
```

### 2. Access Swagger UI

- Gateway Swagger: http://localhost:5166/swagger
- Individual service Swagger endpoints available via gateway

### 3. Sample API Call

```bash
# Get all shipments
curl http://localhost:5166/shipments/api/shipments \
  -H "Authorization: Bearer <YOUR_JWT_TOKEN>"
```

---

## 📝 Environment Variables

Create a `.env` file in the root directory for sensitive data:

```env
DB_PASSWORD=2004@Nilu
SMTP_PASSWORD=ugsg nzcc sumd eteb
```

Reference these in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=<DB>;User Id=sa;Password=${DB_PASSWORD};TrustServerCertificate=True;"
  }
}
```

---

## 🔒 Security Considerations for Development

⚠️ **These settings are for LOCAL DEVELOPMENT ONLY**

- Default SQL Server password: `2004@Nilu`
- RabbitMQ uses default credentials: `guest/guest`
- All services run without HTTPS in Development
- Swagger UI exposed (disable in Production)

**For Production:**

- Use strong, unique passwords
- Enable SSL/HTTPS
- Use Azure Key Vault or similar for secrets management
- Disable Swagger UI
- Configure proper authentication and authorization

---

## 📚 Additional Resources

- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [SQL Server Documentation](https://docs.microsoft.com/en-us/sql/sql-server/)
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [Ocelot API Gateway](https://ocelot.readthedocs.io/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

---

## 📞 Support & Troubleshooting

For issues:

1. Check logs in Console output
2. Verify all prerequisites are installed
3. Ensure all services are running on correct ports
4. Check firewall settings
5. Review connection strings in appsettings.json
6. Enable debug logging in launchSettings.json

---

**Last Updated:** April 18, 2026  
**Status:** ✅ Local Environment Ready

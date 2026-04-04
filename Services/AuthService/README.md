# AuthService Setup and Testing Guide

## Project Structure

```
AuthService/
├── AuthService.API/           # REST API & Controllers
├── AuthService.Application/   # Business Logic & Services
├── AuthService.Domain/        # Domain Entities
└── AuthService.Infrastructure/ # Data Access & External Services
```

## Implementation Complete ✅

### 1. Infrastructure Layer (`AuthService.Infrastructure`)

- **AuthDbContext** - Entity Framework Core DbContext
- **PasswordHasher** - BCrypt password hashing and verification
- **ITokenService & TokenService** - JWT token generation
  - Access tokens (15 minutes expiry)
  - Refresh tokens (7 days expiry)

### 2. Application Layer (`AuthService.Application`)

- **IAuthService & AuthService** - Core authentication business logic
  - Register (Signup)
  - Login
  - Refresh Token
  - Revoke Token

### 3. API Layer (`AuthService.API`)

- **AuthController** - REST endpoints
  - POST `/api/auth/signup` - Register new user
  - POST `/api/auth/login` - Login with credentials
  - POST `/api/auth/refresh` - Get new access token using refresh token
  - POST `/api/auth/revoke` - Revoke a refresh token

### 4. JWT Configuration

- Configured in `appsettings.json` and `appsettings.Development.json`
- Middleware setup in `Program.cs`
- Default JWT settings:
  - Algorithm: HMAC SHA-256
  - Key: `your-super-secret-key-that-is-at-least-32-characters-long-for-security`
  - Issuer: `AuthService`
  - Audience: `AuthServiceAPI`

## Database Setup

### Prerequisites

- SQL Server (Express or Full)
- .NET 10.0 SDK

### Connection String

Update in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=AuthServiceDb;User Id=sa;Password=YourPassword123!;Encrypt=false;"
}
```

### Create Database & Run Migrations

```bash
cd AuthService.API
dotnet ef database update --project ../AuthService.Infrastructure
```

**Alternatively**, run the application first - migrations will be applied on startup if configured.

## Running the Service

```bash
cd AuthService.API
dotnet run
```

The service will start on:

- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

## API Testing

### 1. Signup (Register)

```bash
POST /api/auth/signup
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "SecurePassword123!"
}
```

**Response:**

```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "base64encodedtoken=="
}
```

### 2. Login

```bash
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "SecurePassword123!"
}
```

**Response:** Same as signup

### 3. Use Access Token

```bash
GET /api/protected-endpoint
Authorization: Bearer <accessToken>
```

### 4. Refresh Token

```bash
POST /api/auth/refresh?token=<refreshToken>
```

**Response:**

```json
{
  "accessToken": "newAccessToken...",
  "refreshToken": "newRefreshToken=="
}
```

### 5. Revoke Token

```bash
POST /api/auth/revoke?token=<refreshToken>
```

**Response:**

```json
{
  "success": true
}
```

## Gateway Integration

For OcelotAPI Gateway, add this route configuration:

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/auth/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 5000
        }
      ],
      "UpstreamPathTemplate": "/gateway/auth/{everything}",
      "UpstreamHttpMethod": ["Get", "Post", "Put", "Delete"]
    }
  ]
}
```

### Test via Gateway

```bash
POST /gateway/auth/signup    # Maps to AuthService /api/auth/signup
POST /gateway/auth/login     # Maps to AuthService /api/auth/login
POST /gateway/auth/refresh   # Maps to AuthService /api/auth/refresh
POST /gateway/auth/revoke    # Maps to AuthService /api/auth/revoke
```

## NuGet Dependencies

The following packages have been configured:

### AuthService.Infrastructure

- `BCrypt.Net-Next` v4.0.3 - Password hashing
- `Microsoft.EntityFrameworkCore` v10.0.0
- `Microsoft.EntityFrameworkCore.SqlServer` v10.0.0
- `System.IdentityModel.Tokens.Jwt` v8.2.0
- `Microsoft.IdentityModel.Tokens` v8.2.0

### AuthService.Application

- `Microsoft.EntityFrameworkCore` v10.0.0

### AuthService.API

- `Microsoft.AspNetCore.Authentication.JwtBearer` v10.0.1
- `System.IdentityModel.Tokens.Jwt` v8.2.0
- `Microsoft.IdentityModel.Tokens` v8.2.0
- `Microsoft.EntityFrameworkCore.SqlServer` v10.0.0

## Security Best Practices

⚠️ **Important for Production:**

1. **Change JWT Key**: Update the `Jwt:Key` in `appsettings.json` to a secure, random key

   ```
   Minimum 32 characters for HMAC SHA-256
   ```

2. **Use User Secrets** for development:

   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "Jwt:Key" "your-secure-key"
   ```

3. **Encrypt Connection Strings** in production

4. **Use HTTPS** in production

5. **Add Rate Limiting** for login attempts

6. **Implement HTTPS Redirect** properly configured

## Troubleshooting

### Database Connection Issues

- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure database user has proper permissions

### JWT Token Issues

- Verify `Jwt:Key`, `Jwt:Issuer`, and `Jwt:Audience` match in configuration
- Check token expiration time
- Validate token format (Bearer scheme)

### CORS Issues

- CORS is enabled for all origins in development
- Update CORS policy in `Program.cs` for production

## Testing with Postman/Insomnia

Import the provided `.http` file or create a collection with these endpoints:

1. **Signup** - POST to `/api/auth/signup`
2. **Login** - POST to `/api/auth/login`
3. **Refresh** - POST to `/api/auth/refresh?token=YOUR_REFRESH_TOKEN`
4. **Revoke** - POST to `/api/auth/revoke?token=YOUR_REFRESH_TOKEN`

Store tokens in environment variables for easy testing.

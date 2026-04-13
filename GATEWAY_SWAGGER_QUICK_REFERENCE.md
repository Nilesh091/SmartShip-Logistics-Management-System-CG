# SmartShip Gateway Swagger - Quick Reference Guide

**🔗 Gateway Swagger URL:** `http://localhost:5166/`

---

## Quick Start

### 1. Access Swagger UI

```
Open browser: http://localhost:5166/
```

### 2. Authorize with Token

1. Click **Authorize** button (top right)
2. Paste your JWT token from login response
3. Token format: `Bearer YOUR_JWT_TOKEN_HERE`

### 3. Test Endpoints

- Select an endpoint from the list
- Click **Try it out**
- Fill required parameters
- Click **Execute**

---

## Common Workflows

### Workflow 1: Register & Login

```
1. POST /auth/signup
   Body: {
     "username": "newuser",
     "email": "user@example.com",
     "password": "SecurePass123!"
   }

2. POST /auth/login
   Body: {
     "email": "user@example.com",
     "password": "SecurePass123!"
   }

   Response: {
     "accessToken": "eyJhbGciOiJ...",
     "refreshToken": "eyJhbGciOiJ...",
     "expiresIn": 3600
   }

3. Copy accessToken for next requests
```

### Workflow 2: Create & Track Shipment

```
1. Authorize with token from login

2. POST /shipments
   Body: {
     "recipientName": "John Doe",
     "recipientEmail": "john@example.com",
     "deliveryAddress": "123 Main St",
     "weight": 5.5,
     "items": [...]
   }

   Response: { "id": "shipment-uuid" }

3. GET /tracking/{shipmentId}
   Use the shipment-uuid from response

   Response: {
     "status": "IN_TRANSIT",
     "currentLocation": "Distribution Center",
     "lastUpdate": "2026-04-10T14:30:00Z"
   }
```

### Workflow 3: Admin Dashboard

```
1. Login as admin (admin@smartship.com)

2. GET /admin/dashboard
   Shows: Total shipments, active, delayed, failed counts

3. GET /admin/shipments/exceptions
   Shows: Problematic shipments needing attention

4. PUT /admin/shipments/{shipmentId}/resolve
   Resolve customer issues
```

---

## Endpoint Categories

### 🔐 Authentication (No auth required for login/signup)

- `POST /auth/signup` - Register new user
- `POST /auth/login` - Get JWT token
- `POST /auth/refresh?token=X` - Refresh expired token
- `POST /auth/revoke?token=X` - Logout

### 👥 User Management (ADMIN only)

- `GET /auth/admin/users` - List all users
- `PUT /auth/admin/users/{userId}` - Update user role

### 📊 Admin Dashboard (ADMIN only)

- `GET /admin/dashboard` - Overview metrics
- `GET /admin/shipments/exceptions` - Problem shipments
- `GET /admin/users` - All users
- `PUT /admin/users/{userId}` - Update user status
- `GET /admin/reports` - Analytics data

### 📦 Shipment Management

- `GET /shipments` - List all shipments
- `GET /shipments/{shipmentId}` - Get details
- `POST /shipments` - Create new shipment
- `PUT /shipments/{shipmentId}` - Update shipment
- `DELETE /shipments/{shipmentId}` - Cancel shipment

### 📍 Tracking

- `GET /tracking/{trackingId}` - Get tracking info
- `GET /tracking/shipment/{shipmentId}` - All updates
- `POST /tracking` - Create tracking record
- `PUT /tracking/{trackingId}` - Update status

---

## Response Status Codes

| Code | Meaning      | Example                  |
| ---- | ------------ | ------------------------ |
| 200  | Success      | GET request succeeded    |
| 201  | Created      | New shipment created     |
| 204  | No Content   | DELETE succeeded         |
| 400  | Bad Request  | Missing required field   |
| 401  | Unauthorized | Invalid or missing token |
| 403  | Forbidden    | Insufficient permissions |
| 404  | Not Found    | Shipment not found       |
| 500  | Server Error | Internal service error   |

---

## Token Management

### Get Token (Login)

```json
POST /auth/login
{
  "email": "user@example.com",
  "password": "password123"
}

Response:
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

### Use Token

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Refresh Token (Before expiry)

```
POST /auth/refresh?token=REFRESH_TOKEN_HERE
```

---

## Error Handling

### Common Errors

**401 Unauthorized**

```json
{
  "error": "Invalid or expired token",
  "message": "Please login again"
}
```

**Solution:** Get new token via `POST /auth/login`

**403 Forbidden**

```json
{
  "error": "Insufficient permissions",
  "message": "ADMIN role required for this endpoint"
}
```

**Solution:** Contact admin to upgrade permissions

**400 Bad Request**

```json
{
  "errors": {
    "email": ["Email is required", "Invalid email format"]
  }
}
```

**Solution:** Check required fields and format

---

## Tips & Tricks

### 1. Save Tokens Locally

When you get a token from login, save it in Swagger:

- Click **Authorize**
- Paste the full token (without "Bearer" prefix)
- Check **Bearer** checkbox
- Click **Authorize** then **Close**

### 2. Test Required vs Optional Fields

- **Bold** parameters = Required
- Regular parameters = Optional

### 3. Try with Sample Data

```json
{
  "recipientName": "Test Customer",
  "recipientEmail": "test@example.com",
  "recipientPhone": "+1-555-0123",
  "deliveryAddress": "123 Test St, Test City, TC 12345"
}
```

### 4. Use Try It Out Feature

Each endpoint has **Try it out** button to:

- Edit request body
- Add/change parameters
- See actual request sent
- View response details

### 5. Check Examples

Many endpoints show example requests/responses in Swagger UI

---

## Authentication Roles

### CUSTOMER

- Can create shipments
- Can track own shipments
- Can update own profile
- Cannot access admin endpoints

### ADMIN

- Full access to all endpoints
- Can view all shipments (not just own)
- Can view all users
- Can resolve exceptions
- Can access dashboard

---

## Rate Limits

_(If configured)_

- 100 requests per minute per IP
- 1000 requests per hour per user
- Contact support for higher limits

---

## Service Ports Reference

| Service          | URL                   | Port |
| ---------------- | --------------------- | ---- |
| **Gateway**      | http://localhost:5166 | 5166 |
| Auth Service     | http://localhost:5045 | 5045 |
| Admin Service    | http://localhost:5127 | 5127 |
| Tracking Service | http://localhost:5062 | 5062 |
| Shipment Service | http://localhost:5286 | 5286 |

---

## Direct Service Access (Optional)

If needed, you can bypass the gateway:

```bash
# Direct Auth Service
curl http://localhost:5045/swagger/index.html

# Direct Admin Service
curl http://localhost:5127/swagger/index.html

# Direct Tracking Service
curl http://localhost:5062/swagger/index.html

# Direct Shipment Service
curl http://localhost:5286/swagger/index.html
```

---

## Test Accounts

### Admin Account

```
Email: admin@smartship.com
Password: Admin@123
Role: ADMIN
```

### Sample Customer Account

```
Email: customer@smartship.com
Password: Customer@123
Role: CUSTOMER
```

---

## Additional Resources

- **Full Documentation:** See `SWAGGER_ENDPOINTS_ANALYSIS.md`
- **Implementation Details:** See `GATEWAY_SWAGGER_IMPLEMENTATION.md`
- **API Health Status:** `GET /health` (if available)
- **Service Status:** Individual service `/health` endpoints

---

## Troubleshooting

### Swagger UI not loading?

- ✅ Ensure running in Development environment
- ✅ Check port 5166 is accessible
- ✅ Try clearing browser cache
- ✅ Restart the gateway service

### Cannot authorize?

- ✅ Verify token is valid (hasn't expired)
- ✅ Check exact token format from login response
- ✅ Ensure no extra spaces in token

### 401 Errors on endpoints?

- ✅ Click Authorize and add token
- ✅ Verify endpoint requires authentication (🔒 icon)
- ✅ Check token hasn't expired

### 403 Errors?

- ✅ Verify you have correct role (ADMIN/CUSTOMER)
- ✅ Contact administrator to upgrade permissions

---

**Happy API Testing! 🚀**

For issues or questions, check the comprehensive documentation files or contact the development team.

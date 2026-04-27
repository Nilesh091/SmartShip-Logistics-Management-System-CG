# ADMIN-Authorized Endpoints - All Services via Gateway

This document lists all endpoints accessible to ADMIN users through the API Gateway with complete payload structures.

---

## Gateway Routing Configuration

| Service          | Upstream Route            | Downstream Route              | Port | Authentication |
| ---------------- | ------------------------- | ----------------------------- | ---- | -------------- |
| Admin Service    | `/admin/{everything}`     | `/api/Admin/{everything}`     | 5127 | Bearer (JWT)   |
| Auth Service     | `/auth/{everything}`      | `/api/auth/{everything}`      | 5045 | Bearer (JWT)   |
| Shipment Service | `/shipments/{everything}` | `/api/shipments/{everything}` | 5286 | Bearer (JWT)   |
| Tracking Service | `/tracking/{everything}`  | `/api/Tracking/{everything}`  | 5062 | Bearer (JWT)   |

---

## 1. ADMIN SERVICE ENDPOINTS

### Base URL: `http://localhost:5166/admin` (through Gateway)

### Direct URL: `http://localhost:5127/api/Admin`

#### 1.1 Get Dashboard

- **Gateway Route:** `GET /admin/dashboard`
- **Direct Route:** `GET /api/Admin/dashboard`
- **Authorization:** `[Authorize(Roles = "ADMIN")]`
- **Request Payload:** None
- **Response Payload:**
  ```json
  {
    // Dashboard data structure
  }
  ```

---

#### 1.2 Get Exception Shipments

- **Gateway Route:** `GET /admin/shipments/exceptions`
- **Direct Route:** `GET /api/Admin/shipments/exceptions`
- **Authorization:** `[Authorize(Roles = "ADMIN")]`
- **Request Payload:** None
- **Response Payload:**
  ```json
  {
    "data": [
      {
        "id": "uuid",
        "userId": "uuid",
        "status": "string (DRAFT|BOOKED|PICKED_UP|IN_TRANSIT|OUT_FOR_DELIVERY|DELIVERED)",
        "createdAt": "datetime",
        "updatedAt": "datetime or null",
        "senderAddress": {
          "city": "string",
          "state": "string"
        },
        "receiverAddress": {
          "city": "string",
          "state": "string"
        },
        "package": {
          "weight": "number"
        }
      }
    ],
    "count": "number"
  }
  ```

---

#### 1.3 Get All Shipments

- **Gateway Route:** `GET /admin/shipments`
- **Direct Route:** `GET /api/Admin/shipments`
- **Authorization:** `[Authorize(Roles = "ADMIN")]`
- **Request Payload:** None
- **Response Payload:**
  ```json
  {
    "data": [
      {
        "id": "uuid",
        "userId": "uuid",
        "status": "string (DRAFT|BOOKED|PICKED_UP|IN_TRANSIT|OUT_FOR_DELIVERY|DELIVERED)",
        "createdAt": "datetime",
        "updatedAt": "datetime or null",
        "senderAddress": {
          "city": "string",
          "state": "string"
        },
        "receiverAddress": {
          "city": "string",
          "state": "string"
        },
        "package": {
          "weight": "number"
        }
      }
    ],
    "count": "number"
  }
  ```

---

#### 1.4 Resolve Shipment

- **Gateway Route:** `PUT /admin/shipments/{id}/resolve`
- **Direct Route:** `PUT /api/Admin/shipments/{id}/resolve`
- **Path Parameters:**
  - `id` (uuid): Shipment ID
- **Authorization:** `[Authorize(Roles = "ADMIN")]`
- **Request Payload:** None
- **Response Payload:**
  ```json
  "Shipment resolved"
  ```

---

#### 1.5 Get All Users

- **Gateway Route:** `GET /admin/users`
- **Direct Route:** `GET /api/Admin/users`
- **Authorization:** `[Authorize(Roles = "ADMIN")]`
- **Request Payload:** None
- **Response Payload:**
  ```json
  {
    "success": "boolean",
    "message": "string",
    "data": [
      {
        "id": "uuid",
        "username": "string",
        "email": "string",
        "role": "string (ADMIN|CUSTOMER|OPERATOR)",
        "createdAt": "datetime"
      }
    ]
  }
  ```

---

#### 1.6 Update User Role

- **Gateway Route:** `PUT /admin/users/{id}`
- **Direct Route:** `PUT /api/Admin/users/{id}`
- **Path Parameters:**
  - `id` (uuid): User ID
- **Authorization:** `[Authorize(Roles = "ADMIN")]`
- **Request Payload:**
  ```json
  {
    "role": "string (ADMIN|CUSTOMER|OPERATOR)"
  }
  ```
- **Response Payload:**
  ```json
  {
    "success": "boolean",
    "message": "string",
    "data": [
      {
        "id": "uuid",
        "username": "string",
        "email": "string",
        "role": "string",
        "createdAt": "datetime"
      }
    ]
  }
  ```

---

#### 1.7 Get Reports

- **Gateway Route:** `GET /admin/reports`
- **Direct Route:** `GET /api/Admin/reports`
- **Authorization:** `[Authorize(Roles = "ADMIN")]`
- **Request Payload:** None
- **Response Payload:**
  ```json
  {
    "totalShipments": "number",
    "deliveredShipments": "number",
    "failedShipments": "number",
    "inTransitShipments": "number",
    "deliveredPercentage": "decimal",
    "trends": ["string"]
  }
  ```

---

## 2. AUTH SERVICE ENDPOINTS

### Base URL: `http://localhost:5166/auth` (through Gateway)

### Direct URL: `http://localhost:5045/api/auth`

#### 2.1 Get All Users (ADMIN Only)

- **Gateway Route:** `GET /auth/admin/users`
- **Direct Route:** `GET /api/auth/admin/users`
- **Authorization:** `[Authorize(Roles = "ADMIN")]`
- **Request Payload:** None
- **Response Payload:**
  ```json
  {
    "success": "boolean",
    "message": "string",
    "data": [
      {
        "id": "uuid",
        "name": "string",
        "email": "string",
        "role": "string (ADMIN|CUSTOMER|OPERATOR)"
      }
    ]
  }
  ```

---

#### 2.2 Update User Role (ADMIN Only)

- **Gateway Route:** `PUT /auth/admin/users/{userId}`
- **Direct Route:** `PUT /api/auth/admin/users/{userId}`
- **Path Parameters:**
  - `userId` (uuid): User ID
- **Authorization:** `[Authorize(Roles = "ADMIN")]`
- **Request Payload:**
  ```json
  {
    "role": "string (ADMIN|CUSTOMER|OPERATOR)"
  }
  ```
- **Response Payload:**
  ```json
  {
    "success": "boolean",
    "message": "string",
    "data": [
      {
        "id": "uuid",
        "name": "string",
        "email": "string",
        "role": "string"
      }
    ]
  }
  ```

---

## 3. SHIPMENT SERVICE ENDPOINTS

### Base URL: `http://localhost:5166/shipments` (through Gateway)

### Direct URL: `http://localhost:5286/api/shipments`

#### 3.1 Get All Shipments (ADMIN Only)

- **Gateway Route:** `GET /shipments/admin/all`
- **Direct Route:** `GET /api/shipments/admin/all`
- **Authorization:** `[Authorize(Roles = "ADMIN")]`
- **Request Payload:** None
- **Response Payload:**
  ```json
  {
    "data": [
      {
        "id": "uuid",
        "userId": "uuid",
        "status": "string (DRAFT|BOOKED|PICKED_UP|IN_TRANSIT|OUT_FOR_DELIVERY|DELIVERED)",
        "createdAt": "datetime",
        "updatedAt": "datetime or null",
        "senderAddress": {
          "name": "string",
          "street": "string",
          "city": "string",
          "state": "string",
          "zipCode": "string"
        },
        "receiverAddress": {
          "name": "string",
          "street": "string",
          "city": "string",
          "state": "string",
          "zipCode": "string"
        },
        "package": {
          "weight": "number",
          "description": "string"
        }
      }
    ],
    "count": "number"
  }
  ```

---

#### 3.2 Update Shipment Status (ADMIN Only)

- **Gateway Route:** `PUT /shipments/{id}/status`
- **Direct Route:** `PUT /api/shipments/{id}/status`
- **Path Parameters:**
  - `id` (uuid): Shipment ID
- **Authorization:** `[Authorize(Roles = "ADMIN")]`
- **Request Payload:**
  ```json
  {
    "status": "string (DRAFT|BOOKED|PICKED_UP|IN_TRANSIT|OUT_FOR_DELIVERY|DELIVERED)"
  }
  ```
- **Response Payload:**
  ```json
  {
    "message": "Shipment status updated successfully."
  }
  ```

---

## 4. TRACKING SERVICE ENDPOINTS

### Base URL: `http://localhost:5166/tracking` (through Gateway)

### Direct URL: `http://localhost:5062/api/Tracking`

#### 4.1 Upload Document (ADMIN Only)

- **Gateway Route:** `POST /tracking/{shipmentId}/documents/upload`
- **Direct Route:** `POST /api/Tracking/{shipmentId}/documents/upload`
- **Path Parameters:**
  - `shipmentId` (uuid): Shipment ID
- **Authorization:** `[Authorize(Roles = "Admin")]`
- **Content-Type:** `multipart/form-data`
- **Request Payload:**
  ```
  - documentType (string): Invoice, ShippingLabel, Other
  - file (IFormFile): Document file (max 10MB)
  ```
- **Response Payload:**
  ```json
  {
    "documentId": "uuid",
    "shipmentId": "uuid",
    "documentType": "string",
    "fileName": "string",
    "filePath": "string",
    "fileSize": "number (bytes)",
    "uploadedAt": "datetime"
  }
  ```

---

## Summary Table

| #   | Service  | Endpoint            | Method | Authorization | Gateway Route                                  |
| --- | -------- | ------------------- | ------ | ------------- | ---------------------------------------------- |
| 1   | Admin    | Dashboard           | GET    | ADMIN         | `GET /admin/dashboard`                         |
| 2   | Admin    | Exception Shipments | GET    | ADMIN         | `GET /admin/shipments/exceptions`              |
| 3   | Admin    | All Shipments       | GET    | ADMIN         | `GET /admin/shipments`                         |
| 4   | Admin    | Resolve Shipment    | PUT    | ADMIN         | `PUT /admin/shipments/{id}/resolve`            |
| 5   | Admin    | All Users           | GET    | ADMIN         | `GET /admin/users`                             |
| 6   | Admin    | Update User Role    | PUT    | ADMIN         | `PUT /admin/users/{id}`                        |
| 7   | Admin    | Reports             | GET    | ADMIN         | `GET /admin/reports`                           |
| 8   | Auth     | Get All Users       | GET    | ADMIN         | `GET /auth/admin/users`                        |
| 9   | Auth     | Update User Role    | PUT    | ADMIN         | `PUT /auth/admin/users/{userId}`               |
| 10  | Shipment | Get All Shipments   | GET    | ADMIN         | `GET /shipments/admin/all`                     |
| 11  | Shipment | Update Status       | PUT    | ADMIN         | `PUT /shipments/{id}/status`                   |
| 12  | Tracking | Upload Document     | POST   | ADMIN         | `POST /tracking/{shipmentId}/documents/upload` |

---

## Authentication Header

All endpoints require a valid JWT token in the `Authorization` header:

```
Authorization: Bearer <JWT_TOKEN>
```

The JWT token must contain a claim with role = "ADMIN" (or "Admin" for Tracking Service).

---

## Error Responses

All services return error responses in the following format:

```json
{
  "message": "Error description",
  "error": "Detailed error information (Development only)"
}
```

HTTP Status Codes:

- `200 OK`: Successful request
- `400 Bad Request`: Invalid request parameters
- `401 Unauthorized`: Missing or invalid JWT token
- `403 Forbidden`: User is not ADMIN
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

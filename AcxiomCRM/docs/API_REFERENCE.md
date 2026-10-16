# REST API Reference & Contract

Base URL: `/api/v1`

All responses returning resource collections are paginated using `PaginationQuery` parameters (`page`, `pageSize`, `search`, `sortBy`, `sortDescending`).

---

## 1. Authentication Endpoints

### 1.1 POST `/api/v1/auth/login`
- **Anonymous**: Yes
- **Description**: Authenticates user credentials and issues an HttpOnly session cookie (`.AcxiomCRM.Auth`).
- **Request Body**:
  ```json
  {
    "email": "admin@acxiomcrm.local",
    "password": "CrmDev@2024Secure!",
    "rememberMe": true
  }
  ```
- **Responses**:
  - `200 OK`:
    ```json
    {
      "id": "e8d47cf7-802f-4889-8d77-628d0870951a",
      "email": "admin@acxiomcrm.local",
      "firstName": "System",
      "lastName": "Administrator",
      "fullName": "System Administrator",
      "isActive": true,
      "roles": ["Admin"]
    }
    ```
  - `400 Bad Request`: Invalid credentials, validation error, or locked account.

### 1.2 POST `/api/v1/auth/logout`
- **Authorization**: Required
- **Description**: Signs out the active user and clears the session cookie.
- **Response**: `200 OK`

### 1.3 GET `/api/v1/auth/me`
- **Authorization**: Required
- **Description**: Retrieves current authenticated user context and roles.
- **Response**: `200 OK` (UserDto) or `401 Unauthorized`.

### 1.4 GET `/api/v1/auth/csrf`
- **Anonymous**: Yes
- **Description**: Generates and issues an anti-forgery token in both response payload and `XSRF-TOKEN` cookie.
- **Response**:
  ```json
  {
    "token": "CfDJ8M...",
    "headerName": "X-XSRF-TOKEN"
  }
  ```

---

## 2. Customer Endpoints

### 2.1 GET `/api/v1/customers`
- **Authorization**: Required (Admin, Manager, SalesExecutive)
- **Query Parameters**: `page`, `pageSize`, `search`, `status`, `sortBy`, `sortDescending`
- **Responses**:
  - `200 OK`:
    ```json
    {
      "items": [
        {
          "customerId": "8a69a416-2fd1-4bc6-8d59-26beec197f26",
          "customerCode": "CUST-001",
          "customerName": "Acme Global Industries",
          "email": "contact@acme.com",
          "phone": "+15551234567",
          "companyName": "Acme Corp",
          "address": "123 Market St",
          "city": "San Francisco",
          "state": "CA",
          "status": 1,
          "createdDate": "2026-10-16T12:00:00Z",
          "updatedDate": null,
          "createdBy": "user-guid",
          "ownerId": "user-guid"
        }
      ],
      "page": 1,
      "pageSize": 10,
      "totalCount": 1,
      "totalPages": 1,
      "hasPreviousPage": false,
      "hasNextPage": false
    }
    ```

### 2.2 GET `/api/v1/customers/{id}`
- **Authorization**: Required
- **Responses**:
  - `200 OK`: CustomerDto
  - `403 Forbidden`: SalesExecutive attempting to view a customer assigned to another rep.
  - `404 Not Found`: Customer does not exist.

### 2.3 POST `/api/v1/customers`
- **Authorization**: Required
- **Request Body**:
  ```json
  {
    "customerCode": "CUST-002",
    "customerName": "Zenith Dynamics",
    "email": "info@zenithdynamics.com",
    "phone": "+15559876543",
    "companyName": "Zenith",
    "address": "456 Tech Parkway",
    "city": "Austin",
    "state": "TX"
  }
  ```
- **Responses**:
  - `201 Created`: Location header set to `/api/v1/customers/{id}`
  - `400 Bad Request`: Validation failure.
  - `409 Conflict`: Customer code or email already in use.

### 2.4 PUT `/api/v1/customers/{id}`
- **Authorization**: Required (Owner or Manager/Admin)
- **Response**: `200 OK`

### 2.5 DELETE `/api/v1/customers/{id}`
- **Authorization**: Required (Soft-deactivates customer record)
- **Response**: `204 NoContent`

---

## 3. Leads Endpoints

- `GET /api/v1/leads`: Returns paginated list of leads scoped to ownership/role.
- `GET /api/v1/leads/{id}`: Returns lead details.
- `POST /api/v1/leads`: Creates a new lead (Default status: `New`).
- `PATCH /api/v1/leads/{id}/status`: Transitions lead status (`New`, `Contacted`, `Qualified`, `Unqualified`, `Converted`).

---

## 4. Opportunities Endpoints

- `GET /api/v1/opportunities`: Returns paginated opportunities list.
- `GET /api/v1/opportunities/{id}`: Returns opportunity details.
- `POST /api/v1/opportunities`: Creates opportunity with validated amount and probability.
- `PATCH /api/v1/opportunities/{id}/stage`: Transitions stage (`Prospecting`, `Qualification`, `Proposal`, `Negotiation`, `ClosedWon`, `ClosedLost`).

---

## 5. Follow-Ups Endpoints

- `GET /api/v1/followups`: Returns planned and completed follow-up items.
- `POST /api/v1/followups`: Creates new follow-up associated with customer or lead.
- `PATCH /api/v1/followups/{id}/complete`: Marks follow-up as completed with remarks.
- `PATCH /api/v1/followups/{id}/reschedule`: Updates target date and marks rescheduled.

---

## 6. Dashboard & Reports Endpoints

- `GET /api/v1/dashboard/summary`: Scoped high-level metrics (total customers, open leads, pipeline value).
- `GET /api/v1/dashboard/lead-status`: Distribution of leads across statuses.
- `GET /api/v1/dashboard/opportunity-pipeline`: Distribution of pipeline across stages.
- `GET /api/v1/reports/customers`: Paginated report table of customer accounts.
- `GET /api/v1/reports/leads`: Paginated report table of lead progression.
- `GET /api/v1/reports/pipeline`: Pipeline forecast report with calculated weighted values.
- `GET /api/v1/reports/audit`: [Admin Only] Comprehensive system audit log report.

---

## 7. User Administration Endpoints

- `GET /api/v1/users`: [Admin Only] Paginated user accounts and role assignments.
- `POST /api/v1/users`: [Admin Only] Provisions a new user account with specified role.
- `PATCH /api/v1/users/{id}/status`: [Admin Only] Toggles account active/inactive state.
- `PATCH /api/v1/users/{id}/role`: [Admin Only] Reassigns user role.

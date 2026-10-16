# Validation and Authorization Reference

This reference documents the business rules, validation criteria, authorization constraints, and test scenarios across every domain module in **AcxiomCRM**.

---

## 1. Validation & Authorization Rules Matrix

| Feature | Field / Operation | Business Rule | Client-Side (React UX) | Server-Side Validation | Authorization Scope | HTTP Response | Test Scenario |
|---|---|---|---|---|---|---|---|
| **Auth** | Login | Valid Email format & password >= 6 chars | HTML5 email & min length | `LoginRequestValidator` (FluentValidation) | Anonymous | `200` or `400 BadRequest` | Invalid credentials / locked user |
| **Customer** | `CustomerCode` | Required, unique, alphanumeric with hyphen/underscore, max 30 chars | Regex pattern matching | `CreateCustomerRequestValidator` + DB unique index | Any authenticated user | `400` (format) / `409` (duplicate) | Duplicate code creation |
| **Customer** | `Email` | Required, valid email format, unique across customers, max 150 chars | Type email | `EmailAddress()` + DB unique index | Any authenticated user | `400` (format) / `409` (duplicate) | Duplicate email collision |
| **Customer** | `Phone` | Required, valid international phone (7-15 digits) | Regex digits | `Matches(@"^\+?[1-9]\d{1,14}$")` | Any authenticated user | `400 BadRequest` | Invalid character in phone |
| **Customer** | Read by ID | ID must exist and be accessible | Check guid format | Route constraint `{id:guid}` | Admin/Mgr: All; Sales: Assigned/Created only | `200` / `403` / `404` | Sales rep requesting other rep's customer |
| **Customer** | Deactivate | Soft-delete status to Inactive | Confirm modal dialog | Service verifies record state | Admin/Mgr: All; Sales: Assigned only | `204 NoContent` / `403` | Unauthorized deactivation |
| **Lead** | `LeadCode` | Unique, required, max 30 chars | Required field | `LeadCode` check in DbContext | Any authenticated user | `409 Conflict` | Duplicate lead code |
| **Lead** | `ExpectedValue` | Numeric currency value >= 0 | Number input min 0 | Decimal range check | Any authenticated user | `400 BadRequest` | Negative expected value |
| **Lead** | Status change | Valid enum transition | Dropdown selection | `UpdateLeadStatusValidator` | Admin/Mgr: All; Sales: Assigned only | `200 OK` / `403` | Status transition on unowned lead |
| **Opportunity**| `Amount` | Amount > 0 for active opportunity | Min value 0.01 | `GreaterThan(0)` FluentValidation | Any authenticated user | `400 BadRequest` | Zero or negative opportunity value |
| **Opportunity**| `Probability` | Integer between 0 and 100 inclusive | Range 0 to 100 | `InclusiveBetween(0, 100)` | Any authenticated user | `400 BadRequest` | Probability set to 120% |
| **Opportunity**| `ExpectedCloseDate`| Cannot be in the past for active deal | Min today's date | `GreaterThanOrEqualTo(Today)` | Any authenticated user | `400 BadRequest` | Setting past close date |
| **FollowUp** | `FollowUpDate` | Cannot be earlier than today for planned item | Date picker min today | `GreaterThanOrEqualTo(Today)` | Any authenticated user | `400 BadRequest` | Setting yesterday as new planned date |
| **FollowUp** | Entity Link | Must belong to either Customer or Lead | Form dropdown binding | Must have CustomerId OR LeadId | Any authenticated user | `400 BadRequest` | Neither ID provided |
| **Users** | User Provisioning | Only Admin can create users and assign roles | Admin portal only | Controller `[Authorize(Roles = "Admin")]` | Admin Only | `200 OK` / `403 Forbidden` | Sales rep calling POST `/api/v1/users` |
| **Audit** | Audit Log View | Only Admin can inspect audit logs | Admin portal only | Controller `[Authorize(Roles = "Admin")]` | Admin Only | `200 OK` / `403 Forbidden` | Manager calling `/api/v1/reports/audit` |

---

## 2. Role-Based Access vs Resource Ownership Matrix

| Operation | Admin | Manager | SalesExecutive | Enforced Layer |
|---|---|---|---|---|
| **View All Customers** | Yes | Yes | No (Only assigned/created) | Service query filter (`ApplyAuthorizationScope`) |
| **Create Customer** | Yes (Can assign to any rep) | Yes (Can assign to any rep) | Yes (Forced assignment to self) | `CustomerService.CreateCustomerAsync` |
| **Modify Customer** | Yes | Yes | Assigned only | `CustomerService.UpdateCustomerAsync` |
| **Deactivate Customer** | Yes | Yes | Assigned only | `CustomerService.DeactivateCustomerAsync` |
| **Manage Users & Roles** | Yes | No (`403 Forbidden`) | No (`403 Forbidden`) | Controller `[Authorize(Roles = "Admin")]` |
| **View Audit Trail** | Yes | No (`403 Forbidden`) | No (`403 Forbidden`) | Controller `[Authorize(Roles = "Admin")]` |
| **View Management Reports** | Yes | Yes | No (`403 Forbidden`) | Controller `[Authorize(Roles = "Admin,Manager")]` |
| **View Dashboard** | Global totals | Global totals | Scoped personal metrics only | `DashboardService` scoped counting |

---

## 3. ID Manipulation Protection (Horizontal Privilege Escalation)

When a `SalesExecutive` attempts to query or mutate an entity belonging to another sales executive:
1. The service filters queries by `AssignedTo == currentUserId`.
2. If the user calls `GET /api/v1/customers/{foreignId}`, the scoped query returns null.
3. The service explicitly verifies whether the record exists in the global dataset:
   - If it does not exist: returns `404 Not Found`.
   - If it exists in the database but does not belong to the user: throws `ForbiddenAccessException`, returning **`403 Forbidden`**.
4. This completely halts ID manipulation attacks in both query and mutation pathways.

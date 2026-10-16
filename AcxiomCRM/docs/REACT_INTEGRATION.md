# React Frontend Integration Guide

This guide describes how to connect an independent **React + TypeScript + Vite** frontend to the **AcxiomCRM** ASP.NET Core API running on a different port, domain, or container.

---

## 1. Project Organization & Recommended Structure

```
react-crm/
├── src/
│   ├── app/                         # App providers, router configuration
│   ├── components/
│   │   ├── layout/                  # MainLayout, Sidebar, Navbar
│   │   ├── ui/                      # Button, Input, Modal, Table
│   │   └── shared/                  # ProtectedRoute, RoleGuard
│   ├── features/
│   │   ├── auth/                    # Auth API, LoginForm, useAuth hook, types
│   │   ├── dashboard/               # Dashboard stats & charts
│   │   ├── customers/               # CustomerList, CustomerForm, useCustomers
│   │   ├── leads/                   # Lead Kanban, LeadModal
│   │   ├── opportunities/           # Pipeline view, Stage change
│   │   ├── followups/               # Calendar, FollowUpList, RescheduleModal
│   │   └── reports/                 # Reporting tables & exports
│   ├── lib/
│   │   ├── api-client.ts            # Authenticated Axios/Fetch client
│   │   └── query-client.ts          # TanStack React Query setup
│   ├── types/                       # Shared API DTO definitions
│   └── main.tsx
├── .env.example
├── package.json
└── vite.config.ts
```

---

## 2. Environment Configuration

In `.env.development`:
```env
VITE_API_BASE_URL=http://localhost:5000
```

In `vite.config.ts` (optional proxy setup for local dev without CORS):
```ts
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    // When using direct cross-origin CORS, proxy is optional.
    // If you wish to proxy to avoid cross-origin cookie issues locally:
    // proxy: {
    //   '/api': {
    //     target: 'http://localhost:5000',
    //     changeOrigin: true,
    //     secure: false,
    //   }
    // }
  }
});
```

---

## 3. Core API Client (`src/lib/api-client.ts`)

AcxiomCRM uses **Identity Cookie Authentication** with anti-forgery tokens. Requests must specify `credentials: "include"` (or `withCredentials: true` in Axios).

```typescript
import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true, // Send and receive HttpOnly cookies
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json'
  }
});

// Helper to fetch CSRF token before state-changing mutations
let csrfToken: string | null = null;

export async function ensureCsrfToken(): Promise<string> {
  if (csrfToken) return csrfToken;
  const res = await apiClient.get<{ token: string; headerName: string }>('/api/v1/auth/csrf');
  csrfToken = res.data.token;
  return csrfToken;
}

// Request interceptor to attach CSRF header on POST/PUT/DELETE/PATCH
apiClient.interceptors.request.use(async (config) => {
  const method = config.method?.toUpperCase();
  if (['POST', 'PUT', 'DELETE', 'PATCH'].includes(method || '')) {
    const token = await ensureCsrfToken();
    config.headers['X-XSRF-TOKEN'] = token;
  }
  return config;
});

// Response interceptor to handle session expiry (401)
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401 && window.location.pathname !== '/login') {
      // Redirect to login on unauthenticated
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
```

---

## 4. Authentication Integration

### `src/features/auth/api/auth.ts`
```typescript
import { apiClient } from '../../../lib/api-client';

export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  isActive: boolean;
  roles: string[];
}

export const authApi = {
  login: async (credentials: { email: string; password: string; rememberMe?: boolean }) => {
    const res = await apiClient.post<UserDto>('/api/v1/auth/login', credentials);
    return res.data;
  },
  logout: async () => {
    await apiClient.post('/api/v1/auth/logout');
  },
  getCurrentUser: async () => {
    const res = await apiClient.get<UserDto>('/api/v1/auth/me');
    return res.data;
  }
};
```

### `src/components/shared/ProtectedRoute.tsx`
```tsx
import React, { useEffect, useState } from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { authApi, UserDto } from '../../features/auth/api/auth';

interface ProtectedRouteProps {
  allowedRoles?: string[];
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ allowedRoles }) => {
  const [user, setUser] = useState<UserDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    authApi.getCurrentUser()
      .then(setUser)
      .catch(() => setUser(null))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div>Loading session...</div>;
  if (!user) return <Navigate to="/login" replace />;

  if (allowedRoles && !user.roles.some(r => allowedRoles.includes(r))) {
    return <Navigate to="/unauthorized" replace />;
  }

  return <Outlet context={{ user }} />;
};
```

---

## 5. Customer CRUD Integration Example

### Displaying Server-Side Field Validation Errors Next to Form Inputs
```tsx
import React, { useState } from 'react';
import { apiClient } from '../../../lib/api-client';

interface CustomerFormData {
  customerCode: string;
  customerName: string;
  email: string;
  phone: string;
  companyName?: string;
}

export const CustomerCreateForm: React.FC<{ onSuccess: () => void }> = ({ onSuccess }) => {
  const [formData, setFormData] = useState<CustomerFormData>({
    customerCode: '',
    customerName: '',
    email: '',
    phone: '',
    companyName: ''
  });

  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});
  const [generalError, setGeneralError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setFieldErrors({});
    setGeneralError(null);

    try {
      await apiClient.post('/api/v1/customers', formData);
      onSuccess();
    } catch (err: any) {
      if (err.response) {
        if (err.response.status === 400 && err.response.data.errors) {
          // Server validation errors mapped directly to form controls
          setFieldErrors(err.response.data.errors);
        } else if (err.response.status === 409) {
          setGeneralError(err.response.data.detail || 'Conflict: Record already exists.');
        } else {
          setGeneralError(err.response.data.detail || 'An unexpected error occurred.');
        }
      }
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {generalError && <div className="error-banner">{generalError}</div>}

      <div>
        <label>Customer Code</label>
        <input 
          value={formData.customerCode} 
          onChange={e => setFormData({ ...formData, customerCode: e.target.value })} 
        />
        {fieldErrors.CustomerCode && <span className="text-red-500">{fieldErrors.CustomerCode[0]}</span>}
      </div>

      <div>
        <label>Customer Name</label>
        <input 
          value={formData.customerName} 
          onChange={e => setFormData({ ...formData, customerName: e.target.value })} 
        />
        {fieldErrors.CustomerName && <span className="text-red-500">{fieldErrors.CustomerName[0]}</span>}
      </div>

      <div>
        <label>Email</label>
        <input 
          type="email"
          value={formData.email} 
          onChange={e => setFormData({ ...formData, email: e.target.value })} 
        />
        {fieldErrors.Email && <span className="text-red-500">{fieldErrors.Email[0]}</span>}
      </div>

      <div>
        <label>Phone</label>
        <input 
          value={formData.phone} 
          onChange={e => setFormData({ ...formData, phone: e.target.value })} 
        />
        {fieldErrors.Phone && <span className="text-red-500">{fieldErrors.Phone[0]}</span>}
      </div>

      <button type="submit">Create Customer</button>
    </form>
  );
};
```

---

## 6. Understanding HTTP Status Codes from AcxiomCRM

| Status Code | Meaning | React Frontend Handling |
|---|---|---|
| **200 OK** | Successful read or update | Update UI state or refetch data |
| **201 Created** | Record persisted | Navigate to detail view or trigger toast notification |
| **204 NoContent**| Deletion/Deactivation successful | Remove item from local table/list |
| **400 Bad Request** | Validation failed | Render `errors` object under corresponding inputs |
| **401 Unauthorized**| Missing or expired session | Redirect user to `/login` |
| **403 Forbidden** | Lack of permission or resource ownership | Render "Access Denied" view |
| **404 Not Found** | Record does not exist | Show 404 alert or redirect to list |
| **409 Conflict** | Duplicate key (e.g. Email/CustomerCode) | Display conflict message banner |
| **500 Internal Error**| Unexpected backend failure | Display friendly generic error dialog |

---

## 7. Cross-Origin (CORS) & Cookie Setup

1. **Origins vs Sites**:
   - `http://localhost:5173` (React) and `http://localhost:5000` (API) are different **origins** (due to ports).
   - In modern browsers, cookies with `SameSite=Lax` and `HttpOnly` are transmitted over cross-origin requests **only** when `credentials: "include"` (or Axios `withCredentials: true`) is specified, and the API responds with `Access-Control-Allow-Credentials: true` and an explicit origin (not `*`).

2. **Genuine Cross-Site Production Deployments**:
   - In production (e.g., `https://app.acxiomcrm.com` and `https://api.acxiomcrm.com`), both share the registrable domain `acxiomcrm.com` (same-site). Cookies work natively.
   - If deployed on distinct domains (e.g., `https://frontend.com` and `https://backend.com`), browsers enforce third-party cookie restrictions. In that architecture, use a Reverse Proxy (Nginx/Cloudflare) to route both under a common domain path, or configure an OAuth2 / OpenID Connect authorization server with Refresh Tokens.

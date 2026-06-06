# API Endpoints — App (Mobile) vs Web (MSI)

All endpoints live under `/api/v1/*` and are gated by JWT role claims.
Roles emitted in the JWT decide which client may call which endpoint:

| Client | JWT roles |
|---|---|
| **App** (AMS_Mobile, tenant-facing) | `Tenant`, `Owner` |
| **Web** (AMS_MSI, PM console) | `PropertyManager`, `Admin`, `Staff` |

`Shared` rows are callable by either client.

> Convention: `[Auth]` = JWT required, `[Anon]` = anonymous,
> `[Roles]` = restricted to listed roles.

---

## Auth — `/api/v1/auth/*` (Shared)

| Method | Path | Client | Auth | Notes |
|---|---|---|---|---|
| POST | `/register` | Shared | `[Anon]` | Create user (tenant signup or PM-invited Web user) |
| POST | `/login` | Shared | `[Anon]` | Returns access + refresh token + `is2FaRequired` |
| POST | `/refresh` | Shared | `[Anon]` | Refresh access token |
| POST | `/logout` | Shared | `[Auth]` | Single-device or `logoutAllDevices` |
| GET  | `/me` | Shared | `[Auth]` | Current user profile |
| GET  | `/sessions` | Shared | `[Auth]` | List active sessions |
| DELETE | `/sessions/{id}` | Shared | `[Auth]` | Revoke one session |
| POST | `/verify-email` | Shared | `[Anon]` | Confirm email via OTP code |
| POST | `/password/forgot` | Shared | `[Anon]` | Send reset email |
| POST | `/password/reset` | Shared | `[Anon]` | Reset with token |
| POST | `/otp/request` | App | `[Anon]` | Phone OTP (login + signup) |
| POST | `/otp/verify` | App | `[Anon]` | Verify phone OTP, issue tokens |
| POST | `/2fa/enable` | Shared | `[Auth]` | Provision TOTP secret |
| POST | `/2fa/verify` | Shared | `[Auth]` | Confirm TOTP, activate |
| POST | `/2fa/login` | Shared | `[Anon]` | Step-up login challenge |
| POST | `/2fa/backup-codes` | Shared | `[Auth]` | Regenerate backup codes |
| POST | `/sso/google` | App | `[Anon]` | Google ID token sign-in |
| GET  | `/audit-log` | Web | `[Roles: PM, Admin, Staff]` | Auth audit trail |
| GET  | `/invitations` | Web | `[Roles: PM, Admin]` | List pending invites |
| POST | `/invitations` | Web | `[Roles: PM, Admin]` | Send invite email |
| POST | `/invitations/{id}/resend` | Web | `[Roles: PM, Admin]` | Resend invite |
| DELETE | `/invitations/{id}` | Web | `[Roles: PM, Admin]` | Revoke invite |

---

## Customers — `/api/v1/customers/*` (Shared)

Tenants read their own profile; PMs manage everyone.

| Method | Path | Client | Auth | Notes |
|---|---|---|---|---|
| GET  | `/{customerId}/profile` | Shared | `[Auth]` | |
| POST | `/tenant` | Web | `[Auth]` | PM creates tenant |
| PUT  | `/{customerId}` | Shared | `[Auth]` | App: self; Web: any |
| GET  | `/{customerId}/addresses` | Shared | `[Auth]` | |
| POST | `/{customerId}/addresses` | Shared | `[Auth]` | |
| PUT  | `/addresses/{addressId}` | Shared | `[Auth]` | |
| PATCH | `/addresses/{addressId}/set-primary` | Shared | `[Auth]` | |
| DELETE | `/addresses/{addressId}` | Shared | `[Auth]` | |
| GET  | `/{customerId}/leases/history` | Shared | `[Auth]` | |
| GET  | `/leases/{leaseId}/documents` | Shared | `[Auth]` | |
| POST | `/leases/{leaseId}/documents` | Web | `[Auth]` | Multipart upload |
| POST | `/leases/{leaseId}/sign` | App | `[Auth]` | Tenant e-sign |
| GET  | `/{customerId}/notes` | Web | `[Auth]` | PM-only context |
| POST | `/{customerId}/notes` | Web | `[Auth]` | |
| PUT  | `/notes/{noteId}` | Web | `[Auth]` | |
| DELETE | `/notes/{noteId}` | Web | `[Auth]` | |
| GET  | `/{customerId}/communications` | Web | `[Auth]` | |
| POST | `/{customerId}/communications` | Web | `[Auth]` | |
| POST | `/leases/{leaseId}/move-in-checklist` | Web | `[Auth]` | Inspector creates |
| GET  | `/checklists/{checklistId}` | Shared | `[Auth]` | |
| GET  | `/search` | Web | `[Auth]` | |
| GET  | `/tenants` | Web | `[Auth]` | |
| GET  | `/properties/{propertyId}/tenants/active` | Web | `[Auth]` | |
| GET  | `/owners/{ownerId}/portfolio` | Shared | `[Auth]` | Owner: self; PM: any |

---

## Properties — `/api/v1/properties/*` (Web only)

| Method | Path | Client | Auth | Notes |
|---|---|---|---|---|
| GET  | `/` | Web | `[Roles: PM, Admin, Staff]` | List + filter |
| POST | `/` | Web | `[Roles: PM, Admin, Staff]` | |
| GET  | `/{id}` | Web | `[Roles: PM, Admin, Staff]` | |
| PUT  | `/{id}` | Web | `[Roles: PM, Admin, Staff]` | |
| DELETE | `/{id}` | Web | `[Roles: PM, Admin, Staff]` | |
| GET  | `/{id}/units` | Web | `[Roles: PM, Admin, Staff]` | |
| GET  | `/{id}/photos` | Web | `[Roles: PM, Admin, Staff]` | |
| POST | `/{id}/photos` | Web | `[Roles: PM, Admin, Staff]` | |
| GET  | `/{id}/owners` | Web | `[Roles: PM, Admin, Staff]` | |
| POST | `/{id}/owners` | Web | `[Roles: PM, Admin, Staff]` | |
| GET  | `/{id}/kpis` | Web | `[Roles: PM, Admin, Staff]` | Occupancy, revenue, open tickets |

---

## Products / Units — `/api/v1/products/*`

Catalog browse is anonymous (public marketing); mutations are PM-only.

| Method | Path | Client | Auth | Notes |
|---|---|---|---|---|
| GET  | `/units` | Shared | `[Anon]` | Public listing |
| GET  | `/units/available` | Shared | `[Anon]` | Public listing |
| GET  | `/{productId}` | Shared | `[Anon]` | |
| GET  | `/search` | Shared | `[Anon]` | |
| GET  | `/{productId}/photos` | Shared | `[Anon]` | |
| GET  | `/{id}/payment-breakdown` | App | `[Roles: Tenant, Owner]` | Rent + utilities + fees |
| GET  | `/by-property` | Web | `[Roles: PM, Admin, Staff]` | Filter by propertyId |
| POST | `/` | Web | `[Roles: Admin, Manager]` | |
| PUT  | `/{productId}` | Web | `[Roles: Admin, Manager]` | |
| PATCH | `/{productId}/status` | Web | `[Roles: Admin, Manager]` | |
| DELETE | `/{productId}` | Web | `[Roles: Admin, Manager]` | |
| PATCH | `/{productId}/maintenance` | Web | `[Roles: Admin, Manager]` | |
| GET  | `/{productId}/maintenance-requests` | Web | `[Roles: Admin, Manager]` | |
| POST | `/{productId}/photos` | Web | `[Roles: Admin, Manager]` | |
| PATCH | `/bulk-status` | Web | `[Roles: Admin, Manager]` | |

---

## Listings — `/api/v1/listings/*` (Web only)

| Method | Path | Client | Auth |
|---|---|---|---|
| GET / POST / GET-by-id / PUT / DELETE | `/`, `/{id}` | Web | `[Roles: PM, Admin, Staff]` |
| PATCH | `/{id}/publish`, `/{id}/unpublish`, `/{id}/feature` | Web | `[Roles: PM, Admin, Staff]` |

---

## Leases — `/api/v1/leases/*` (Shared, role-gated)

| Method | Path | Client | Auth | Notes |
|---|---|---|---|---|
| GET  | `/` | Web | `[Roles: PM, Admin, Staff]` | Top-level lease list |
| POST | `/` | Web | `[Roles: PM, Admin]` | PM creates |
| GET  | `/{id}` | Shared | `[Auth]` | Tenant: own; PM: any |
| PATCH | `/{id}/status` | Web | `[Roles: PM, Admin]` | |
| POST | `/{id}/send-for-signature` | Web | `[Roles: PM, Admin]` | |
| POST | `/{id}/sign` | App | `[Roles: Tenant, Owner, PM]` | E-sign |
| POST | `/{id}/renew`, `/{id}/terminate` | Web | `[Roles: PM, Admin]` | |
| GET  | `/{id}/documents` | Shared | `[Auth]` | |
| POST | `/{id}/documents` | Shared | `[Auth]` | |

---

## Maintenance — `/api/v1/maintenance/*` (mostly Web)

| Method | Path | Client | Auth | Notes |
|---|---|---|---|---|
| GET  | `/requests` | Web | `[Roles: PM, Admin, Staff]` | Flat kanban |
| POST | `/requests` | App | `[Auth]` | Tenant ticket |
| GET  | `/requests/{id}` | Shared | `[Auth]` | |
| PATCH | `/requests/{id}/status` | Web | `[Roles: PM, Admin, Staff]` | |
| PATCH | `/requests/{id}/assign` | Web | `[Roles: PM, Admin, Staff]` | |
| POST | `/requests/{id}/photos` | App | `[Roles: Tenant, Owner]` | |
| GET / POST | `/vendors` | Web | `[Roles: PM, Admin]` | |
| GET  | `/sla-summary` | Web | `[Roles: PM, Admin, Staff]` | |

---

## Invoices — `/api/v1/invoices/*`

| Method | Path | Client | Auth |
|---|---|---|---|
| GET  | `/` | Web | `[Roles: PM, Admin, Staff]` |
| POST | `/` | Web | `[Roles: PM, Admin]` |
| GET  | `/{id}` | Shared | `[Auth]` |
| PUT / DELETE | `/{id}` | Web | `[Roles: PM, Admin]` |
| POST | `/{id}/send`, `/{id}/void` | Web | `[Roles: PM, Admin]` |
| GET  | `/{id}/pdf` | Shared | `[Auth]` |
| GET  | `/outstanding` | App | `[Roles: Tenant, Owner]` |

---

## Payments — `/api/v1/payments/*`

| Method | Path | Client | Auth | Notes |
|---|---|---|---|---|
| GET  | `/` | Web | `[Roles: PM, Admin, Staff]` | |
| GET  | `/unmatched` | Web | `[Roles: PM, Admin, Staff]` | |
| POST | `/{id}/match` | Web | `[Roles: PM, Admin, Staff]` | |
| DELETE | `/matches/{matchId}` | Web | `[Roles: PM, Admin, Staff]` | |
| POST | `/` | App | `[Roles: Tenant, Owner]` | Tenant pays |
| GET  | `/{id}` | Shared | `[Auth]` | |
| GET  | `/history` | App | `[Roles: Tenant, Owner]` | |
| GET / POST | `/methods` | App | `[Roles: Tenant, Owner]` | |
| DELETE | `/methods/{id}` | App | `[Roles: Tenant, Owner]` | |
| POST | `/bakong-qr` | App | `[Roles: Tenant, Owner]` | |
| POST | `/bakong/webhook` | — | `[Anon]` (signature-verified) | Bakong → us |
| POST | `/{id}/confirm`, `/{id}/cancel` | App | `[Roles: Tenant, Owner]` | |
| GET  | `/{id}/receipt` | Shared | `[Auth]` | PDF |

---

## Messages — `/api/v1/conversations/*` (Shared)

All endpoints `[Auth]` — caller's role decides scope.

| Method | Path | Client |
|---|---|---|
| GET / POST | `/` | Shared |
| GET  | `/{id}` | Shared |
| GET  | `/{id}/messages` | Shared |
| POST | `/{id}/messages` | Shared |
| POST | `/{id}/read` | Shared |
| WS   | `/ws/messages` | Shared |

---

## Announcements — `/api/v1/announcements/*`

| Method | Path | Client | Auth |
|---|---|---|---|
| GET  | `/` | Shared | `[Auth]` (App reads feed; Web reads all) |
| POST | `/` | Web | `[Roles: PM, Admin, Staff]` |
| GET  | `/{id}` | Shared | `[Auth]` |
| POST | `/{id}/send-now` | Web | `[Roles: PM, Admin, Staff]` |
| GET  | `/{id}/deliveries` | Web | `[Roles: PM, Admin, Staff]` |

---

## Notifications, Push, Prefs

| Method | Path | Client | Auth |
|---|---|---|---|
| GET  | `/api/v1/notifications` | Shared | `[Auth]` |
| GET  | `/api/v1/notifications/unread-count` | Shared | `[Auth]` |
| POST | `/api/v1/notifications/{id}/read` | Shared | `[Auth]` |
| POST | `/api/v1/notifications/mark-all-read` | Shared | `[Auth]` |
| POST | `/api/v1/push/devices` | App | `[Roles: Tenant, Owner]` |
| DELETE | `/api/v1/push/devices/{token}` | App | `[Roles: Tenant, Owner]` |
| GET / PUT | `/api/v1/notification-prefs` | App | `[Roles: Tenant, Owner]` |

---

## Reports — `/api/v1/reports/*` (Web only)

All `[Roles: PM, Admin, Staff]`.

| Method | Path |
|---|---|
| GET | `/occupancy` |
| GET | `/income-expenses` |
| GET | `/maintenance-sla` |
| GET | `/export?type=&format=` |

---

## Settings + Audit Log — Web only

| Method | Path | Roles |
|---|---|---|
| GET / PUT | `/api/v1/settings/branding` | `PM, Admin` |
| POST | `/api/v1/settings/branding/logo` | `PM, Admin` |
| GET | `/api/v1/audit-log` | `PM, Admin, Staff` |

---

## Uploads — `/api/v1/uploads/*`

| Method | Path | Client |
|---|---|---|
| POST | `/profile-photo` | App |
| POST | `/maintenance-photo` | App |
| POST | `/property-photo` | Web |
| POST | `/document` | Shared |

---

## Health — `/health/*` (Anonymous, both clients)

| Method | Path |
|---|---|
| GET | `/status`, `/ready`, `/live` |

---

## Quick counts

| Area | App | Web | Shared | Total |
|---|---:|---:|---:|---:|
| Auth | 3 | 5 | 14 | 22 |
| Customers | 1 | 13 | 10 | 24 |
| Properties | 0 | 11 | 0 | 11 |
| Products | 1 | 9 | 5 | 15 |
| Listings | 0 | 8 | 0 | 8 |
| Leases | 1 | 6 | 2 | 9 |
| Maintenance | 2 | 6 | 1 | 9 |
| Invoices | 1 | 5 | 2 | 8 |
| Payments | 8 | 4 | 2 | 14 |
| Messages | 0 | 0 | 6 | 6 |
| Announcements | 0 | 3 | 2 | 5 |
| Notifications/Push/Prefs | 4 | 0 | 4 | 8 |
| Reports | 0 | 4 | 0 | 4 |
| Settings | 0 | 4 | 0 | 4 |
| Uploads | 2 | 1 | 1 | 4 |
| Health | 0 | 0 | 3 | 3 |
| **Total** | **23** | **79** | **52** | **154** |

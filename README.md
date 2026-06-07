# CMS Auth Service

Identity and user management microservice for a generic CMS platform. Handles authentication, authorization, user profiles, notifications, and activity logging. Issues JWT tokens consumed by every other service in the system.

## Microservices Overview

This is one of four microservices that together form the CMS platform:

| Service | Stack | Responsibility |
|---------|-------|---------------|
| **Auth** (this repo) | .NET 8, MySQL, Redis, Kafka, SignalR | Identity, authentication, user management, roles, profiles, notifications |
| **Content** (`../content`) | .NET 8, MySQL, Redis, Elasticsearch | Content entries, categories, tags, API key issuance for public access |
| **Assets** (`../assets`) | NestJS, MinIO (S3), Kafka | File upload to object storage, asset tracking, Kafka event publishing |
| **Organisations** (`../organisations`) | .NET 8, MySQL, Redis | Organisation CRUD, member management, role-based access within orgs, org-scoped API keys |

### How the services fit together

```
Client
  │
  ├─ POST /api/auth/login ──────────────► Auth Service
  │                                         issues JWT
  │
  ├─ Authorization: Bearer <jwt> ────────► Content / Organisations / Assets
  │                                         each service validates JWT independently
  │
  ├─ POST /files/upload ─────────────────► Assets Service
  │                                         uploads to MinIO, publishes files.uploaded to Kafka
  │
  └─ files.uploaded (Kafka) ─────────────► Auth Service
                                            updates avatarUrl on user profile
                                            pushes real-time notification via SignalR
```

All services validate JWTs on their own — there is no API gateway. The `sub` / `NameIdentifier` claim in the JWT carries the user ID; services never accept a `userId` from the request body.

---

## Features

- **Registration & Login** — email/password authentication, issues JWT (60 min) + refresh token (90 days)
- **Token Management** — refresh JWTs without re-authenticating; invalidate on logout
- **Two-Factor Auth (2FA)** — TOTP-based (Google Authenticator compatible); setup, confirm, disable, and complete login
- **Account Management** — update own email, username, or password
- **User Management** — admin CRUD over all users; list, get, update, delete (single or bulk), search with sorting and pagination
- **Role Management** — custom roles; assign/remove roles per user; `user` and `admin` seeded by default
- **User Profiles** — extended profile (display name, first/last name, avatar, bio, phone, timezone) with upsert semantics
- **Notifications** — create, list, mark-as-read (single or all), delete; real-time push via SignalR
- **Activity Logs** — automatic logging of key actions (login, logout, etc.); paginated admin access
- **Kafka Integration** — consumes `files.uploaded` events to update `avatarUrl` and push a notification to the affected user
- **Real-Time (SignalR)** — push notifications to connected clients via `/hubs/notifications`

---

## Authentication

| Type | Header | Usage |
|------|--------|-------|
| **JWT Bearer** | `Authorization: Bearer <token>` | All protected endpoints |

JWT is issued on login and carries the user ID (`sub`) and roles. Refresh tokens are stored in the database and used to issue new JWTs without re-authentication.

---

## API Reference

### Auth — `/api/auth`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/auth/register` | None | Create a new account |
| `POST` | `/api/auth/login` | None | Authenticate; returns JWT + refresh token (or `twoFactorId` if 2FA enabled) |
| `POST` | `/api/auth/logout` | JWT | Invalidate a refresh token |
| `POST` | `/api/auth/refresh` | None | Exchange refresh token for a new JWT |
| `GET` | `/api/auth/account` | JWT | Get current user's account info |
| `PUT` | `/api/auth/account` | JWT | Update email, username, or password |
| `POST` | `/api/auth/2fa/setup` | JWT | Generate TOTP secret and QR code URI |
| `POST` | `/api/auth/2fa/confirm` | JWT | Verify a TOTP code and enable 2FA |
| `POST` | `/api/auth/2fa/disable` | JWT | Disable 2FA with a valid TOTP code |
| `POST` | `/api/auth/2fa/login` | None | Complete a 2FA login using `twoFactorId` from login |

#### Login flow with 2FA

```
1. POST /api/auth/login  →  { twoFactorId: "guid", jwtToken: null, refreshToken: null }
2. POST /api/auth/2fa/login  →  { jwtToken: "...", refreshToken: "guid" }
```

---

### User Management — `/api/user` *(Admin only)*

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/user` | List all users (paginated) |
| `GET` | `/api/user/{id}` | Get a single user |
| `GET` | `/api/user/search` | Search/filter users by username, email, admin status, with sorting |
| `PUT` | `/api/user/{id}` | Update a user's email and username |
| `DELETE` | `/api/user/{id}` | Delete a user |
| `POST` | `/api/user/delete-bulk` | Delete multiple users at once |

Query params for search: `username`, `email`, `isAdmin`, `orderBy`, `descending`, `pageNumber`, `pageSize`.

---

### Roles — `/api/roles` *(Admin only)*

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/roles` | List all roles |
| `GET` | `/api/roles/{id}` | Get a single role |
| `POST` | `/api/roles` | Create a role |
| `PUT` | `/api/roles/{id}` | Update a role |
| `DELETE` | `/api/roles/{id}` | Delete a role |
| `GET` | `/api/roles/user/{userId}` | Get roles assigned to a user |
| `POST` | `/api/roles/user/{userId}` | Assign a role to a user |
| `DELETE` | `/api/roles/user/{userId}/{roleId}` | Remove a role from a user |

Default seeded roles: `user`, `admin`.

---

### Profile — `/api/profile` *(Requires JWT)*

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/profile` | JWT | Get current user's profile |
| `GET` | `/api/profile/{userId}` | Admin | Get a specific user's profile |
| `PUT` | `/api/profile` | JWT | Create or update profile (upsert) |

Profile fields: `displayName`, `firstName`, `lastName`, `avatarUrl`, `bio`, `phoneNumber`, `timezone`. All optional on update.

---

### Notifications — `/api/notifications` *(Requires JWT)*

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/notifications` | JWT | List all notifications for current user |
| `POST` | `/api/notifications` | Admin | Create a notification for a user |
| `PATCH` | `/api/notifications/{id}/read` | JWT | Mark a notification as read |
| `PATCH` | `/api/notifications/read-all` | JWT | Mark all notifications as read |
| `DELETE` | `/api/notifications/{id}` | JWT | Delete a notification |

Notifications are also pushed in real time via SignalR on creation, on login, and when triggered by Kafka events.

---

### Activity Logs — `/api/logs` *(Admin only)*

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/logs` | Paginated activity log; filter by `userId` |

Log fields: `id`, `userId`, `action`, `details`, `ipAddress`, `createdAt`.

---

### Real-Time Notifications (SignalR)

**Hub URL**: `/hubs/notifications`

Connect with the JWT in the query string:

```
/hubs/notifications?access_token={jwtToken}
```

Clients are joined to a group keyed by their `userId`. The server emits `ReceiveNotification` events.

```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${API_BASE}/hubs/notifications?access_token=${jwtToken}`)
  .withAutomaticReconnect()
  .build();

connection.on("ReceiveNotification", (notification) => {
  console.log(notification.message);
});

await connection.start();
```

---

## Response Format

**Success**
```json
{ "success": true, "data": { } }
```

**Error**
```json
{ "Code": 401, "Message": "Unauthorized" }
```

**Paginated**
```json
{
  "items": [],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 10
}
```

| HTTP Status | Meaning |
|-------------|---------|
| 200 | OK |
| 201 | Created |
| 204 | No Content |
| 400 | Bad Request |
| 401 | Unauthorized (missing or invalid JWT) |
| 403 | Forbidden (wrong role) |
| 404 | Not Found |
| 409 | Conflict (duplicate resource) |
| 500 | Internal Server Error |

---

## Architecture

Clean Architecture with four layers:

```
cmsUserManagment.API            HTTP controllers, middleware, SignalR hub, Dockerfile
cmsUserManagment.Application    DTOs, interfaces, use case definitions, exceptions
cmsUserManagment.Domain         Entities (User, Role, UserProfile, Notification, ActivityLog, ...)
cmsUserManagment.Infrastructure Repositories, EF Core, migrations, Kafka consumer, Redis, security
```

---

## Infrastructure

### MySQL 8 — primary database

All persistent state lives here. Tables:

| Table | Key columns | Notes |
|-------|------------|-------|
| `Users` | `Id`, `Email`, `Username`, `Password`, `IsTwoFactorEnabled`, `TwoFactorSecret` | Core identity; password is bcrypt-hashed |
| `RefreshTokens` | `Id`, `UserId`, `Expires` | 90-day tokens; deleted on logout |
| `TwoFactorAuthCodes` | `Id`, `UserId`, `Expires` | Temporary session ID issued during 2FA login; deleted after use |
| `Roles` | `Id`, `Name`, `Description` | Seeded with `user` and `admin` on startup |
| `UserRoles` | `UserId`, `RoleId`, `AssignedAt` | Composite PK; many-to-many join |
| `UserProfiles` | `UserId` (PK/FK), `DisplayName`, `FirstName`, `LastName`, `AvatarUrl`, `Bio`, `PhoneNumber`, `Timezone` | One-to-one with Users; created on registration, updated by Kafka on file upload |
| `Notifications` | `Id`, `UserId`, `Message`, `Type`, `IsRead`, `CreatedAt` | Cascade-deleted when user is deleted |
| `ActivityLogs` | `Id`, `UserId`, `Action`, `Details`, `IpAddress`, `CreatedAt` | `UserId` set to NULL on user delete (preserved for audit) |

### Redis 7 — caching layer

Three key namespaces, all serialized as JSON:

| Key pattern | What's stored | Set when | Invalidated when |
|-------------|--------------|----------|-----------------|
| `email:{email}` | `{ id, email, username, isAdmin, isTwoFactorEnabled }` | Login, register, 2FA toggle, account/user update | Logout, email change, user delete |
| `user:{id}` | Full `User` entity | `GET /api/user/{id}` (cache-aside) | User update, user delete |
| `search:{params}` | `PaginatedResult<User>` for the exact query | `GET /api/user/search` | Expires after 10 minutes (TTL only) |

### Kafka — async event consumer

Subscribes to the `files.uploaded` topic (published by the Assets service after a MinIO upload). On each message:
1. Parses `assetId` — format: `{userId}/{entryId}/{timestamp}-{filename}`
2. Extracts `userId` from the first path segment
3. Updates `UserProfile.AvatarUrl` in MySQL
4. Creates a notification and pushes it via SignalR to the affected user

### SignalR — real-time push

Hub at `/hubs/notifications`. Clients pass the JWT via `?access_token=`. Connected clients are grouped by `userId`; the server calls `ReceiveNotification` on the group when:
- A notification is created via the API
- The user logs in (login welcome message)
- A Kafka `files.uploaded` event is processed for that user

---

## Running Locally

**Prerequisites**: .NET 8 SDK, Docker

```bash
# Start MySQL, Redis, and Kafka
docker compose up -d

# Run the API
dotnet run --project cmsUserManagment.API
```

API available at `http://localhost:5055`  
Swagger UI at `http://localhost:5055/swagger`

---

## Quick Examples

### Register

```bash
curl -X POST http://localhost:5055/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{ "email": "user@example.com", "username": "user1", "password": "password123" }'
```

### Login

```bash
curl -X POST http://localhost:5055/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{ "email": "user@example.com", "password": "password123" }'
```

### Refresh token

```bash
curl -X POST http://localhost:5055/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{ "refreshToken": "<guid>" }'
```

### Search users (Admin)

```bash
curl "http://localhost:5055/api/user/search?email=example&pageSize=5" \
  -H "Authorization: Bearer <jwt>"
```

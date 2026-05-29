# CMS User Management Service

**Stack**: .NET 8 ASP.NET Core · MySQL 8 · Redis 7 · Kafka · SignalR  
**Auth**: JWT Bearer (60 min expiry) + Refresh tokens (90 days)

---

## What This Service Can Do

| Capability | Description |
|------------|-------------|
| **Registration & Login** | Create accounts, authenticate with email/password, issue JWT + refresh tokens |
| **Token Management** | Refresh JWTs using a long-lived refresh token; invalidate tokens on logout |
| **Two-Factor Auth (2FA)** | TOTP-based 2FA (Google Authenticator compatible) — setup, enable, disable, and complete login |
| **Account Management** | Update own email, username, or password |
| **User Management** | Admin CRUD over all users — list, get, update, delete (single or bulk), search/filter with sorting and pagination |
| **Role Management** | Create custom roles; assign/remove roles per user; `user` and `admin` seeded by default |
| **User Profiles** | Extended profile data (name, bio, avatar, phone, timezone) with upsert semantics |
| **Notifications** | Create, list, mark-as-read (single or all), and delete notifications; real-time push via SignalR |
| **Activity Logs** | Automatic logging of key actions (login, logout, etc.); paginated admin access |
| **Kafka Integration** | Consumes `files.uploaded` events to update `avatarUrl` and push a notification to the affected user |
| **Real-Time (SignalR)** | Push notifications to connected clients via `/hubs/notifications` |

---

## Standard Response Shapes

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

## Auth — `/api/auth`

### `POST /api/auth/register`

Create a new user account. Assigns the default `user` role automatically.

**Body**
```json
{ "email": "string", "username": "string", "password": "string" }
```

**Returns** `200`
```json
{ "success": true, "data": null }
```

---

### `POST /api/auth/login`

Authenticate and receive tokens. When 2FA is enabled, tokens are withheld and a `twoFactorId` is returned instead.

**Body**
```json
{ "email": "string", "password": "string" }
```

**Returns** `200`
```json
{
  "success": true,
  "data": {
    "jwtToken": "string | null",
    "refreshToken": "guid | null",
    "twoFactorId": "guid | null"
  }
}
```

> `jwtToken` and `refreshToken` are null when 2FA is required. Use `twoFactorId` with `POST /api/auth/2fa/login`.

---

### `POST /api/auth/logout`

Invalidate a refresh token. Requires JWT.

**Body**
```json
{ "refreshToken": "guid" }
```

**Returns** `200`
```json
{ "success": true, "data": null }
```

---

### `POST /api/auth/refresh`

Exchange a refresh token for a new JWT.

**Body**
```json
{ "refreshToken": "guid" }
```

**Returns** `200`
```json
{ "success": true, "data": "new-jwt-string" }
```

---

### `GET /api/auth/account`

Get the current user's account info. Requires JWT.

**Returns** `200`
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "email": "string",
    "username": "string",
    "isTwoFactorEnabled": false
  }
}
```

---

### `PUT /api/auth/account`

Update email, username, or password. Requires JWT. All fields are optional.

**Body**
```json
{
  "email": "string?",
  "username": "string?",
  "currentPassword": "string?",
  "newPassword": "string?"
}
```

**Returns** `200`
```json
{ "success": true, "data": null }
```

---

### `POST /api/auth/2fa/setup`

Generate a TOTP secret and QR code URI. Requires JWT.

**Returns** `200`
```json
{
  "success": true,
  "data": {
    "qrCodeUri": "otpauth://totp/...",
    "manualKey": "BASE32SECRET"
  }
}
```

---

### `POST /api/auth/2fa/confirm`

Verify a TOTP code and enable 2FA on the account. Requires JWT.

**Body**
```json
{ "code": "string" }
```

**Returns** `200`
```json
{ "success": true, "data": null }
```

---

### `POST /api/auth/2fa/disable`

Disable 2FA with a valid TOTP code. Requires JWT.

**Body**
```json
{ "code": "string" }
```

**Returns** `200`
```json
{ "success": true, "data": null }
```

---

### `POST /api/auth/2fa/login`

Complete a 2FA login flow after receiving a `twoFactorId` from `POST /login`.

**Body**
```json
{ "loginId": "guid", "code": "string" }
```

**Returns** `200`
```json
{
  "success": true,
  "data": {
    "jwtToken": "string",
    "refreshToken": "guid"
  }
}
```

---

## User Management — `/api/user` *(Admin only)*

### `GET /api/user`

List all users with pagination.

**Query params**: `pageNumber` (default 1), `pageSize` (default 10)

**Returns** `200` — `PaginatedResult<User>`
```json
{
  "items": [
    { "id": "guid", "email": "string", "username": "string", "isTwoFactorEnabled": false }
  ],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 10
}
```

---

### `GET /api/user/{id}`

Get a single user by ID.

**Returns** `200` — `User`
```json
{ "id": "guid", "email": "string", "username": "string", "isTwoFactorEnabled": false }
```

---

### `GET /api/user/search`

Search and filter users.

**Query params**

| Param | Type | Description |
|-------|------|-------------|
| `username` | string? | Partial match |
| `email` | string? | Partial match |
| `isAdmin` | bool? | Filter by admin role |
| `orderBy` | string? | Sort field (default `username`) |
| `descending` | bool | Sort direction (default false) |
| `pageNumber` | int | Default 1 |
| `pageSize` | int | Default 10 |

**Returns** `200` — `PaginatedResult<User>` (same shape as `GET /api/user`)

---

### `PUT /api/user/{id}`

Update a user's email and username.

**Body**
```json
{ "username": "string", "email": "string" }
```

**Returns** `200` — `bool`

---

### `DELETE /api/user/{id}`

Delete a single user.

**Returns** `200` — `bool`

---

### `POST /api/user/delete-bulk`

Delete multiple users at once.

**Body**
```json
["guid", "guid", "..."]
```

**Returns** `200` — `bool`

---

## Roles — `/api/roles` *(Admin only)*

### `GET /api/roles`

List all roles.

**Returns** `200` — `RoleResponse[]`
```json
[{ "id": "guid", "name": "string", "description": "string?", "createdAt": "datetime" }]
```

---

### `GET /api/roles/{id}`

Get a single role by ID.

**Returns** `200` — `RoleResponse` · `404` if not found

---

### `POST /api/roles`

Create a new role.

**Body**
```json
{ "name": "string", "description": "string?" }
```

**Returns** `201` — `RoleResponse`
```json
{ "id": "guid", "name": "string", "description": "string?", "createdAt": "datetime" }
```

---

### `PUT /api/roles/{id}`

Update an existing role. All fields optional.

**Body**
```json
{ "name": "string?", "description": "string?" }
```

**Returns** `200` — `RoleResponse` · `404` if not found

---

### `DELETE /api/roles/{id}`

Delete a role.

**Returns** `204` · `404` if not found

---

### `GET /api/roles/user/{userId}`

Get all roles assigned to a user.

**Returns** `200` — `UserRoleResponse[]`
```json
[{ "userId": "guid", "roleId": "guid", "roleName": "string", "assignedAt": "datetime" }]
```

---

### `POST /api/roles/user/{userId}`

Assign a role to a user.

**Body**
```json
{ "roleId": "guid" }
```

**Returns** `201` — `UserRoleResponse` · `404` if user/role not found · `409` if already assigned

---

### `DELETE /api/roles/user/{userId}/{roleId}`

Remove a role from a user.

**Returns** `204` · `404` if not found

---

## Profile — `/api/profile` *(Requires JWT)*

### `GET /api/profile`

Get the current user's profile.

**Returns** `200` — `ProfileResponse`
```json
{
  "userId": "guid",
  "displayName": "string?",
  "firstName": "string?",
  "lastName": "string?",
  "avatarUrl": "string?",
  "bio": "string?",
  "phoneNumber": "string?",
  "timezone": "string?",
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

---

### `GET /api/profile/{userId}` *(Admin only)*

Get a specific user's profile by user ID.

**Returns** `200` — `ProfileResponse` (same shape as above)

---

### `PUT /api/profile`

Create or update the current user's profile (upsert). All fields optional.

**Body**
```json
{
  "displayName": "string?",
  "firstName": "string?",
  "lastName": "string?",
  "avatarUrl": "string?",
  "bio": "string?",
  "phoneNumber": "string?",
  "timezone": "string?"
}
```

**Returns** `200` — `ProfileResponse`

---

## Notifications — `/api/notifications` *(Requires JWT)*

### `GET /api/notifications`

Get all notifications for the current user.

**Returns** `200` — `NotificationResponse[]`
```json
[{
  "id": "guid",
  "userId": "guid",
  "message": "string",
  "type": "string?",
  "isRead": false,
  "createdAt": "datetime"
}]
```

---

### `POST /api/notifications` *(Admin only)*

Create a notification for a specific user.

**Body**
```json
{ "userId": "guid", "message": "string", "type": "string?" }
```

**Returns** `201` — `NotificationResponse`
```json
{
  "id": "guid",
  "userId": "guid",
  "message": "string",
  "type": "string?",
  "isRead": false,
  "createdAt": "datetime"
}
```

---

### `PATCH /api/notifications/{id}/read`

Mark a single notification as read.

**Returns** `204`

---

### `PATCH /api/notifications/read-all`

Mark all notifications as read for the current user.

**Returns** `204`

---

### `DELETE /api/notifications/{id}`

Delete a notification.

**Returns** `204` · `404` if not found

---

## Activity Logs — `/api/logs` *(Admin only)*

### `GET /api/logs`

Retrieve activity logs with optional user filter.

**Query params**: `userId` (Guid?), `pageNumber` (default 1), `pageSize` (default 20)

**Returns** `200` — `PaginatedResult<LogResponse>`
```json
{
  "items": [{
    "id": "guid",
    "userId": "guid?",
    "action": "string",
    "details": "string?",
    "ipAddress": "string?",
    "createdAt": "datetime"
  }],
  "totalCount": 50,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

---

## Real-Time Notifications (SignalR)

**Hub URL**: `/hubs/notifications`

Pass the JWT via query string:
```
/hubs/notifications?access_token={jwtToken}
```

Connected clients are joined to a group keyed by their `userId`. Notifications created via the API or triggered by Kafka (`files.uploaded`) are broadcast to that group in real time.

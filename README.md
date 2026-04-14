# EncryptApp

End-to-end encrypted messaging web application. Users register, add friends, and exchange messages encrypted entirely client-side — the server never sees plaintext.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Setup & Installation](#setup--installation)
- [Running the Application](#running-the-application)
- [Database Schema](#database-schema)
- [API Reference](#api-reference)
- [SignalR Hub](#signalr-hub)
- [Encryption Flow](#encryption-flow)
- [Frontend Modules](#frontend-modules)
- [Dependencies](#dependencies)
- [Known Issues](#known-issues)

---

## Features

- **End-to-end encryption** — RSA-OAEP 2048-bit + AES-GCM 256-bit hybrid encryption
- **Real-time messaging** — SignalR WebSocket-based delivery
- **Zero-knowledge server** — encrypted payloads stored as `{wrappedKey, iv, ciphertext}`; server cannot decrypt
- **Friend system** — send/accept/reject friend requests, online status tracking
- **User search** — find and connect with other users
- **Typing indicators** — real-time "user is typing..." feedback
- **Read receipts** — delivered and read status on messages
- **Private key storage** — non-extractable CryptoKey objects stored in IndexedDB
- **Session-based auth** — server-side session with BCrypt password hashing

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Runtime** | .NET Framework 4.8, ASP.NET Web API 5 |
| **Real-time** | SignalR 2.4.3 (classic) |
| **ORM** | Entity Framework 6.5.1 (Code-First) |
| **Database** | SQL Server |
| **Password hashing** | BCrypt (BCrypt.Net-Next 4.0.3) |
| **Client crypto** | Web Crypto API (RSA-OAEP + AES-GCM) |
| **Key storage** | IndexedDB (non-extractable CryptoKey objects) |
| **Frontend** | Static HTML, vanilla JS (IIFE pattern), jQuery 3.7.1 |
| **JSON** | Newtonsoft.Json 13.0.3 with camelCase serialization |

---

## 3rd Party libraries

| BCrypt.Net-Next | 4.0.3	| Password hashing - bcrypt (registration, login)| 
| Entity Framework | 6.5.1	| ORM — manipulation of C# classes to SQL tables, generates queries, manages migrations| 
| Newtonsoft.Json	| 13.0.3 | JSON serialization/deserialization (API requests and responses)| 
| jQuery | 3.7.1	| DOM manipulation, the basis for the SignalR JS client| 

---
## Architecture

```
┌──────────────┐     AJAX/Fetch      ┌──────────────────┐
│   Browser    │ ──────────────────► │  ASP.NET Web API  │
│              │                     │   Controllers     │
│  HTML Pages  │     SignalR WS      │                   │
│  Vanilla JS  │ ◄─────────────────► │   ChatHub         │
│  Web Crypto  │                     │                   │
│  IndexedDB   │                     │   Services        │
└──────────────┘                     │   Repositories    │
                                     │   EF6 DbContext   │
                                     └────────┬─────────┘
                                              │
                                     ┌────────▼─────────┐
                                     │   SQL Server      │
                                     │   (encryptapp)    │
                                     └──────────────────┘
```

**Backend layers:**
- **Controllers** — route HTTP requests, validate input, return `ApiResponse<T>`
- **Services** — business logic (auth, friends, messaging, keys)
- **Repositories** — data access wrapping Entity Framework 6
- **No DI container** — services instantiate their own repositories and DbContext
```
  Controller
      │
   Service ──→ Repository ──→ DbContext ──→ SQL Server
```

**Frontend layers:**
- **Static HTML pages** — no server-side rendering (Razor)
- **IIFE module pattern** — `var Module = (function() { ... })();`
- **Shared utilities** — `api.js` (fetch wrapper with 401 redirect), `utils.js` (escapeHtml, toast, session)

---

## Project Structure

```
WebApplication5/
├── WebApplication5.sln
├── Issues.md
├── README.md
└── WebApplication5/
    ├── WebApplication5.csproj
    ├── packages.config
    ├── Web.config
    ├── Global.asax / Global.asax.cs
    │
    ├── App_Start/
    │   ├── Startup.cs                  ← OWIN startup, MapSignalR()
    │   └── WebApiConfig.cs             ← Attribute routing, camelCase JSON
    │
    ├── Models/
    │   ├── EncryptAppDbContext.cs       ← EF6 DbContext with Fluent API
    │   ├── Entities/                   ← User, Friend, FriendRequest, Message, Connection
    │   └── DTOs/                       ← ApiResponse, RegisterDto, LoginDto, MessageDto, etc.
    │
    ├── Repositories/
    │   ├── Interfaces/                 ← IUserRepository, IFriendRepository, etc.
    │   └── *.cs                        ← Implementations wrapping EF6
    │
    ├── Services/
    │   ├── Interfaces/                 ← IAuthService, IUserService, etc.
    │   └── *.cs                        ← Business logic
    │
    ├── Controllers/
    │   ├── AuthController.cs           ← Register, Login, Logout
    │   ├── UsersController.cs          ← Search, Profile, Me
    │   ├── FriendsController.cs        ← Requests, Accept/Reject, List
    │   └── MessagesController.cs       ← Send, History
    │
    ├── Hubs/
    │   └── ChatHub.cs                  ← SignalR real-time messaging hub
    │
    ├── Filters/
    │   └── SessionAuthAttribute.cs     ← Session-based auth filter
    │
    ├── Migrations/
    │   └── Configuration.cs            ← EF automatic migrations
    │
    ├── Views/                          ← Static HTML pages
    │   ├── login.html                  ← Default start page
    │   ├── register.html
    │   ├── dashboard.html
    │   ├── chat.html
    │   ├── friends.html
    │   ├── search.html
    │   └── profile.html
    │
    ├── Scripts/
    │   ├── lib/                        ← jQuery 3.7.1, SignalR 2.4.3
    │   ├── shared/                     ← api.js, utils.js
    │   └── app/                        ← crypto.js, keystore.js, signalr-client.js,
    │                                     auth.js, chat.js, friends.js, search.js,
    │                                     dashboard.js, profile.js
    │
    ├── Styles/
    │   ├── main.css                    ← CSS variables, layout, nav, toast
    │   ├── auth.css                    ← Login/register page styles
    │   └── chat.css                    ← Chat interface split-pane layout
    │
    └── Properties/
        └── AssemblyInfo.cs
```

---

## Prerequisites

- **Visual Studio 2022** with the **ASP.NET and web development** workload
- **SQL Server** (local or remote instance)
- **.NET Framework 4.8** Developer Pack
- **IIS Express** (included with Visual Studio)

---

## Setup & Installation

### 1. Clone the repository

```bash
git clone https://github.com/gigashot/WebApplication5.git
cd WebApplication5
```

### 2. Configure the database connection

Edit `WebApplication5/Web.config` and update the connection string:

```xml
<connectionStrings>
  <add name="EncryptAppConnection"
       connectionString="Data Source=YOUR_SERVER;Initial Catalog=encryptapp;Integrated Security=True;TrustServerCertificate=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

Replace `YOUR_SERVER` with your SQL Server instance name.

### 3. Restore NuGet packages

Open the solution in Visual Studio — packages restore automatically on build. Or run manually:

```bash
nuget restore WebApplication5.sln
```

### 4. Build the project

Press `Ctrl+Shift+B` in Visual Studio, or:

```bash
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" WebApplication5.sln /p:Configuration=Debug
```

### 5. Create the database

The database is created automatically on first run via EF6 automatic migrations. Ensure your SQL Server instance is running and the connection string is correct.

---

## Running the Application

### Option A — Visual Studio (recommended)

1. Open `WebApplication5.sln` in Visual Studio 2022
2. Ensure the debug target is set to **IIS Express** in the toolbar dropdown
3. Press **F5** to start debugging
4. The browser opens `Views/login.html` automatically

### Option B — IIS Express command line

```bash
"C:\Program Files\IIS Express\iisexpress.exe" /path:"C:\path\to\WebApplication5\WebApplication5" /port:5088
```

Then navigate to `http://localhost:5088/Views/login.html`.

---

## Database Schema

Entity Framework 6 Code-First creates these tables automatically:

### Users
| Column | Type | Notes |
|--------|------|-------|
| UserId | int | PK, identity |
| Username | nvarchar(50) | Unique index |
| PasswordHash | nvarchar(max) | BCrypt hash |
| PublicKey | nvarchar(max) | RSA-OAEP SPKI base64 |
| CreatedAt | datetime | |
| LastLogin | datetime | Nullable |

### Friends
| Column | Type | Notes |
|--------|------|-------|
| FriendId | int | PK, identity |
| UserId1 | int | FK → Users |
| UserId2 | int | FK → Users |
| CreatedAt | datetime | |

Unique composite index on (UserId1, UserId2). No cascade delete.

### FriendRequests
| Column | Type | Notes |
|--------|------|-------|
| RequestId | int | PK, identity |
| SenderId | int | FK → Users |
| ReceiverId | int | FK → Users |
| Status | nvarchar(max) | pending / accepted / rejected |
| CreatedAt | datetime | |

### Messages
| Column | Type | Notes |
|--------|------|-------|
| MessageId | bigint | PK, identity |
| SenderId | int | FK → Users |
| ReceiverId | int | FK → Users |
| EncryptedContent | nvarchar(max) | JSON: `{wrappedKey, iv, ciphertext}` |
| SentAt | datetime | |
| Delivered | bit | |
| Read | bit | |

Index on (SenderId, ReceiverId, SentAt).

### Connections
| Column | Type | Notes |
|--------|------|-------|
| ConnectionId | nvarchar(128) | PK (SignalR connection ID) |
| UserId | int | FK → Users |
| ConnectedAt | datetime | |

---

## API Reference

All responses follow the format: `{ "success": bool, "message": string, "data": T }`

### Authentication

| Method | Route | Auth | Body | Description |
|--------|-------|------|------|-------------|
| POST | `/api/auth/register` | No | `{username, password, publicKey}` | Create account |
| POST | `/api/auth/login` | No | `{username, password}` | Login, sets session cookie |
| POST | `/api/auth/logout` | No | — | Clear session |

### Users

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/users/search?q=` | Yes | Search users by username |
| GET | `/api/users/{id}` | Yes | Get user profile |
| GET | `/api/users/me` | Yes | Get current authenticated user |

### Friends

| Method | Route | Auth | Body | Description |
|--------|-------|------|------|-------------|
| POST | `/api/friends/request` | Yes | `{receiverId}` | Send friend request |
| GET | `/api/friends/requests` | Yes | — | Get pending received requests |
| POST | `/api/friends/requests/{id}/accept` | Yes | — | Accept friend request |
| POST | `/api/friends/requests/{id}/reject` | Yes | — | Reject friend request |
| GET | `/api/friends` | Yes | — | List all friends with online status |

### Messages

| Method | Route | Auth | Body | Description |
|--------|-------|------|------|-------------|
| GET | `/api/messages/{friendId}?page=&pageSize=` | Yes | — | Get conversation history |
| POST | `/api/messages` | Yes | `{receiverId, encryptedContent}` | Save encrypted message |

---

## SignalR Hub

The `ChatHub` handles real-time messaging over WebSockets.

### Server Methods (client → server)

| Method | Parameters | Description |
|--------|-----------|-------------|
| `Register` | `userId` (int) | Register connection for user |
| `SendMessage` | `toUserId` (int), `encryptedPayload` (string) | Send encrypted message |
| `SendTypingIndicator` | `toUserId` (int) | Notify recipient of typing |
| `MarkMessageDelivered` | `messageId` (long) | Mark message as delivered |
| `MarkMessageRead` | `messageId` (long) | Mark message as read |

### Client Callbacks (server → client)

| Callback | Data | Description |
|----------|------|-------------|
| `receiveMessage` | MessageDto | New message arrived |
| `messageSent` | MessageDto | Confirm message stored |
| `messageDelivered` | messageId | Delivery confirmation |
| `messageRead` | messageId | Read confirmation |
| `userOnline` | userId | User connected |
| `userOffline` | userId | User disconnected |
| `typingIndicator` | userId | User is typing |
| `refreshRequests` | — | Refresh friend request list |
| `error` | message | Error notification |

### Connection tracking

- **In-memory**: `ConcurrentDictionary<int, string>` maps userId → connectionId
- **Database**: `Connections` table persists connection state
- **Lifecycle**: `OnConnected`, `OnDisconnected`, `OnReconnected` maintain both stores

---

## Encryption Flow

EncryptApp uses **hybrid encryption** — RSA-OAEP wraps an ephemeral AES-GCM key per message.

### Key Generation (Registration)

```
Browser generates RSA-OAEP 2048-bit key pair
  ├── Public key → exported as SPKI base64 → sent to server → stored in Users table
  └── Private key → stored in IndexedDB as non-extractable CryptoKey object
```

### Sending a Message

```
1. Generate ephemeral AES-GCM 256-bit key
2. Generate random 12-byte IV
3. Encrypt plaintext with AES-GCM → ciphertext
4. Wrap AES key with recipient's RSA-OAEP public key → wrappedKey
5. Send JSON payload: { wrappedKey, iv, ciphertext } (all base64-encoded)
```

### Receiving a Message

```
1. Parse { wrappedKey, iv, ciphertext } from payload
2. Unwrap AES key using private key from IndexedDB
3. Decrypt ciphertext with AES-GCM using unwrapped key + IV
4. Decode bytes to plaintext string
```

### Security Notes

- RSA-OAEP 2048-bit can only encrypt ~190 bytes directly — hence the hybrid approach
- Private keys are **non-extractable** — they cannot be read from IndexedDB, only used for crypto operations
- Private keys are **per-browser** — no key recovery or cross-device sync mechanism
- The server stores only encrypted payloads — it cannot decrypt messages

---

## Frontend Modules

All JS modules use the IIFE (Immediately Invoked Function Expression) pattern.

| Module | File | Responsibility |
|--------|------|---------------|
| `Crypto` | `Scripts/app/crypto.js` | Key generation, hybrid encrypt/decrypt |
| `KeyStore` | `Scripts/app/keystore.js` | IndexedDB private key CRUD |
| `SignalRClient` | `Scripts/app/signalr-client.js` | Hub connection, auto-reconnect |
| `Auth` | `Scripts/app/auth.js` | Registration, login, logout flows |
| `Chat` | `Scripts/app/chat.js` | Message UI, encrypt/decrypt, real-time |
| `Friends` | `Scripts/app/friends.js` | Friend requests, accept/reject |
| `Search` | `Scripts/app/search.js` | User search interface |
| `Dashboard` | `Scripts/app/dashboard.js` | Dashboard initialization |
| `Profile` | `Scripts/app/profile.js` | User profile page |
| `API` | `Scripts/shared/api.js` | Fetch wrapper, 401 → login redirect |
| `Utils` | `Scripts/shared/utils.js` | escapeHtml, formatDate, toast, session |

### CSS Theme

The app uses CSS custom properties for theming defined in `Styles/main.css`:

```css
--primary: #4f46e5        /* Indigo */
--primary-hover: #4338ca
--success: #22c55e        /* Green */
--danger: #ef4444         /* Red */
--warning: #f59e0b        /* Amber */
--radius: 8px
```

---

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| BCrypt.Net-Next | 4.0.3 | Password hashing |
| EntityFramework | 6.5.1 | ORM (Code-First with migrations) |
| jQuery | 3.7.1 | DOM manipulation |
| Microsoft.AspNet.SignalR | 2.4.3 | Real-time messaging framework |
| Microsoft.AspNet.SignalR.Core | 2.4.3 | SignalR core library |
| Microsoft.AspNet.SignalR.JS | 2.4.3 | SignalR JavaScript client |
| Microsoft.AspNet.SignalR.SystemWeb | 2.4.3 | SystemWeb integration |
| Microsoft.AspNet.WebApi | 5.3.0 | Web API framework |
| Microsoft.AspNet.WebApi.Client | 6.0.0 | HTTP formatting & media types |
| Microsoft.AspNet.WebApi.Core | 5.3.0 | Web API core |
| Microsoft.AspNet.WebApi.WebHost | 5.3.0 | Web hosting integration |
| Microsoft.CodeDom.Providers.DotNetCompilerPlatform | 2.0.1 | Roslyn compiler |
| Microsoft.Owin | 4.2.2 | OWIN middleware |
| Microsoft.Owin.Host.SystemWeb | 4.2.2 | OWIN SystemWeb host |
| Microsoft.Owin.Security | 4.2.2 | OWIN security |
| Newtonsoft.Json | 13.0.3 | JSON serialization |
| Owin | 1.0 | OWIN specification |
| System.Runtime.CompilerServices.Unsafe | 4.5.3 | Runtime utilities (BCrypt dependency) |

---

## Known Issues

See [Issues.md](Issues.md) for the full list of tracked issues.

**Open:**
- Messages not sent/received/stored via SignalR — connection established but messages don't arrive
- Private key loss in IndexedDB — clearing browser data breaks encryption with no recovery path

**Resolved:**
- Missing `System.Runtime.CompilerServices.Unsafe` assembly
- NullReferenceException on login
- Visual Studio not recognizing project as Web Application
- Git worktree path issues

---

## License

This project is for educational and demonstration purposes.

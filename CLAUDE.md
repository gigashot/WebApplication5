# EncryptApp - Project Memory

## What Is This Project?
End-to-end encrypted messaging web application. Users register, add friends, and exchange messages encrypted client-side so the server never sees plaintext.

## Tech Stack
- **Runtime**: .NET Framework 4.8, ASP.NET Web API 5 (NOT .NET Core)
- **Real-time**: SignalR 2.4.3 (classic, not Core)
- **ORM**: Entity Framework 6 Code-First with migrations
- **Database**: SQL Server — `Data Source=MARA;Initial Catalog=encryptapp;Integrated Security=True`
- **Password hashing**: BCrypt (BCrypt.Net-Next 4.0.3)
- **Client crypto**: Web Crypto API — RSA-OAEP 2048-bit + AES-GCM 256-bit hybrid encryption
- **Key storage**: IndexedDB for private keys (non-extractable CryptoKey objects)
- **Frontend**: Static HTML pages, vanilla JS (IIFE module pattern), jQuery, jQuery.SignalR
- **No DI container** — services instantiate their own repositories/DbContext

## Project Structure
```
WebApplication5/                      ← solution root
├── WebApplication5.sln
└── WebApplication5/                  ← project root
    ├── WebApplication5.csproj        ← old-style .NET Framework csproj
    ├── packages.config
    ├── Web.config                    ← connection string, session, OWIN
    ├── Global.asax / Global.asax.cs  ← WebApiConfig registration
    ├── App_Start/
    │   ├── Startup.cs                ← OWIN startup, MapSignalR()
    │   └── WebApiConfig.cs           ← attribute routing, camelCase JSON
    ├── Models/
    │   ├── Entities/                 ← User, Friend, FriendRequest, Message, Connection
    │   ├── DTOs/                     ← RegisterDto, LoginDto, MessageDto, ApiResponse, etc.
    │   └── EncryptAppDbContext.cs     ← EF6 DbContext with Fluent API config
    ├── Repositories/
    │   ├── Interfaces/               ← IUserRepository, IFriendRepository, IMessageRepository, IConnectionRepository
    │   └── *.cs                      ← implementations wrapping EF6
    ├── Services/
    │   ├── Interfaces/               ← IAuthService, IUserService, IFriendService, IMessageService, IKeyService
    │   └── *.cs                      ← business logic
    ├── Controllers/                  ← AuthController, UsersController, FriendsController, MessagesController
    ├── Hubs/ChatHub.cs               ← SignalR hub (ConcurrentDictionary + DB for connections)
    ├── Filters/SessionAuthAttribute.cs ← checks HttpContext.Session["UserId"]
    ├── Views/                        ← static HTML pages (login, register, dashboard, chat, friends, search, profile)
    ├── Scripts/
    │   ├── lib/                      ← jQuery 3.7.1 + jQuery.SignalR 2.4.3 (NOT YET DOWNLOADED)
    │   ├── shared/                   ← api.js (fetch wrapper), utils.js (escapeHtml, formatDate, toast, session)
    │   └── app/                      ← crypto.js, keystore.js, signalr-client.js, auth.js, chat.js, friends.js, search.js, dashboard.js, profile.js
    └── Styles/                       ← main.css, auth.css, chat.css
```

## API Endpoints
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/api/auth/register` | No | Register with {username, password, publicKey} |
| POST | `/api/auth/login` | No | Login, sets session cookie |
| POST | `/api/auth/logout` | No | Clears session |
| GET | `/api/users/search?q=` | Yes | Search users by username |
| GET | `/api/users/{id}` | Yes | Get user profile |
| GET | `/api/users/me` | Yes | Get current user |
| POST | `/api/friends/request` | Yes | Send friend request {receiverId} |
| GET | `/api/friends/requests` | Yes | Get pending received requests |
| POST | `/api/friends/requests/{id}/accept` | Yes | Accept friend request |
| POST | `/api/friends/requests/{id}/reject` | Yes | Reject friend request |
| GET | `/api/friends` | Yes | List all friends with online status |
| GET | `/api/messages/{friendId}?page=&pageSize=` | Yes | Get conversation history |
| POST | `/api/messages` | Yes | Save encrypted message |

## SignalR Hub Methods (ChatHub)
**Server methods** (JS → server): `Register(userId)`, `SendMessage(toUserId, encryptedPayload)`, `SendTypingIndicator(toUserId)`, `MarkMessageRead(messageId)`, `MarkMessageDelivered(messageId)`
**Client callbacks** (server → JS): `receiveMessage`, `messageSent`, `messageDelivered`, `messageRead`, `userOnline`, `userOffline`, `typingIndicator`, `refreshRequests`, `error`

## Encryption Flow
1. **Registration**: Browser generates RSA-OAEP 2048-bit key pair → public key (SPKI base64) sent to server → private key stored in IndexedDB
2. **Sending**: Generate ephemeral AES-GCM-256 key → encrypt plaintext → wrap AES key with recipient's RSA public key → send JSON `{wrappedKey, iv, ciphertext}` (all base64)
3. **Receiving**: Unwrap AES key with private key from IndexedDB → decrypt ciphertext → display plaintext
4. **Limitation**: Private keys are per-browser; no key recovery mechanism

## Database Schema (EF Code-First)
- **Users**: UserId (int PK), Username (unique, max 50), PasswordHash, PublicKey, CreatedAt, LastLogin
- **Friends**: FriendId (int PK), UserId1, UserId2, CreatedAt — unique composite index (UserId1, UserId2), no cascade delete
- **FriendRequests**: RequestId (int PK), SenderId, ReceiverId, Status (pending/accepted/rejected), CreatedAt
- **Messages**: MessageId (long PK), SenderId, ReceiverId, EncryptedContent, SentAt, Delivered, Read — index on (SenderId, ReceiverId, SentAt)
- **Connections**: ConnectionId (string PK), UserId, ConnectedAt

## Current Build Status — COMPLETE
### All files created and verified:
- ✅ Solution + project files (.sln, .csproj, packages.config, Web.config, Web.Debug.config, Web.Release.config)
- ✅ EF6 entities (User, Friend, FriendRequest, Message, Connection) + DbContext + Migrations/Configuration.cs
- ✅ All repository interfaces + implementations (User, Friend, Message, Connection)
- ✅ All service interfaces + implementations (Auth, User, Friend, Message, Key)
- ✅ All DTOs (RegisterDto, LoginDto, LoginResponseDto, MessageDto, FriendDto, etc.)
- ✅ All 4 controllers (Auth, Users, Friends, Messages) + SessionAuthAttribute
- ✅ ChatHub (SignalR hub with ConcurrentDictionary + DB connection tracking)
- ✅ All 7 HTML pages (login, register, dashboard, chat, friends, search, profile)
- ✅ All 9 JS modules (crypto, keystore, signalr-client, auth, chat, friends, search, dashboard, profile)
- ✅ Shared JS (api.js, utils.js) + jQuery 3.7.1 + jQuery.SignalR 2.4.3
- ✅ All 3 CSS files (main.css, auth.css, chat.css)
- ✅ NuGet packages restored (17 packages)
- ✅ **Project compiles with 0 errors** (1 warning: System.Memory binding redirect, non-blocking)
- ✅ **Database created** via EF automatic migration — all 5 tables + indexes + FKs in `encryptapp` on `MARA`

### Build Notes:
- **MSBuild path**: `C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\amd64\MSBuild.exe`
- **NuGet**: `nuget.exe` downloaded to solution root for package restore
- **EF migrations**: Uses `ef6.exe` from `packages\EntityFramework.6.5.1\tools\net45\any\ef6.exe` with config at `ef6.exe.config`
- **WebApi.Client**: Version 6.0.0 (5.3.0 no longer on NuGet); binding redirect 0.0.0.0-6.0.0.0 → 6.0.0.0 in Web.config
- **BCrypt.Net-Next**: PublicKeyToken=`1e11be04b6288443` (corrected from initial value)

## Key Design Decisions
1. Static HTML pages (not Razor) — all data via AJAX to Web API
2. Session-based auth via `HttpContext.Session` — SignalR query string userId
3. Hybrid encryption (RSA wraps AES) — RSA-OAEP can only encrypt ~190 bytes
4. ConcurrentDictionary + DB for SignalR connection tracking
5. JS uses IIFE module pattern (no ES modules, no bundler)
6. User session info cached in `sessionStorage` via `Utils.setCurrentUser()`
7. All FK relationships use `WillCascadeOnDelete(false)` to avoid multiple cascade paths

## Important Patterns
- Controllers instantiate services directly: `new AuthService()`
- Services instantiate repos + DbContext in constructor
- All API responses use `ApiResponse` / `ApiResponse<T>` wrapper: `{success, message, data}`
- Frontend checks for 401 in api.js and redirects to login
- Chat.js decrypts messages on the client after fetching encrypted content from server

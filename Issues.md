# EncryptApp - Known Issues & Fixes

## Resolved

### 1. Missing `System.Runtime.CompilerServices.Unsafe` assembly
- **Symptom**: 500 error on `POST /api/auth/register` — `FileNotFoundException` for `System.Runtime.CompilerServices.Unsafe, Version=4.0.4.1`
- **Cause**: BCrypt.Net-Next 4.0.3 depends on `System.Runtime.CompilerServices.Unsafe` which wasn't included as a NuGet package
- **Fix**: Install `System.Runtime.CompilerServices.Unsafe` NuGet package and add binding redirect in Web.config

### 2. NullReferenceException on login (AuthController.cs line 40)
- **Symptom**: `System.NullReferenceException: Object reference not set to an instance of an object` at `session["UserId"] = result.Data.UserId`
- **Cause**: `result.Data` is null when login returns a failure result, or session object is null
- **Status**: Resolved — confirmed login works after fix

### 3. Visual Studio project not recognized as Web Application
- **Symptom**: VS 2022 tried to run `WebApplication5.dll` directly instead of launching IIS Express — "not a valid Win32 application"
- **Cause**: Missing `ProjectTypeGuids` in `.csproj` — VS treated project as a class library
- **Fix**: Unloaded project in VS, edited `.csproj` to add `ProjectTypeGuids` with Web Application and C# GUIDs, reloaded. VS now shows "IIS Express" in debug dropdown

### 4. Git worktree path issues
- **Symptom**: `.sln` file failed to load project in VS when opened from worktree path
- **Fix**: Checked out `claude/crazy-brattain` branch directly in main repo at `C:\Users\eiche\source\repos\WebApplication5\`

## Open

### 5. Messages not sent/received/stored
- **Symptom**: Sending a message in chat does not deliver to the other user, and nothing is saved in `dbo.Messages`
- **Observed**: SignalR connection IS established (WebSocket transport, userId passed). The `sendMessage` call returns a jQuery Deferred but messages don't arrive.
- **Likely cause**: Needs investigation — possible issues:
  - `GetCallerUserId()` in ChatHub may not find the caller's connection mapping
  - `AreFriends()` check may be failing silently
  - The hub `SendMessage` method may be erroring server-side without surfacing to the client
- **Debug approach**: Set breakpoint on `ChatHub.SendMessage()` and trace the flow

### 6. Private key not found in IndexedDB
- **Symptom**: "No encryption key found. Messages cannot be decrypted." shown on chat page
- **Cause**: During registration, the private key is stored in IndexedDB keyed by `username`. On login, the code tries to migrate it to `userId`. If migration fails or IndexedDB was cleared (e.g. browser data wipe, different browser profile), the key is lost.
- **Impact**: Without the private key, messages cannot be encrypted or decrypted — chat is non-functional
- **Possible fixes**:
  - Re-register the user (generates new key pair, but loses old message history)
  - Add a key re-generation flow that also updates the public key on the server
  - Verify the IndexedDB migration path in `auth.js` (lines 114-130) works correctly

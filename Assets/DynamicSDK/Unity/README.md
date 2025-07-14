# Unity Message Types - Modular Structure

## 📁 Directory Structure

```
Unity/
├── Core/
│   ├── IUnityMessage.cs         # Base interface and BaseMessage class
│   └── CommonDataTypes.cs       # UserInfo, WalletCredential, ErrorInfo
├── Auth/
│   ├── AuthConstants.cs         # Auth action constants
│   └── AuthMessages.cs          # All Auth message types
├── Wallet/
│   ├── WalletConstants.cs       # Wallet action constants
│   ├── WalletRequestMessages.cs # Request message types
│   └── WalletResponseMessages.cs# Response message types
├── Utils/
│   ├── MessageFactory.cs        # Factory for creating messages
│   ├── MessageValidator.cs      # Validation utilities
│   ├── ErrorCodes.cs           # Error code constants
│   └── MessageExtensions.cs     # Extension methods
└── README.md                   # This file
```

## 🎯 Purpose of Modularization

### **Easy Management**
- Each file has its own distinct responsibility
- Easy to search and modify
- Reduces conflicts when multiple developers work

### **Logical Organization**
- **Core**: Basic classes and interfaces
- **Auth**: Everything related to authentication
- **Wallet**: Everything related to wallet operations
- **Utils**: Utilities and helper functions

### **Easy Extension**
- Adding new message types only requires editing the corresponding file
- Adding new modules (e.g., NFT/) without affecting existing code

## 📝 Usage

### **Import by module:**

```csharp
// Only import what you need
using DynamicSDK.Unity.Messages;           // Core types
using DynamicSDK.Unity.Messages.Auth;      // Auth messages
using DynamicSDK.Unity.Messages.Wallet;    // Wallet messages
```

### **Using Factory:**

```csharp
// Create auth request
var authRequest = MessageFactory.CreateAuthRequest("my-game", new[] { "ethereum" });

// Create sign message request
var signRequest = MessageFactory.CreateSignMessageRequest(
    "0x742d35Cc6665C4687BAb34b1A7AB6781e2B2896C", 
    "Hello World!", 
    "ethereum"
);
```

### **Validation:**

```csharp
// Validate message
if (MessageValidator.IsValidMessage(message) && MessageValidator.IsWalletMessage(message))
{
    // Process wallet message
}

// Validate wallet address
if (MessageValidator.IsValidWalletAddress(address))
{
    // Use address
}
```

### **Extension methods:**

```csharp
// Check success
if (balanceResponse.IsSuccessful())
{
    float balance = balanceResponse.GetBalanceAsFloat();
}

// Check expiry
if (message.IsExpired(300)) // 5 minutes
{
    // Handle expired message
}
```

## 🔄 Migration from old file

If you are using the old `UnityMessageTypes.cs`:

1. **Replace imports:**
```csharp
// Old
using DynamicSDK.Unity.Messages;

// New - specific imports
using DynamicSDK.Unity.Messages;
using DynamicSDK.Unity.Messages.Auth;
using DynamicSDK.Unity.Messages.Wallet;
```

2. **Update namespace references:**
```csharp
// Old
Auth.AuthSuccessMessage

// New
AuthSuccessMessage
```

3. **Use Factory methods:**
```csharp
// Old
var message = new Auth.AuthRequestMessage { data = new Auth.AuthRequestData { ... } };

// New
var message = MessageFactory.CreateAuthRequest("gameId", chains);
```

## ✅ Benefits

- **Maintainability**: Easy to maintain and debug
- **Scalability**: Easy to add new features
- **Team collaboration**: Reduces merge conflicts
- **Performance**: Only compiles necessary classes
- **Organization**: Code is logically organized and clean 
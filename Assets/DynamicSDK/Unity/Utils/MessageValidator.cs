using System;

namespace DynamicSDK.Unity.Messages
{
    /// <summary>
    /// Message validation utilities
    /// </summary>
    public static class MessageValidator
    {
        public static bool IsValidMessage(IUnityMessage message)
        {
            return !string.IsNullOrEmpty(message?.type) &&
                   !string.IsNullOrEmpty(message?.action) &&
                   !string.IsNullOrEmpty(message?.requestId) &&
                   message.timestamp > 0;
        }

        public static bool IsAuthMessage(IUnityMessage message) { return message?.type == "auth"; }

        public static bool IsWalletMessage(IUnityMessage message) { return message?.type == "wallet"; }

        public static bool IsValidWalletAddress(string address)
        {
            return !string.IsNullOrEmpty(address) &&
                   address.StartsWith("0x") &&
                   address.Length == 66; // Sui addresses are 32 bytes (64 chars) + "0x" prefix
        }

        public static bool IsValidChain(string chain) { return chain?.Equals("sui", StringComparison.OrdinalIgnoreCase) ?? false; }
    }
}
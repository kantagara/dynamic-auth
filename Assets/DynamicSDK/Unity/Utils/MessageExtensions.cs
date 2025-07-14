using System;
using UnityEngine;
using DynamicSDK.Unity.Messages.Wallet;

namespace DynamicSDK.Unity.Messages
{
    /// <summary>
    /// Extension methods for message handling
    /// </summary>
    public static class MessageExtensions
    {
        // ============================================================================
        // SUCCESS VALIDATION EXTENSIONS
        // ============================================================================
        
        public static bool IsSuccessful(this BalanceResponseMessage message)
        {
            return message?.data?.success == true && string.IsNullOrEmpty(message.data.error);
        }

        public static bool IsSuccessful(this SignMessageResponseMessage message)
        {
            return message?.data?.success == true && 
                   !string.IsNullOrEmpty(message.data.signature) &&
                   string.IsNullOrEmpty(message.data.error);
        }

        public static bool IsSuccessful(this TransactionResponseMessage message)
        {
            return message?.data?.success == true &&
                   !string.IsNullOrEmpty(message.data.transactionHash) &&
                   string.IsNullOrEmpty(message.data.error);
        }

        // ============================================================================
        // UTILITY EXTENSIONS
        // ============================================================================
        
        public static float GetBalanceAsFloat(this BalanceResponseMessage message)
        {
            if (!message.IsSuccessful() || string.IsNullOrEmpty(message.data.balance))
                return 0f;

            if (float.TryParse(message.data.balance, out float balance))
            {
                // Convert from wei to ether (assuming 18 decimals)
                return balance / Mathf.Pow(10, message.data.decimals);
            }

            return 0f;
        }

        public static bool IsExpired(this IUnityMessage message, int timeoutSeconds = 300)
        {
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return (currentTime - message.timestamp) > (timeoutSeconds * 1000);
        }

        // ============================================================================
        // PARSING EXTENSIONS
        // ============================================================================
        
        public static T SafeCast<T>(this IUnityMessage message) where T : class, IUnityMessage
        {
            return message as T;
        }

        public static bool TryGetData<T>(this IUnityMessage message, out T data) where T : class
        {
            data = null;
            
            if (message == null)
                return false;

            var property = message.GetType().GetProperty("data");
            if (property != null && property.PropertyType == typeof(T))
            {
                data = property.GetValue(message) as T;
                return data != null;
            }

            return false;
        }
    }
} 
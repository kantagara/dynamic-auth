using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Web;
using DynamicSDK.Unity.Messages;
using DynamicSDK.Unity.Messages.Auth;
using DynamicSDK.Unity.Messages.Wallet;

namespace DynamicSDK.Unity.Parser
{
    /// <summary>
    /// Unity message parser for UniWebView messages
    /// Handles parsing messages from Web to Unity via UniWebView URL schemes
    /// </summary>
    public static class UnityMessageParser
    {
        // ============================================================================
        // URL SCHEME CONSTANTS
        // ============================================================================

        private const string AUTH_SCHEME = "uniwebview://auth";
        private const string WALLET_SCHEME = "uniwebview://wallet";
        private const string MESSAGE_PARAM = "message";

        // ============================================================================
        // MAIN PARSING METHODS
        // ============================================================================

        /// <summary>
        /// Parse UniWebView message from URL scheme
        /// Supports both auth and wallet schemes
        /// </summary>
        /// <param name="url">The URL from UniWebView (e.g., "uniwebview://auth?message=...")</param>
        /// <returns>Parsed message or null if parsing failed</returns>
        public static IUnityMessage ParseUniWebViewMessage(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    Debug.LogError("[UnityMessageParser] URL is null or empty");

                    return null;
                }

                Debug.Log($"[UnityMessageParser] Parsing URL: {url}");

                // Extract message parameter from URL
                string messageJson = ExtractMessageFromUrl(url);

                if (string.IsNullOrEmpty(messageJson))
                {
                    Debug.LogError("[UnityMessageParser] Failed to extract message from URL");

                    return null;
                }

                Debug.Log($"[UnityMessageParser] Extracted JSON: {messageJson}");

                // Parse the JSON message
                return ParseJsonMessage(messageJson);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityMessageParser] Error parsing UniWebView message: {ex.Message}\nStackTrace: {ex.StackTrace}");

                return null;
            }
        }

        /// <summary>
        /// Parse JSON message string directly
        /// </summary>
        /// <param name="jsonMessage">JSON string message</param>
        /// <returns>Parsed message or null if parsing failed</returns>
        public static IUnityMessage ParseJsonMessage(string jsonMessage)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonMessage))
                {
                    Debug.LogError("[UnityMessageParser] JSON message is null or empty");

                    return null;
                }

                // First, parse as a generic message to determine type and action
                var baseMessage = JsonUtility.FromJson<BaseMessageData>(jsonMessage);

                if (baseMessage == null)
                {
                    Debug.LogError("[UnityMessageParser] Failed to parse base message structure");

                    return null;
                }

                Debug.Log($"[UnityMessageParser] Message type: {baseMessage.type}, action: {baseMessage.action}");

                // Route to appropriate parser based on type
                switch (baseMessage.type?.ToLower())
                {
                    case "auth":
                        return ParseAuthMessage(jsonMessage, baseMessage.action);

                    case "wallet":
                        var wallet = ParseWalletMessage(jsonMessage, baseMessage.action);
                        if (wallet == null)
                        {
                            Debug.LogError($"[UnityMessageParser] Failed to parse wallet message for action: {baseMessage.action}");
                        }
                        else
                        {
                            Debug.Log($"[UnityMessageParser] Successfully parsed wallet message: {wallet.GetType().Name}");
                        }
                        return wallet;
                    default:
                        Debug.LogError($"[UnityMessageParser] Unknown message type: {baseMessage.type}");

                        return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityMessageParser] Error parsing JSON message: {ex.Message}\nJSON: {jsonMessage}");

                return null;
            }
        }

        // ============================================================================
        // AUTH MESSAGE PARSING
        // ============================================================================

        private static IUnityMessage ParseAuthMessage(string jsonMessage, string action)
        {
            try
            {
                switch (action)
                {
                    case AuthActions.AUTH_SUCCESS:
                        return JsonUtility.FromJson<AuthSuccessMessage>(jsonMessage);

                    case AuthActions.AUTH_FAILED:
                        return JsonUtility.FromJson<AuthFailedMessage>(jsonMessage);

                    case AuthActions.LOGGED_OUT:
                        return JsonUtility.FromJson<LoggedOutMessage>(jsonMessage);

                    case AuthActions.HANDLE_AUTHENTICATED_USER:
                        return JsonUtility.FromJson<HandleAuthenticatedUserMessage>(jsonMessage);

                    case AuthActions.JWT_TOKEN_RESPONSE:
                        return JsonUtility.FromJson<JwtTokenResponseMessage>(jsonMessage);

                    default:
                        Debug.LogError($"[UnityMessageParser] Unknown auth action: {action}");

                        return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityMessageParser] Error parsing auth message: {ex.Message}\nAction: {action}");

                return null;
            }
        }

        // ============================================================================
        // WALLET MESSAGE PARSING
        // ============================================================================

        private static IUnityMessage ParseWalletMessage(string jsonMessage, string action)
        {
            try
            {
                switch (action)
                {
                    case WalletActions.BALANCE_RESPONSE:
                        return JsonUtility.FromJson<BalanceResponseMessage>(jsonMessage);
                    case WalletActions.SWITCH_WALLET:
                        return JsonUtility.FromJson<BalanceResponseMessage>(jsonMessage);
                    case WalletActions.SWITCH_NETWORK:
                        return JsonUtility.FromJson<BalanceResponseMessage>(jsonMessage);
                    case WalletActions.SIGN_MESSAGE_RESPONSE:
                        return JsonUtility.FromJson<SignMessageResponseMessage>(jsonMessage);

                    case WalletActions.TRANSACTION_RESPONSE:
                        return JsonUtility.FromJson<TransactionResponseMessage>(jsonMessage);

                    case WalletActions.WALLET_CONNECTED:
                        var wallet2 = JsonUtility.FromJson<WalletConnectedMessage>(jsonMessage);
                        if (wallet2 == null)
                        {
                            Debug.LogError("[UnityMessageParser] Failed to parse WalletConnectedMessage");

                            return null;
                        }
                        else
                        {
                            Debug.Log($"[UnityMessageParser] Successfully parsed WalletConnectedMessage: {wallet2.data}");
                        }
                        return wallet2;

                    case WalletActions.WALLET_DISCONNECTED:
                        return JsonUtility.FromJson<WalletDisconnectedMessage>(jsonMessage);

                    case WalletActions.WALLET_ERROR:
                        return JsonUtility.FromJson<WalletErrorMessage>(jsonMessage);

                    case WalletActions.WALLETS_RESPONSE:
                        return JsonUtility.FromJson<WalletsResponseMessage>(jsonMessage);

                    case WalletActions.NETWORKS_RESPONSE:
                        return JsonUtility.FromJson<NetworksResponseMessage>(jsonMessage);

                    default:
                        Debug.LogError($"[UnityMessageParser] Unknown wallet action: {action}");

                        return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityMessageParser] Error parsing wallet message: {ex.Message}\nAction: {action}");

                return null;
            }
        }

        // ============================================================================
        // URL PARSING UTILITIES
        // ============================================================================

        /// <summary>
        /// Extract message parameter from UniWebView URL
        /// </summary>
        /// <param name="url">Full URL from UniWebView</param>
        /// <returns>Decoded JSON message string</returns>
        private static string ExtractMessageFromUrl(string url)
        {
            try
            {
                // Parse URL to get query parameters
                var uri = new Uri(url);
                var queryParameters = ParseQueryString(uri.Query);

                if (!queryParameters.ContainsKey(MESSAGE_PARAM))
                {
                    Debug.LogError($"[UnityMessageParser] Message parameter not found in URL: {url}");

                    return null;
                }

                string encodedMessage = queryParameters[MESSAGE_PARAM];

                // URL decode the message
                string decodedMessage = Uri.UnescapeDataString(encodedMessage);

                return decodedMessage;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityMessageParser] Error extracting message from URL: {ex.Message}\nURL: {url}");

                return null;
            }
        }

        /// <summary>
        /// Parse query string into dictionary
        /// </summary>
        /// <param name="queryString">Query string (e.g., "?message=...")</param>
        /// <returns>Dictionary of key-value pairs</returns>
        private static Dictionary<string, string> ParseQueryString(string queryString)
        {
            var result = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(queryString))
                return result;

            // Remove leading '?' if present
            if (queryString.StartsWith("?"))
                queryString = queryString.Substring(1);

            // Split by '&' to get key-value pairs
            string[] pairs = queryString.Split('&');

            foreach (string pair in pairs)
            {
                string[] keyValue = pair.Split('=');

                if (keyValue.Length == 2)
                {
                    string key = Uri.UnescapeDataString(keyValue[0]);
                    string value = Uri.UnescapeDataString(keyValue[1]);
                    result[key] = value;
                }
            }

            return result;
        }

        // ============================================================================
        // VALIDATION AND UTILITIES
        // ============================================================================

        /// <summary>
        /// Check if URL is a valid UniWebView scheme
        /// </summary>
        /// <param name="url">URL to validate</param>
        /// <returns>True if valid scheme</returns>
        public static bool IsValidUniWebViewUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            return url.StartsWith(AUTH_SCHEME) || url.StartsWith(WALLET_SCHEME);
        }

        /// <summary>
        /// Get message type from URL scheme
        /// </summary>
        /// <param name="url">UniWebView URL</param>
        /// <returns>"auth" or "wallet" or null</returns>
        public static string GetMessageTypeFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            if (url.StartsWith(AUTH_SCHEME))
                return "auth";

            if (url.StartsWith(WALLET_SCHEME))
                return "wallet";

            return null;
        }

        /// <summary>
        /// Validate parsed message
        /// </summary>
        /// <param name="message">Parsed message to validate</param>
        /// <returns>True if message is valid</returns>
        public static bool ValidateParsedMessage(IUnityMessage message)
        {
            if (message == null)
                return false;

            return MessageValidator.IsValidMessage(message);
        }

        // ============================================================================
        // HELPER CLASSES
        // ============================================================================

        /// <summary>
        /// Base message data for initial parsing
        /// </summary>
        [Serializable]
        private class BaseMessageData
        {
            public string type;
            public string action;
            public long timestamp;
            public string requestId;
        }
    }

    // ============================================================================
    // TYPED PARSING EXTENSIONS
    // ============================================================================

    /// <summary>
    /// Typed parsing extensions for specific message types
    /// </summary>
    public static class TypedMessageParser
    {
        /// <summary>
        /// Parse as Auth Success message
        /// </summary>
        public static AuthSuccessMessage ParseAsAuthSuccess(string url)
        {
            var message = UnityMessageParser.ParseUniWebViewMessage(url);

            return message as AuthSuccessMessage;
        }

        /// <summary>
        /// Parse as Auth Failed message
        /// </summary>
        public static AuthFailedMessage ParseAsAuthFailed(string url)
        {
            var message = UnityMessageParser.ParseUniWebViewMessage(url);

            return message as AuthFailedMessage;
        }

        /// <summary>
        /// Parse as Sign Message Response
        /// </summary>
        public static SignMessageResponseMessage ParseAsSignMessageResponse(string url)
        {
            var message = UnityMessageParser.ParseUniWebViewMessage(url);

            return message as SignMessageResponseMessage;
        }

        /// <summary>
        /// Parse as Balance Response
        /// </summary>
        public static BalanceResponseMessage ParseAsBalanceResponse(string url)
        {
            var message = UnityMessageParser.ParseUniWebViewMessage(url);

            return message as BalanceResponseMessage;
        }

        /// <summary>
        /// Parse as Transaction Response
        /// </summary>
        public static TransactionResponseMessage ParseAsTransactionResponse(string url)
        {
            var message = UnityMessageParser.ParseUniWebViewMessage(url);

            return message as TransactionResponseMessage;
        }

        /// <summary>
        /// Parse as Wallet Connected
        /// </summary>
        public static WalletConnectedMessage ParseAsWalletConnected(string url)
        {
            var message = UnityMessageParser.ParseUniWebViewMessage(url);

            return message as WalletConnectedMessage;
        }

        /// <summary>
        /// Parse as Wallet Error
        /// </summary>
        public static WalletErrorMessage ParseAsWalletError(string url)
        {
            var message = UnityMessageParser.ParseUniWebViewMessage(url);

            return message as WalletErrorMessage;
        }

        /// <summary>
        /// Parse as Wallets Response
        /// </summary>
        public static WalletsResponseMessage ParseAsWalletsResponse(string url)
        {
            var message = UnityMessageParser.ParseUniWebViewMessage(url);

            return message as WalletsResponseMessage;
        }

        /// <summary>
        /// Parse as Networks Response
        /// </summary>
        public static NetworksResponseMessage ParseAsNetworksResponse(string url)
        {
            var message = UnityMessageParser.ParseUniWebViewMessage(url);

            return message as NetworksResponseMessage;
        }
    }

    // ============================================================================
    // UNITY INTEGRATION HELPER
    // ============================================================================

    /// <summary>
    /// Unity MonoBehaviour helper for message parsing
    /// </summary>
    public class UnityMessageHandler : MonoBehaviour
    {
        [Header("Debug Settings")] public bool enableDebugLogs = true;
        public bool logRawMessages = false;

        /// <summary>
        /// Event fired when a message is successfully parsed
        /// </summary>
        public System.Action<IUnityMessage> OnMessageReceived;

        /// <summary>
        /// Event fired when message parsing fails
        /// </summary>
        public System.Action<string, string> OnParsingError; // url, error

        /// <summary>
        /// Handle UniWebView message
        /// Call this from UniWebView's OnMessageReceived callback
        /// </summary>
        /// <param name="webView">The WebView instance</param>
        /// <param name="message">The message from UniWebView</param>
        public void HandleUniWebViewMessage(object webView, string message)
        {
            try
            {
                if (enableDebugLogs)
                    Debug.Log($"[UnityMessageHandler] Received message: {message}");

                if (logRawMessages)
                    Debug.Log($"[UnityMessageHandler] Raw message: {message}");

                // Check if it's a valid UniWebView URL
                if (!UnityMessageParser.IsValidUniWebViewUrl(message))
                {
                    if (enableDebugLogs)
                        Debug.Log($"[UnityMessageHandler] Not a UniWebView scheme URL, ignoring: {message}");

                    return;
                }

                // Parse the message
                var parsedMessage = UnityMessageParser.ParseUniWebViewMessage(message);

                if (parsedMessage != null)
                {
                    if (enableDebugLogs)
                        Debug.Log($"[UnityMessageHandler] Successfully parsed message: {parsedMessage.type}/{parsedMessage.action}");

                    OnMessageReceived?.Invoke(parsedMessage);
                }
                else
                {
                    string error = "Failed to parse message";
                    Debug.LogError($"[UnityMessageHandler] {error}: {message}");
                    OnParsingError?.Invoke(message, error);
                }
            }
            catch (Exception ex)
            {
                string error = $"Exception in message handling: {ex.Message}";
                Debug.LogError($"[UnityMessageHandler] {error}\nStackTrace: {ex.StackTrace}");
                OnParsingError?.Invoke(message, error);
            }
        }

        /// <summary>
        /// Handle specific message types with type safety
        /// </summary>
        /// <typeparam name="T">Expected message type</typeparam>
        /// <param name="message">Parsed message</param>
        /// <param name="callback">Callback for the specific type</param>
        public void HandleTypedMessage<T>(IUnityMessage message, System.Action<T> callback) where T : class, IUnityMessage
        {
            if (message is T typedMessage)
            {
                callback?.Invoke(typedMessage);
            }
            else
            {
                Debug.LogWarning($"[UnityMessageHandler] Expected {typeof(T).Name} but got {message?.GetType().Name}");
            }
        }
    }
}
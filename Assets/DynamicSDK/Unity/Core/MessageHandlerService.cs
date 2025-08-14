using UnityEngine;
using DynamicSDK.Unity.Messages;
using DynamicSDK.Unity.Messages.Auth;
using DynamicSDK.Unity.Messages.Wallet;
using DynamicSDK.Unity.Parser;

namespace DynamicSDK.Unity.Core
{
    /// <summary>
    /// Service to handle and route Unity messages
    /// </summary>
    public class MessageHandlerService
    {
        // Events for different message types
        public System.Action<AuthSuccessMessage> OnAuthSuccess;
        public System.Action<AuthFailedMessage> OnAuthFailed;
        public System.Action<LoggedOutMessage> OnLoggedOut;
        public System.Action<HandleAuthenticatedUserMessage> OnAuthenticatedUser;
        public System.Action<JwtTokenResponseMessage> OnJwtTokenResponse;

        public System.Action<BalanceResponseMessage> OnBalanceResponse;
        public System.Action<BalanceResponseMessage> OnWalletSwitched;
        public System.Action<BalanceResponseMessage> OnNetworkSwitched;
        public System.Action<SignMessageResponseMessage> OnSignMessageResponse;
        public System.Action<TransactionResponseMessage> OnTransactionResponse;
        public System.Action<WalletConnectedMessage> OnWalletConnected;
        public System.Action<WalletDisconnectedMessage> OnWalletDisconnected;
        public System.Action<WalletErrorMessage> OnWalletError;
        public System.Action<WalletsResponseMessage> OnWalletsResponse;
        public System.Action<NetworksResponseMessage> OnNetworksResponse;

        private readonly DynamicSDKConfig config;

        public MessageHandlerService()
        {
            config = DynamicSDKConfig.Instance;
        }

        /// <summary>
        /// Process incoming UniWebView message
        /// </summary>
        public void ProcessMessage(UniWebViewMessage msg)
        {
            if (config.enableDebugLogs)
            {
                Debug.Log($"[MessageHandlerService] Received message: {msg.Path}");
            }

            // Handle legacy "connected" message for backward compatibility
            if (msg.Path.Equals("connected", System.StringComparison.OrdinalIgnoreCase))
            {
                HandleLegacyConnectedMessage(msg);
                return;
            }

            // Parse the message using the Unity message parser
            IUnityMessage parsedMessage = UnityMessageParser.ParseUniWebViewMessage(msg.RawMessage);
            if (config.enableDebugLogs)
            {
                Debug.Log($"[MessageHandlerService] Parsed message: {parsedMessage?.type} - {parsedMessage?.action}");
            }
            if (parsedMessage == null)
            {
                Debug.LogError($"[MessageHandlerService] Failed to parse message: {msg.RawMessage}");
                return;
            }

            RouteMessage(parsedMessage);
        }

        /// <summary>
        /// Route parsed message to appropriate handler
        /// </summary>
        private void RouteMessage(IUnityMessage message)
        {
            switch (message.type?.ToLower())
            {
                case "auth":
                    HandleAuthMessage(message);
                    break;

                case "wallet":
                    HandleWalletMessage(message);
                    break;

                default:
                    Debug.LogWarning($"[MessageHandlerService] Unknown message type: {message.type}");
                    break;
            }
        }

        /// <summary>
        /// Handle authentication messages
        /// </summary>
        private void HandleAuthMessage(IUnityMessage message)
        {
            if (config.enableDebugLogs)
            {
                Debug.Log($"[MessageHandlerService] Handling auth message: {message.action}");
            }

            switch (message.action)
            {
                case AuthActions.AUTH_SUCCESS:
                    OnAuthSuccess?.Invoke(message as AuthSuccessMessage);
                    break;

                case AuthActions.AUTH_FAILED:
                    OnAuthFailed?.Invoke(message as AuthFailedMessage);
                    break;

                case AuthActions.LOGGED_OUT:
                    OnLoggedOut?.Invoke(message as LoggedOutMessage);
                    break;

                case AuthActions.HANDLE_AUTHENTICATED_USER:
                    OnAuthenticatedUser?.Invoke(message as HandleAuthenticatedUserMessage);
                    break;

                case AuthActions.JWT_TOKEN_RESPONSE:
                    OnJwtTokenResponse?.Invoke(message as JwtTokenResponseMessage);
                    break;

                default:
                    Debug.LogWarning($"[MessageHandlerService] Unknown auth action: {message.action}");
                    break;
            }
        }

        /// <summary>
        /// Handle wallet messages
        /// </summary>
        private void HandleWalletMessage(IUnityMessage message)
        {
            if (config.enableDebugLogs)
            {
                Debug.Log($"[MessageHandlerService] Handling wallet message: {message.action}");
            }

            switch (message.action)
            {

                case WalletActions.SWITCH_NETWORK:
                    OnNetworkSwitched?.Invoke(message as BalanceResponseMessage);
                    break;

                case WalletActions.SWITCH_WALLET:
                    OnWalletSwitched?.Invoke(message as BalanceResponseMessage);
                    break;
                case WalletActions.BALANCE_RESPONSE:
                    OnBalanceResponse?.Invoke(message as BalanceResponseMessage);
                    break;

                case WalletActions.SIGN_MESSAGE_RESPONSE:
                    OnSignMessageResponse?.Invoke(message as SignMessageResponseMessage);
                    break;

                case WalletActions.TRANSACTION_RESPONSE:
                    OnTransactionResponse?.Invoke(message as TransactionResponseMessage);
                    break;

                case WalletActions.WALLET_CONNECTED:
                    OnWalletConnected?.Invoke(message as WalletConnectedMessage);
                    break;

                case WalletActions.WALLET_DISCONNECTED:
                    OnWalletDisconnected?.Invoke(message as WalletDisconnectedMessage);
                    break;

                case WalletActions.WALLET_ERROR:
                    OnWalletError?.Invoke(message as WalletErrorMessage);
                    break;

                case WalletActions.WALLETS_RESPONSE:
                    OnWalletsResponse?.Invoke(message as WalletsResponseMessage);
                    break;

                case WalletActions.NETWORKS_RESPONSE:
                    OnNetworksResponse?.Invoke(message as NetworksResponseMessage);
                    break;

                default:
                    Debug.LogWarning($"[MessageHandlerService] Unknown wallet action: {message.action}");
                    break;
            }
        }

        /// <summary>
        /// Handle legacy connected message format
        /// </summary>
        private void HandleLegacyConnectedMessage(UniWebViewMessage msg)
        {
            if (config.enableDebugLogs)
            {
                Debug.Log("[MessageHandlerService] Handling legacy connected message");
            }

            // Create a wallet connected message from legacy format
            var walletConnectedMessage = new WalletConnectedMessage
            {
                data = new WalletConnectedData
                {
                    success = true,
                    wallet = new WalletCredential
                    {
                        address = msg.Args.ContainsKey("address") ? msg.Args["address"] : "",
                        chain = msg.Args.ContainsKey("chain") ? msg.Args["chain"] : "",
                        walletName = "Legacy Wallet"
                    }
                }
            };

            OnWalletConnected?.Invoke(walletConnectedMessage);
        }
    }
}
using System.Collections;
using UnityEngine;
using DynamicSDK.Unity.Core;
using DynamicSDK.Unity.Messages;
using DynamicSDK.Unity.Messages.Auth;
using DynamicSDK.Unity.Messages.Wallet;
using DynamicSDK.Unity.Utils;
using Newtonsoft.Json;

/// <summary>
/// Refactored WebView connector using service-oriented architecture
/// Manages Dynamic SDK integration with improved separation of concerns
/// </summary>
public class WebViewConnector : MonoBehaviour
{
    [Header("SDK Configuration")] [SerializeField]
    private DynamicSDKConfig config;

    // Core service components - removed UIManager dependency
    private WebViewService        webViewService;
    private MessageHandlerService messageHandler;

    // Current wallet state
    private string currentWalletAddress;

    // ============================================================================
    // EVENTS FOR DYNAMICSDKMANAGER
    // ============================================================================
    public System.Action<string>           OnWalletConnected;
    public System.Action<UserInfo>         OnUserAuthenticated;
    public System.Action<WalletCredential> OnWalletInfoUpdated;
    public System.Action                   OnWalletDisconnected;
    public System.Action<string>           OnTransactionSent;
    public System.Action<string>           OnMessageSigned;
    public System.Action<string>           OnBalanceUpdated;
    public System.Action<string>           OnError;
    public System.Action<JwtTokenResponseMessage> OnJwtTokenReceived;

    // ============================================================================
    // PUBLIC API METHODS FOR DYNAMICSDKMANAGER
    // ============================================================================

    /// <summary>
    /// Connect wallet - public method for DynamicSDKManager
    /// </summary>
    public void ConnectWallet() { ConnectWalletInternal(); }

    /// <summary>
    /// Disconnect wallet - public method for DynamicSDKManager
    /// </summary>
    public void DisconnectWallet() { DisconnectWalletInternal(); }

    /// <summary>
    /// Sign message - public method for DynamicSDKManager
    /// </summary>
    public void SignMessage(string message) { SignMessageInternal(message); }

    /// <summary>
    /// Send transaction - public method for DynamicSDKManager
    /// </summary>
    public void SendTransaction(string to, string value, string data = "", string network = "mainnet") { SendTransactionInternal(to, value, data, network); }

    /// <summary>
    /// Get balance - public method for DynamicSDKManager
    /// </summary>
    public void GetBalance() { GetBalanceInternal(); }

    /// <summary>
    /// Check connection status - public method for DynamicSDKManager
    /// </summary>
    public void CheckConnectionStatus()
    {
        // Implementation for checking connection status
        bool isConnected = !string.IsNullOrEmpty(currentWalletAddress);

        if (isConnected)
        {
            OnWalletConnected?.Invoke(currentWalletAddress);
        }
        else
        {
            OnWalletDisconnected?.Invoke();
        }
    }

    /// <summary>
    /// Open profile - public method for DynamicSDKManager
    /// </summary>
    public void OpenProfile() { OpenProfileInternal(); }

    /// <summary>
    /// Get JWT token - public method for DynamicSDKManager
    /// </summary>
    public void GetJwtToken() { GetJwtTokenInternal(); }

    // ============================================================================
    // LIFECYCLE METHODS
    // ============================================================================

    void Awake()
    {
        InitializeServices();
        SetupEventHandlers();
    }

    private void InitializeServices()
    {
        // Load configuration
        if (config != null)
        {
            DynamicSDKConfig.LoadFromAsset(config);
        }

        // Initialize services
        webViewService = GetComponent<WebViewService>();

        if (webViewService == null)
        {
            webViewService = gameObject.AddComponent<WebViewService>();
        }

        messageHandler = new MessageHandlerService();
    }

    private void SetupEventHandlers()
    {
        // WebView Service events
        if (webViewService != null)
        {
            webViewService.OnMessageReceived += messageHandler.ProcessMessage;
        }

        // Message Handler events
        SetupMessageHandlerEvents();
    }

    private void SetupMessageHandlerEvents()
    {
        messageHandler.OnAuthSuccess       += HandleAuthSuccess;
        messageHandler.OnAuthFailed        += HandleAuthFailed;
        messageHandler.OnLoggedOut         += HandleLoggedOut;
        messageHandler.OnAuthenticatedUser += HandleAuthenticatedUser;
        messageHandler.OnJwtTokenResponse  += HandleJwtTokenResponse;

        messageHandler.OnBalanceResponse     += HandleBalanceResponse;
        messageHandler.OnSignMessageResponse += HandleSignMessageResponse;
        messageHandler.OnTransactionResponse += HandleTransactionResponse;
        messageHandler.OnWalletConnected     += HandleWalletConnected;
        messageHandler.OnWalletDisconnected  += HandleWalletDisconnected;
        messageHandler.OnWalletError         += HandleWalletError;
    }

    // ============================================================================
    // AUTH MESSAGE HANDLERS
    // ============================================================================

    private void HandleAuthSuccess(AuthSuccessMessage message)
    {
        if (message?.data == null)
        {
            Debug.LogError("[WebViewConnector] Auth success message data is null");

            return;
        }

        Debug.Log($"[WebViewConnector] Auth Success - User: {message.data.user?.name}, Provider: {message.data.provider}");

        // Display user info if available
        if (message.data.user != null)
        {
            // Fire user authenticated event
            OnUserAuthenticated?.Invoke(message.data.user);
        }

        // Display wallet info if available
        if (message.data.wallets != null && message.data.wallets.Length > 0)
        {
            var wallet = message.data.wallets[0];
            UpdateWalletInfo(wallet);

            // Fire wallet connected event for scene transition
            if (!string.IsNullOrEmpty(currentWalletAddress))
            {
                OnWalletConnected?.Invoke(currentWalletAddress);
            }
        }

        // Store session token if needed
        // PlayerPrefs.SetString("SessionToken", message.data.sessionToken);

        // Hide webview
        webViewService?.HideWithAnimation();
    }

    private void HandleAuthFailed(AuthFailedMessage message)
    {
        if (message?.data == null)
        {
            Debug.LogError("[WebViewConnector] Auth failed message data is null");

            return;
        }

        Debug.LogError($"[WebViewConnector] Auth Failed - Error: {message.data.error}, Code: {message.data.errorCode}");

        // Hide webview
        webViewService?.HideWithAnimation();
    }

    private void HandleLoggedOut(LoggedOutMessage message)
    {
        if (message?.data != null)
        {
            Debug.Log($"[WebViewConnector] User logged out - Success: {message.data.success}, Timestamp: {message.data.timestamp}");
        }
        else
        {
            Debug.Log("[WebViewConnector] User logged out");
        }

        // Clear wallet info and reset state
        ClearWalletInfo();

        // Clear session data
        // PlayerPrefs.DeleteKey("SessionToken");

        // Fire disconnect event for DynamicSDKManager
        OnWalletDisconnected?.Invoke();

        // Hide webview
        webViewService?.HideWithAnimation();
    }

    private void HandleAuthenticatedUser(HandleAuthenticatedUserMessage message)
    {
        if (message?.data == null)
        {
            Debug.LogError("[WebViewConnector] Authenticated user message data is null");

            return;
        }

        Debug.Log($"[WebViewConnector] Authenticated User - User: {message.data.user?.name}");

        // Display user info if available
        if (message.data.user != null)
        {
            // Fire user authenticated event
            OnUserAuthenticated?.Invoke(message.data.user);
        }

        // Display wallet info if available
        if (message.data.wallets != null && message.data.wallets.Length > 0)
        {
            var wallet = message.data.wallets[0];
            UpdateWalletInfo(wallet);

            // Fire wallet connected event for scene transition
            if (!string.IsNullOrEmpty(currentWalletAddress))
            {
                OnWalletConnected?.Invoke(currentWalletAddress);
            }
        }

        // Store session token
        // PlayerPrefs.SetString("SessionToken", message.data.sessionToken);

        webViewService?.HideWithAnimation();
    }

    private void HandleJwtTokenResponse(JwtTokenResponseMessage message)
    {
        if (message?.data == null)
        {
            Debug.LogError("[WebViewConnector] JWT token response message data is null");
            OnError?.Invoke("Failed to get JWT token: Invalid response");
            return;
        }

        Debug.Log($"[WebViewConnector] JWT Token Response - Token: {message.data.token?.Substring(0, 20)}..., UserId: {message.data.userId}, Email: {message.data.email}");

        // Fire JWT token received event
        OnJwtTokenReceived?.Invoke(message);

        // Hide webview
        webViewService?.HideWithAnimation();
    }

    // ============================================================================
    // UTILITY METHODS
    // ============================================================================

    private void UpdateWalletInfo(WalletCredential wallet)
    {
        if (wallet == null) return;

        currentWalletAddress = wallet.address;

        Debug.Log($"[WebViewConnector] Wallet Info - Address: {FormatAddress(wallet.address)}, Chain: {wallet.chain}, Balance: {wallet.balance}");

        // Fire wallet info updated event
        OnWalletInfoUpdated?.Invoke(wallet);
    }

    private void ClearWalletInfo()
    {
        currentWalletAddress = null;
        Debug.Log("[WebViewConnector] Wallet info cleared");
    }

    // ============================================================================
    // WALLET MESSAGE HANDLERS
    // ============================================================================

    private void HandleBalanceResponse(BalanceResponseMessage message)
    {
        if (message?.data == null)
        {
            Debug.LogError("[WebViewConnector] Balance response message data is null");
            OnError?.Invoke("Failed to get balance: Invalid response");

            return;
        }

        if (message.data.success)
        {
            Debug.Log($"[WebViewConnector] Balance Response - Address: {message.data.walletAddress}, Balance: {message.data.balance} {message.data.symbol}");

            // Update current wallet address from balance response
            currentWalletAddress = message.data.walletAddress;

            // If we have chain info in balance response, create/update wallet info
            if (!string.IsNullOrEmpty(message.data.chain))
            {
                var walletInfo = new WalletCredential
                {
                    address  = message.data.walletAddress,
                    chain    = message.data.chain,
                    balance  = message.data.balance,
                    symbol   = message.data.symbol,
                    decimals = message.data.decimals,
                    network  = "mainnet" // Default network assumption
                };

                // Fire wallet info updated event
                OnWalletInfoUpdated?.Invoke(walletInfo);
            }

            // Fire balance updated event
            OnBalanceUpdated?.Invoke($"{message.data.balance} {message.data.symbol}");
        }
        else
        {
            Debug.LogError($"[WebViewConnector] Balance request failed: {message.data.error}");
            OnError?.Invoke($"Failed to get balance: {message.data.error}");
        }
    }

    private void HandleSignMessageResponse(SignMessageResponseMessage message)
    {
        if (message?.data == null)
        {
            Debug.LogError("[WebViewConnector] Sign message response data is null");
            OnError?.Invoke("Failed to sign message: Invalid response");

            return;
        }

        if (message.data.success)
        {
            Debug.Log($"[WebViewConnector] Message signed successfully: {message.data.signature}");

            // Fire message signed event
            OnMessageSigned?.Invoke(message.data.signature);
        }
        else
        {
            Debug.LogError($"[WebViewConnector] Sign message failed: {message.data.error}");
            OnError?.Invoke($"Sign failed: {message.data.error}");
        }

        // Hide webview after signing operation
        webViewService?.HideWithAnimation();
    }

    private void HandleTransactionResponse(TransactionResponseMessage message)
    {
        if (message?.data == null)
        {
            Debug.LogError("[WebViewConnector] Transaction response data is null");
            OnError?.Invoke("Failed to send transaction: Invalid response");

            return;
        }

        if (message.data.success)
        {
            Debug.Log($"[WebViewConnector] Transaction successful: {message.data.transactionHash}");

            // Fire transaction sent event
            OnTransactionSent?.Invoke(message.data.transactionHash);
        }
        else
        {
            Debug.LogError($"[WebViewConnector] Transaction failed: {message.data.error}");
            OnError?.Invoke($"Transaction failed: {message.data.error}");
        }

        // Hide webview after transaction operation
        webViewService?.HideWithAnimation();
    }

    private void HandleWalletConnected(WalletConnectedMessage message)
    {
        if (message?.data == null)
        {
            Debug.LogError("[WebViewConnector] Wallet connected message data is null");
            OnError?.Invoke("Wallet connection failed: Invalid response");

            return;
        }

        if (message.data.success)
        {
            Debug.Log($"[WebViewConnector] Wallet Connected - Address: {message.data.wallet?.address}, Chain: {message.data.wallet?.chain}");

            if (message.data.wallet != null)
            {
                // Update wallet info
                UpdateWalletInfo(message.data.wallet);

                // Fire wallet connected event
                OnWalletConnected?.Invoke(message.data.wallet.address);
            }

            // Hide webview
            webViewService?.HideWithAnimation();
        }
        else
        {
            OnError?.Invoke("Wallet connection failed");
        }
    }

    private void HandleWalletDisconnected(WalletDisconnectedMessage message)
    {
        Debug.Log($"[WebViewConnector] Wallet Disconnected - Address: {message?.data?.walletAddress}, Reason: {message?.data?.reason}");

        // Clear wallet info and update state
        ClearWalletInfo();

        // Fire wallet disconnected event
        OnWalletDisconnected?.Invoke();

        // Hide webview
        webViewService?.HideWithAnimation();
    }

    private void HandleWalletError(WalletErrorMessage message)
    {
        if (message?.data == null)
        {
            Debug.LogError("[WebViewConnector] Wallet error message data is null");

            return;
        }

        Debug.LogError($"[WebViewConnector] Wallet Error: {message.data.error}");

        // Fire error event
        OnError?.Invoke($"Wallet error: {message.data.error}");
    }

    // ============================================================================
    // BUTTON HANDLERS
    // ============================================================================

    private void ConnectWalletInternal()
    {
        if (!string.IsNullOrEmpty(currentWalletAddress))
        {
            Debug.Log("[WebViewConnector] Wallet already connected");

            return;
        }

        Debug.Log("[WebViewConnector] Connect wallet requested");

        // Make sure the WebView is open and send request
        webViewService?.OpenBottomSheet();
        webViewService?.RetryWithDelay(SendConnectWalletRequest, 1.0f);
    }

    private void SendConnectWalletRequest()
    {
        string message = RequestBuilder.BuildConnectWalletRequest();
        webViewService?.SendMessage(message);
    }

    private void DisconnectWalletInternal()
    {
        if (string.IsNullOrEmpty(currentWalletAddress))
        {
            Debug.Log("[WebViewConnector] No wallet connected");

            return;
        }

        Debug.Log("[WebViewConnector] Disconnect wallet requested");

        // Open WebView if needed and send disconnect request
        webViewService?.OpenBottomSheet();
        webViewService?.RetryWithDelay(SendDisconnectRequest, 1.0f);
    }

    private void SendDisconnectRequest()
    {
        string message = RequestBuilder.BuildDisconnectRequest();
        webViewService?.SendMessage(message);

        Debug.Log($"[WebViewConnector] Sent disconnect request: {message}");

        // Update Unity-side state immediately (you can wait for Web feedback if preferred)
        ClearWalletInfo();

        // Fire disconnect event for DynamicSDKManager
        OnWalletDisconnected?.Invoke();
    }

    private void SignMessageInternal()
    {
        // This method would need to get message from external source
        // since we removed UIManager dependency
        Debug.LogWarning("[WebViewConnector] SignMessageInternal() called without message parameter");
    }

    private void SignMessageInternal(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            Debug.LogError("[WebViewConnector] Message cannot be empty");
            OnError?.Invoke("Please enter a message to sign");

            return;
        }

        // Check if wallet is connected
        if (string.IsNullOrEmpty(currentWalletAddress))
        {
            Debug.LogError("[WebViewConnector] No wallet connected");
            OnError?.Invoke("No wallet connected");

            return;
        }

        Debug.Log($"[WebViewConnector] Sign message requested: {message}");

        // Open WebView if needed and send request
        webViewService?.OpenBottomSheet();
        webViewService?.RetryWithDelay(() => SendSignMessageRequest(message), 1.0f);
    }

    private void SendSignMessageRequest(string message)
    {
        if (!RequestBuilder.Validator.IsValidMessage(message))
        {
            OnError?.Invoke("Invalid message");

            return;
        }

        string request = RequestBuilder.BuildSignMessageRequest(currentWalletAddress, message);
        webViewService?.SendMessage(request);

        Debug.Log($"[WebViewConnector] Sent sign message request: {request}");
    }

    private void SendTransactionInternal(string to, string value, string data = "", string network = "mainnet")
    {
        if (string.IsNullOrEmpty(to) || string.IsNullOrEmpty(value))
        {
            Debug.LogError("[WebViewConnector] Invalid transaction parameters");
            OnError?.Invoke("Invalid transaction parameters");

            return;
        }

        // Check if wallet is connected
        if (string.IsNullOrEmpty(currentWalletAddress))
        {
            Debug.LogError("[WebViewConnector] No wallet connected");
            OnError?.Invoke("No wallet connected");

            return;
        }

        // Validate inputs
        if (!RequestBuilder.Validator.IsValidAddress(to))
        {
            Debug.LogError("[WebViewConnector] Invalid recipient address format");
            OnError?.Invoke("Invalid recipient address");

            return;
        }

        if (!RequestBuilder.Validator.IsValidAmount(value))
        {
            Debug.LogError("[WebViewConnector] Invalid amount format");
            OnError?.Invoke("Invalid amount");

            return;
        }

        Debug.Log($"[WebViewConnector] Send transaction requested: {value} to {to} on {network}");

        // Open WebView if needed and send request
        webViewService?.OpenBottomSheet();
        webViewService?.RetryWithDelay(() => SendTransactionRequest(to, value, network), 1.0f);
    }

    private void SendTransactionRequest(string toAddress, string amount, string network = "mainnet")
    {
        string request = RequestBuilder.BuildTransactionRequest(currentWalletAddress, toAddress, amount, "sui", network);
        webViewService?.SendMessage(request);

        Debug.Log($"[WebViewConnector] Sent transaction request: {request}");
    }

    private void GetBalanceInternal()
    {
        if (string.IsNullOrEmpty(currentWalletAddress))
        {
            Debug.LogError("[WebViewConnector] No wallet connected");
            OnError?.Invoke("No wallet connected");

            return;
        }

        Debug.Log("[WebViewConnector] Getting wallet balance...");
        string message = RequestBuilder.BuildGetBalanceRequest(currentWalletAddress);
        webViewService?.SendMessage(message);
    }

    // ============================================================================
    // UTILITY METHODS
    // ============================================================================

    private string FormatAddress(string address)
    {
        if (string.IsNullOrEmpty(address) || address.Length <= 10)
            return address;

        return $"{address.Substring(0, 6)}...{address.Substring(address.Length - 4)}";
    }

    private string GetFullWalletAddress() { return currentWalletAddress; }

    void OnDestroy()
    {
        // Clean up services
        webViewService?.CloseWebView();
    }

    private void OpenProfileInternal()
    {
        // Check if wallet is connected
        if (string.IsNullOrEmpty(currentWalletAddress))
        {
            Debug.LogError("[WebViewConnector] No wallet connected");
            OnError?.Invoke("No wallet connected");

            return;
        }

        Debug.Log("[WebViewConnector] Opening profile");

        // Open WebView if needed and send request
        webViewService?.OpenBottomSheet();
        webViewService?.RetryWithDelay(() => SendOpenProfileRequest(), 1.0f);
    }

    private void SendOpenProfileRequest()
    {
        string request = RequestBuilder.BuildOpenProfileRequest(currentWalletAddress);
        webViewService?.SendMessage(request);

        Debug.Log($"[WebViewConnector] Sent open profile request: {request}");
    }

    private void GetJwtTokenInternal()
    {
        Debug.Log("[WebViewConnector] Getting JWT token");

        // Open WebView if needed and send request
        webViewService?.OpenBottomSheet();
        webViewService?.RetryWithDelay(() => SendGetJwtTokenRequest(), 1.0f);
    }

    private void SendGetJwtTokenRequest()
    {
        string request = RequestBuilder.BuildGetJwtTokenRequest();
        webViewService?.SendMessage(request);

        Debug.Log($"[WebViewConnector] Sent get JWT token request: {request}");
    }
}
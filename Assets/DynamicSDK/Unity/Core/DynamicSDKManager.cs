using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DynamicSDK.Unity.Messages;

namespace DynamicSDK.Unity.Core
{
    using DynamicSDK.Unity.Messages.Auth;
    using DynamicSDK.Unity.Messages.Wallet;

    /// <summary>
    /// Main SDK Manager - Singleton pattern for easy access across all scenes
    /// Manages all Dynamic SDK functionality in one place
    /// </summary>
    public class DynamicSDKManager : MonoBehaviour
    {
        #region Singleton Implementation

        private static DynamicSDKManager _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        public static DynamicSDKManager Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning("[DynamicSDKManager] Instance requested during application quit. Returning null.");

                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<DynamicSDKManager>();

                        if (_instance == null)
                        {
                            GameObject go = new GameObject("DynamicSDKManager");
                            _instance = go.AddComponent<DynamicSDKManager>();
                            DontDestroyOnLoad(go);
                        }
                    }

                    return _instance;
                }
            }
        }

        #endregion

        #region Events

        // Authentication Events
        public static event Action<bool> OnConnectionStatusChanged;
        public static event Action<string> OnWalletConnected;
        public static event Action<UserInfo> OnUserAuthenticated;
        public static event Action<WalletCredential> OnWalletInfoUpdated;
        public static event Action OnWalletDisconnected;
        public static event Action<JwtTokenResponseMessage> OnJwtTokenReceived;

        // Wallet Events  
        public static event Action<string> OnTransactionSent;
        public static event Action<string> OnMessageSigned;
        public static event Action<BalanceResponseData> OnBalanceUpdated;
        public static event Action<BalanceResponseData> OnWalletSwitched;
        public static event Action<BalanceResponseData> OnNetworkSwitched;
        public static event Action<WalletsResponseData> OnWalletsReceived;
        public static event Action<NetworksResponseData> OnNetworksReceived;

        // WebView Events
        public static event Action OnWebViewClosed;
        public static event Action OnWebViewReady;

        // General Events
        public static event Action OnSDKInitialized;
        public static event Action<string> OnSDKError;

        #endregion

        #region Private Components

        private WebViewConnector webViewConnector;
        private WebViewService webViewService;
        private DynamicSDKConfig config;

        private bool isInitialized = false;
        private bool isWebViewReady = false;
        private string currentWalletAddress = "";
        private bool isConnected = false;
        private UserInfo currentUserInfo = null;
        private WalletCredential currentWalletInfo = null;

        #endregion

        #region Public Properties

        /// <summary>
        /// Check if SDK is initialized and ready to use
        /// </summary>
        public bool IsInitialized => isInitialized;
        public bool IsWebViewReady => isWebViewReady;

        /// <summary>
        /// Check if wallet is currently connected
        /// </summary>
        public bool IsWalletConnected => isConnected;

        /// <summary>
        /// Get current connected wallet address
        /// </summary>
        public string CurrentWalletAddress => currentWalletAddress;

        /// <summary>
        /// Get current authenticated user information
        /// </summary>
        public UserInfo CurrentUserInfo => currentUserInfo;

        /// <summary>
        /// Get current wallet credential information
        /// </summary>
        public WalletCredential CurrentWalletInfo => currentWalletInfo;

        /// <summary>
        /// Access to SDK configuration
        /// </summary>
        public DynamicSDKConfig Config => config;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Ensure only one instance exists
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSDK();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnApplicationQuit() { _applicationIsQuitting = true; }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the Dynamic SDK with default configuration
        /// </summary>
        public void InitializeSDK() { InitializeSDK(null); }

        /// <summary>
        /// Initialize the Dynamic SDK with custom configuration
        /// </summary>
        /// <param name="customConfig">Custom configuration, if null uses default</param>
        public void InitializeSDK(DynamicSDKConfig customConfig)
        {
            if (isInitialized)
            {
                Debug.LogWarning("[DynamicSDKManager] SDK already initialized!");

                return;
            }

            try
            {
                // Load configuration
                config = customConfig ?? DynamicSDKConfig.Instance;

                if (config.enableDebugLogs)
                {
                    Debug.Log("[DynamicSDKManager] Initializing Dynamic SDK...");
                }

                // Initialize core components
                InitializeComponents();
                SetupEventHandlers();

                isInitialized = true;

                if (config.enableDebugLogs)
                {
                    Debug.Log("[DynamicSDKManager] Dynamic SDK initialized successfully!");
                }

                // Pre-load webview if enabled
                if (config.enableWebViewPreload)
                {
                    StartCoroutine(PreloadWebViewAfterDelay());
                }

                OnSDKInitialized?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DynamicSDKManager] Failed to initialize SDK: {ex.Message}");
                OnSDKError?.Invoke($"Initialization failed: {ex.Message}");
            }
        }

        private void InitializeComponents()
        {
            // Create WebViewConnector if not exists
            if (webViewConnector == null)
            {
                webViewConnector = gameObject.GetComponent<WebViewConnector>();

                if (webViewConnector == null)
                {
                    webViewConnector = gameObject.AddComponent<WebViewConnector>();
                }
            }

            // Get references to other components
            webViewService = webViewConnector.GetComponent<WebViewService>();

            if (webViewService == null)
            {
                webViewService = gameObject.AddComponent<WebViewService>();
            }
        }

        private void SetupEventHandlers()
        {
            // Subscribe to WebViewConnector events
            if (webViewConnector != null)
            {
                webViewConnector.OnWalletConnected += HandleWalletConnected;
                webViewConnector.OnUserAuthenticated += HandleUserAuthenticated;
                webViewConnector.OnWalletInfoUpdated += HandleWalletInfoUpdated;
                webViewConnector.OnWalletDisconnected += HandleWalletDisconnected;
                webViewConnector.OnTransactionSent += HandleTransactionSent;
                webViewConnector.OnMessageSigned += HandleMessageSigned;
                webViewConnector.OnBalanceUpdated += HandleBalanceUpdated;
                webViewConnector.OnWalletsReceived += HandleWalletsReceived;
                webViewConnector.OnWalletSwitched += HandleWalletSwitched;
                webViewConnector.OnNetworkSwitched += HandleNetworkSwitched;
                webViewConnector.OnNetworksReceived += HandleNetworksReceived;
                webViewConnector.OnError += HandleError;
                webViewConnector.OnJwtTokenReceived += HandleJwtTokenReceived;
            }

            // Subscribe to WebViewService events
            if (webViewService != null)
            {
                webViewService.OnWebViewClosed += HandleWebViewClosed;
                webViewService.OnWebViewReady += HandleWebViewReady;
            }
        }




        private System.Collections.IEnumerator PreloadWebViewAfterDelay()
        {
            // Wait a short delay to ensure all components are fully ready
            yield return new WaitForSeconds(1.0f);

            if (config.enableDebugLogs)
            {
                Debug.Log("[DynamicSDKManager] Starting WebView preload...");
            }

            // Pre-load the webview in background
            webViewService?.PreloadWebView();
        }

        #endregion

        #region Public API - Authentication

        /// <summary>
        /// Connect wallet - opens the Dynamic authentication flow
        /// </summary>
        public void ConnectWallet()
        {
            if (!isInitialized)
            {
                Debug.LogError("[DynamicSDKManager] SDK not initialized! Call InitializeSDK() first.");
                OnSDKError?.Invoke("SDK not initialized");

                return;
            }

            if (config.enableDebugLogs)
            {
                Debug.Log("[DynamicSDKManager] Connecting wallet...");
            }

            webViewConnector?.ConnectWallet();
        }

        /// <summary>
        /// Disconnect current wallet
        /// </summary>
        public void DisconnectWallet()
        {
            if (!isInitialized)
            {
                Debug.LogError("[DynamicSDKManager] SDK not initialized!");

                return;
            }

            if (config.enableDebugLogs)
            {
                Debug.Log("[DynamicSDKManager] Disconnecting wallet...");
            }

            webViewConnector?.DisconnectWallet();
        }

        /// <summary>
        /// Check current connection status
        /// </summary>
        public void CheckConnectionStatus()
        {
            if (!isInitialized)
            {
                Debug.LogError("[DynamicSDKManager] SDK not initialized!");

                return;
            }

            webViewConnector?.CheckConnectionStatus();
        }

        #endregion

        #region Public API - Wallet Operations

        /// <summary>
        /// Sign a message with the connected wallet
        /// </summary>
        /// <param name="message">Message to sign</param>
        public void SignMessage(string message)
        {
            if (!isInitialized)
            {
                Debug.LogError("[DynamicSDKManager] SDK not initialized!");
                OnSDKError?.Invoke("SDK not initialized");

                return;
            }

            if (!isConnected)
            {
                Debug.LogError("[DynamicSDKManager] No wallet connected!");
                OnSDKError?.Invoke("No wallet connected");

                return;
            }

            if (string.IsNullOrEmpty(message))
            {
                Debug.LogError("[DynamicSDKManager] Message cannot be empty!");
                OnSDKError?.Invoke("Message cannot be empty");

                return;
            }

            if (config.enableDebugLogs)
            {
                Debug.Log($"[DynamicSDKManager] Signing message: {message}");
            }

            webViewConnector?.SignMessage(message);
        }

        /// <summary>
        /// Send a transaction
        /// </summary>
        /// <param name="to">Recipient address</param>
        /// <param name="value">Value in wei</param>
        /// <param name="data">Transaction data (optional)</param>
        /// <param name="network">Network to use (mainnet/testnet)</param>
        public void SendTransaction(string to, string value, string data = "", string network = "mainnet")
        {
            if (!isInitialized)
            {
                Debug.LogError("[DynamicSDKManager] SDK not initialized!");
                OnSDKError?.Invoke("SDK not initialized");

                return;
            }

            if (!isConnected)
            {
                Debug.LogError("[DynamicSDKManager] No wallet connected!");
                OnSDKError?.Invoke("No wallet connected");

                return;
            }

            if (string.IsNullOrEmpty(to) || string.IsNullOrEmpty(value))
            {
                Debug.LogError("[DynamicSDKManager] Invalid transaction parameters!");
                OnSDKError?.Invoke("Invalid transaction parameters");

                return;
            }

            if (config.enableDebugLogs)
            {
                Debug.Log($"[DynamicSDKManager] Sending transaction to: {to}, value: {value}, network: {network}");
            }

            webViewConnector?.SendTransaction(to, value, data, network);
        }

        /// <summary>
        /// Get wallet balance
        /// </summary>
        public void GetBalance()
        {
            if (!isInitialized)
            {
                Debug.LogError("[DynamicSDKManager] SDK not initialized!");
                OnSDKError?.Invoke("SDK not initialized");

                return;
            }

            if (!isConnected)
            {
                Debug.LogError("[DynamicSDKManager] No wallet connected!");
                OnSDKError?.Invoke("No wallet connected");

                return;
            }

            if (config.enableDebugLogs)
            {
                Debug.Log("[DynamicSDKManager] Getting wallet balance...");
            }

            webViewConnector?.GetBalance();
        }

        /// <summary>
        /// Get available wallets
        /// </summary>
        public void GetWallets()
        {
            if (!isInitialized)
            {
                Debug.LogError("[DynamicSDKManager] SDK not initialized!");
                OnSDKError?.Invoke("SDK not initialized");

                return;
            }

            if (config.enableDebugLogs)
            {
                Debug.Log("[DynamicSDKManager] Getting available wallets...");
            }

            webViewConnector?.GetWallets();
        }

        /// <summary>
        /// Get available networks
        /// </summary>
        public void GetNetworks()
        {
            if (!isInitialized)
            {
                Debug.LogError("[DynamicSDKManager] SDK not initialized!");
                OnSDKError?.Invoke("SDK not initialized");

                return;
            }

            if (config.enableDebugLogs)
            {
                Debug.Log("[DynamicSDKManager] Getting available networks...");
            }

            webViewConnector?.GetNetworks();
        }

        /// <summary>
        /// Open WebView for user profile/dashboard
        /// </summary>
        public void OpenProfile()
        {
            if (!isInitialized)
            {
                Debug.LogError("[DynamicSDKManager] SDK not initialized!");
                OnSDKError?.Invoke("SDK not initialized");

                return;
            }

            if (!isConnected)
            {
                Debug.LogError("[DynamicSDKManager] No wallet connected!");
                OnSDKError?.Invoke("No wallet connected");

                return;
            }

            if (config.enableDebugLogs)
            {
                Debug.Log("[DynamicSDKManager] Opening profile");
            }

            webViewConnector?.OpenProfile();
        }

        /// <summary>
        /// Get JWT token for authenticated user
        /// </summary>
        public void GetJwtToken()
        {
            if (!isInitialized)
            {
                Debug.LogError("[DynamicSDKManager] SDK not initialized!");
                OnSDKError?.Invoke("SDK not initialized");

                return;
            }

            if (config.enableDebugLogs)
            {
                Debug.Log("[DynamicSDKManager] Getting JWT token...");
            }

            webViewConnector?.GetJwtToken();
        }

        /// <summary>
        /// Switch to a specific wallet
        /// </summary>
        /// <param name="walletId">The ID of the wallet to switch to</param>
        public void SwitchWallet(string walletId)
        {
            if (!isInitialized)
            {
                Debug.LogError("[DynamicSDKManager] SDK not initialized!");
                OnSDKError?.Invoke("SDK not initialized");

                return;
            }

            if (config.enableDebugLogs)
            {
                Debug.Log($"[DynamicSDKManager] Switching to wallet: {walletId}");
            }

            webViewConnector?.SwitchWallet(walletId);
        }

        /// <summary>
        /// Switch to a specific network
        /// </summary>
        /// <param name="networkChainId">The chain ID of the network to switch to</param>
        public void SwitchNetwork(string networkChainId)
        {
            if (!isInitialized)
            {
                Debug.LogError("[DynamicSDKManager] SDK not initialized!");
                OnSDKError?.Invoke("SDK not initialized");

                return;
            }

            if (config.enableDebugLogs)
            {
                Debug.Log($"[DynamicSDKManager] Switching to network: {networkChainId}");
            }

            webViewConnector?.SwitchNetwork(networkChainId);
        }

        /// <summary>
        /// Manually pre-load WebView in background (if not already done)
        /// </summary>
        public void PreloadWebView()
        {
            if (!isInitialized)
            {
                Debug.LogError("[DynamicSDKManager] SDK not initialized!");

                return;
            }

            if (config.enableDebugLogs)
            {
                Debug.Log("[DynamicSDKManager] Manual WebView preload requested...");
            }

            webViewService?.PreloadWebView();
        }

        #endregion

        #region Public API - Configuration

        /// <summary>
        /// Update SDK configuration at runtime
        /// </summary>
        /// <param name="newConfig">New configuration</param>
        public void UpdateConfiguration(DynamicSDKConfig newConfig)
        {
            if (newConfig == null)
            {
                Debug.LogError("[DynamicSDKManager] Configuration cannot be null!");

                return;
            }

            config = newConfig;
            DynamicSDKConfig.LoadFromAsset(newConfig);

            if (config.enableDebugLogs)
            {
                Debug.Log("[DynamicSDKManager] Configuration updated successfully!");
            }
        }

        /// <summary>
        /// Enable or disable debug logging
        /// </summary>
        /// <param name="enabled">Enable debug logs</param>
        public void SetDebugLogging(bool enabled)
        {
            if (config != null)
            {
                config.enableDebugLogs = enabled;
                Debug.Log($"[DynamicSDKManager] Debug logging {(enabled ? "enabled" : "disabled")}");
            }
        }

        #endregion

        #region Event Handlers

        private void HandleWalletConnected(string walletAddress)
        {
            currentWalletAddress = walletAddress;
            isConnected = true;

            if (config.enableDebugLogs)
            {
                Debug.Log($"[DynamicSDKManager] Wallet connected: {walletAddress}");
            }

            OnConnectionStatusChanged?.Invoke(true);
            OnWalletConnected?.Invoke(walletAddress);
        }

        private void HandleUserAuthenticated(UserInfo userInfo)
        {
            currentUserInfo = userInfo;

            if (config.enableDebugLogs)
            {
                Debug.Log($"[DynamicSDKManager] User authenticated: {userInfo?.name} ({userInfo?.email})");
            }

            OnUserAuthenticated?.Invoke(userInfo);
        }

        private void HandleWalletInfoUpdated(WalletCredential walletInfo)
        {
            currentWalletInfo = walletInfo;

            if (config.enableDebugLogs)
            {
                Debug.Log($"[DynamicSDKManager] Wallet info updated: {walletInfo}");
            }

            OnWalletInfoUpdated?.Invoke(walletInfo);
        }

        private void HandleWalletDisconnected()
        {
            currentWalletAddress = "";
            currentUserInfo = null;
            currentWalletInfo = null;
            isConnected = false;

            if (config.enableDebugLogs)
            {
                Debug.Log("[DynamicSDKManager] Wallet disconnected");
            }

            OnConnectionStatusChanged?.Invoke(false);
            OnWalletDisconnected?.Invoke();
        }

        private void HandleTransactionSent(string transactionHash)
        {
            if (config.enableDebugLogs)
            {
                Debug.Log($"[DynamicSDKManager] Transaction sent: {transactionHash}");
            }

            OnTransactionSent?.Invoke(transactionHash);
        }

        private void HandleMessageSigned(string signature)
        {
            if (config.enableDebugLogs)
            {
                Debug.Log($"[DynamicSDKManager] Message signed: {signature}");
            }

            OnMessageSigned?.Invoke(signature);
        }

        private void HandleBalanceUpdated(BalanceResponseData balance)
        {
            if (config.enableDebugLogs)
            {
                Debug.Log($"[DynamicSDKManager] Balance updated: {balance}");
            }

            OnBalanceUpdated?.Invoke(balance);
        }

        private void HandleWalletSwitched(BalanceResponseData balance)
        {
            if (config.enableDebugLogs)
            {
                Debug.Log($"[DynamicSDKManager] Wallet switched: {balance}");
            }

            OnWalletSwitched?.Invoke(balance);
        }

        private void HandleNetworkSwitched(BalanceResponseData balance)
        {
            if (config.enableDebugLogs)
            {
                Debug.Log($"[DynamicSDKManager] Network switched: {balance}");
            }

            OnNetworkSwitched?.Invoke(balance);

        }

        private void HandleWalletsReceived(WalletsResponseData walletsData)
        {
            if (config.enableDebugLogs)
            {
                Debug.Log($"[DynamicSDKManager] Wallets received: {walletsData}");
            }

            OnWalletsReceived?.Invoke(walletsData);
        }

        private void HandleNetworksReceived(NetworksResponseData networksData)
        {
            if (config.enableDebugLogs)
            {
                Debug.Log($"[DynamicSDKManager] Networks received: {networksData}");
            }

            OnNetworksReceived?.Invoke(networksData);
        }

        private void HandleError(string error)
        {
            Debug.LogError($"[DynamicSDKManager] Error: {error}");
            OnSDKError?.Invoke(error);
        }

        private void HandleWebViewClosed()
        {
            if (config.enableDebugLogs)
            {
                Debug.Log("[DynamicSDKManager] WebView closed");
            }

            OnWebViewClosed?.Invoke();
        }

        private void HandleWebViewReady()
        {
            if (config.enableDebugLogs)
            {
                Debug.Log("[DynamicSDKManager] WebView is ready");
            }
            isWebViewReady = true;
            OnWebViewReady?.Invoke();
        }

        private void HandleJwtTokenReceived(DynamicSDK.Unity.Messages.Auth.JwtTokenResponseMessage message)
        {
            if (config.enableDebugLogs)
            {
                Debug.Log($"[DynamicSDKManager] JWT token received: Token={message.data.token?.Substring(0, 20)}..., UserID={message.data.userId}, Email={message.data.email}");
            }

            OnJwtTokenReceived?.Invoke(message);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Get SDK status information
        /// </summary>
        /// <returns>Dictionary with SDK status</returns>
        public Dictionary<string, object> GetSDKStatus()
        {
            return new Dictionary<string, object>
            {
                ["isInitialized"] = isInitialized,
                ["isWalletConnected"] = isConnected,
                ["currentWalletAddress"] = currentWalletAddress,
                ["sdkVersion"] = "1.0.0",
                ["configUrl"] = config?.startUrl ?? "Not set"
            };
        }

        /// <summary>
        /// Reset SDK to initial state
        /// </summary>
        public void ResetSDK()
        {
            if (config.enableDebugLogs)
            {
                Debug.Log("[DynamicSDKManager] Resetting SDK...");
            }

            DisconnectWallet();
            currentWalletAddress = "";
            isConnected = false;

            // Close any open WebViews
            webViewService?.CloseWebView();
        }

        #endregion
    }
}
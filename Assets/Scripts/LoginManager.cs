using UnityEngine;
using UnityEngine.SceneManagement;
using DynamicSDK.Unity.Core;
using System.Collections;
using System;


public class LoginManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string mainCanvasSceneName = "MainCanvas";

    [Header("UI References")]
    [SerializeField] private UnityEngine.UI.Button connectButton;

    private DynamicSDKManager sdkManager;

    void Start()
    {
        // Get SDK Manager instance
        sdkManager = DynamicSDKManager.Instance;

        // Setup UI
        SetupUI();

        // Subscribe to events
        SubscribeToEvents();

        // Check connection status will be done in OnSDKInitialized to ensure proper timing
        // Don't check immediately to avoid timing issues with webview loading
    }

    private void SetupUI()
    {
        // Setup connect button
        if (connectButton != null)
        {
            connectButton.onClick.AddListener(OnConnectButtonClicked);
            SetConnectButtonEnabled(false);
        }

        // UI setup complete
    }

    private void SubscribeToEvents()
    {
        // Subscribe to wallet connection events
        DynamicSDKManager.OnWalletConnected += OnWalletConnected;
        DynamicSDKManager.OnWalletDisconnected += OnWalletDisconnected;
        DynamicSDKManager.OnSDKError += OnSDKError;
        DynamicSDKManager.OnSDKInitialized += OnSDKInitialized;
        DynamicSDKManager.OnWebViewClosed += OnWebViewClosed;
        DynamicSDKManager.OnWebViewReady += OnWebViewReady;
    }

    private void OnWebViewReady()
    {
        Debug.Log("[LoginManager] WebView is ready");
        SetConnectButtonEnabled(true);
    }

    private void CheckInitialConnectionStatus()
    {
        // Check if wallet is already connected from previous session
        if (sdkManager != null && sdkManager.IsWalletConnected)
        {
            Debug.Log("[LoginManager] Wallet already connected, proceeding to main scene");
            OnWalletConnected(sdkManager.CurrentWalletAddress);
        }
        else
        {
            // Ensure button is enabled when no wallet is connected
            UpdateUIForDisconnectedState();
        }
    }

    private void UpdateUIForDisconnectedState()
    {
        Debug.Log("[LoginManager] Updating UI for disconnected state");

        // Enable connect button for disconnected state
        if (sdkManager != null)
        {
            if (sdkManager.IsInitialized)
            {
                SetConnectButtonEnabled(sdkManager.IsWebViewReady);
            }
            else
            {
                // SDK not initialized yet, button will be enabled when OnSDKInitialized is called
                SetConnectButtonEnabled(false);
            }
        }
        else
        {
            SetConnectButtonEnabled(false);
        }
    }

    public void OnConnectButtonClicked()
    {
        Debug.Log("[LoginManager] Connect button clicked");

        if (sdkManager == null)
        {
            Debug.LogError("[LoginManager] SDK Manager not available!");
            return;
        }

        if (!sdkManager.IsInitialized)
        {
            Debug.Log("[LoginManager] SDK not initialized, waiting...");
            return;
        }

        // Update UI
        SetConnectButtonEnabled(false);

        // Connect wallet
        sdkManager.ConnectWallet();
    }

    #region Event Handlers
    private void OnSDKInitialized()
    {
        Debug.Log("[LoginManager] SDK initialized successfully");

        // Add a small delay to ensure all components are fully ready
        StartCoroutine(CheckConnectionAfterDelay());
    }

    private System.Collections.IEnumerator CheckConnectionAfterDelay()
    {
        // Wait a bit to ensure webview and all components are ready
        yield return new WaitForSeconds(0.5f);

        // Now check if wallet is already connected
        CheckInitialConnectionStatus();
    }

    private void OnWalletConnected(string walletAddress)
    {
        Debug.Log($"[LoginManager] Wallet connected: {walletAddress}");

        // Update UI
        SetConnectButtonEnabled(false);

        // Load main scene immediately
        LoadMainScene();
    }

    private void OnWalletDisconnected()
    {
        Debug.Log("[LoginManager] Wallet disconnected");
        UpdateUIForDisconnectedState();
    }

    private void OnSDKError(string error)
    {
        Debug.LogError($"[LoginManager] SDK error: {error}");
        SetConnectButtonEnabled(true);
    }

    private void OnWebViewClosed()
    {
        Debug.Log("[LoginManager] WebView closed - re-enabling connect button");

        // Only re-enable button if wallet is not connected
        // (avoid enabling button when user successfully connects and webview closes)
        if (!sdkManager.IsWalletConnected)
        {
            SetConnectButtonEnabled(true);
        }
    }
    #endregion

    #region Scene Management
    private void LoadMainScene()
    {
        try
        {
            Debug.Log($"[LoginManager] Loading scene: {mainCanvasSceneName}");
            SceneManager.LoadScene(mainCanvasSceneName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LoginManager] Failed to load scene '{mainCanvasSceneName}': {ex.Message}");
            SetConnectButtonEnabled(true);
        }
    }
    #endregion

    #region UI Helpers

    private void SetConnectButtonEnabled(bool enabled)
    {
        Debug.Log($"[LoginManager] Setting connect button enabled: {enabled}");
        if (connectButton != null)
        {
            connectButton.interactable = enabled;
        }
    }
    #endregion

    #region Unity Lifecycle
    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        DynamicSDKManager.OnWalletConnected -= OnWalletConnected;
        DynamicSDKManager.OnWalletDisconnected -= OnWalletDisconnected;
        DynamicSDKManager.OnSDKError -= OnSDKError;
        DynamicSDKManager.OnSDKInitialized -= OnSDKInitialized;
        DynamicSDKManager.OnWebViewClosed -= OnWebViewClosed;
        DynamicSDKManager.OnWebViewReady -= OnWebViewReady;
    }
    #endregion
}

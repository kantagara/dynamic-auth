using System;
using System.Collections;
using System.Collections.Generic;
using DynamicSDK.Unity.Core;
using DynamicSDK.Unity.Messages;
using DynamicSDK.Unity.Messages.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthScene : MonoBehaviour
{
    [SerializeField] TMP_Text Result;
    [SerializeField] Button LogInButton;
    [SerializeField] Button LogOutButton;

    DynamicSDKManager m_sdk;
    DynamicSDKManifest m_manifest;
    bool m_isWebviewReady = false;
    bool m_walletConnected = false;
    bool m_isAuthenticating;
    int m_authResult = default;

    bool m_waitingWebviewReady = false;

    void Awake()
    {
        DynamicSDKManager.OnWalletConnected += OnWalletConnected;
        DynamicSDKManager.OnWalletDisconnected += OnWalletDisconnected;
        DynamicSDKManager.OnUserAuthenticated += OnUserAuthenticated;
        DynamicSDKManager.OnJwtTokenReceived += OnJwtTokenReceived;
        DynamicSDKManager.OnSDKError += OnSDKError;
        DynamicSDKManager.OnWebViewReady += OnWebViewReady;
        DynamicSDKManager.OnWebViewClosed += OnWebViewClosed;

        LogInButton.onClick.AddListener(ShowDynamicAuth);
        LogOutButton.onClick.AddListener(Disconnect);
    }

    void OnDestroy()
    {
        DynamicSDKManager.OnWalletConnected -= OnWalletConnected;
        DynamicSDKManager.OnWalletDisconnected -= OnWalletDisconnected;
        DynamicSDKManager.OnUserAuthenticated -= OnUserAuthenticated;
        DynamicSDKManager.OnJwtTokenReceived -= OnJwtTokenReceived;
        DynamicSDKManager.OnSDKError -= OnSDKError;
        DynamicSDKManager.OnWebViewReady -= OnWebViewReady;
        DynamicSDKManager.OnWebViewClosed -= OnWebViewClosed;
    }

    void Start()
    {
        var config = DynamicSDKConfig.Instance;
        config.transitionDuration = 0.25f;
        config.enableClickOutsideToClose = false;

        m_manifest = Resources.Load<DynamicSDKManifest>("DynamicSDKManifest");
        if (m_manifest != null)
        {
            // simulate to set environmentId dynamically
            m_manifest.environmentId = RetrieveEnvironmentId();
        }

        m_sdk = DynamicSDKManager.Instance;
        ShowDynamicAuth();
    }

    /////////////////////////////////////////////////

    private string RetrieveEnvironmentId()
    {
        var useStaging = PlayerPrefs.GetInt("use-staging", 0) == 1;
        return useStaging ? "c1eed653-f1d1-4615-9fa7-181ad415f209" : "c1564a11-63ec-4236-8414-ec7972cc767f";
    }

    private void ShowDynamicAuth()
    {
        if (!m_sdk.IsInitialized)
        {
            m_sdk.InitializeSDK();
        }

        if (!m_isWebviewReady)
        {
            m_waitingWebviewReady = true;
            return;
        }

        if (!m_walletConnected)
        {
            m_isAuthenticating = true;
            m_authResult = default;

            m_sdk.ConnectWallet();
        }
        else
        {
            _ = GetJWT(delay: 0);
        }
    }

    private void Disconnect()
    {
        if (!m_walletConnected)
        {
            return;
        }

        m_sdk.DisconnectWallet();
    }

    //////////////////////////////////////////////////

    private void OnWalletConnected(string walletAddress)
    {
        Debug.Log($"[DynamicTest] Wallet connected: {walletAddress}");
        m_walletConnected = true;
        m_authResult = 1;
    }

    private void OnWalletDisconnected()
    {
        Debug.Log($"[DynamicTest] Wallet disconnected");
        m_walletConnected = false;
    }

    private void OnUserAuthenticated(UserInfo userInfo)
    {
        Debug.Log($"[DynamicTest] User authenticated: {userInfo.email}");
        m_authResult = 2;
    }

    private void OnJwtTokenReceived(JwtTokenResponseMessage jwtToken)
    {
        var jwt = jwtToken.data.token;
        Result.text = $"JWT token received:\n{jwt}";

        Debug.Log($"[DynamicTest] JWT token received: {jwt}");
    }

    private void OnSDKError(string error)
    {
        Debug.Log($"[DynamicTest] SDK Error: {error}");
        // m_authResult = -1;
    }

    private void OnWebViewReady()
    {
        Debug.Log("[DynamicTest] WebView ready");
        m_isWebviewReady = true;

        if (m_waitingWebviewReady)
        {
            m_waitingWebviewReady = false;

            if (!m_walletConnected)
            {
                m_isAuthenticating = true;
                m_authResult = default;

                m_sdk.ConnectWallet();
            }
            else
            {
                _ = GetJWT(delay: 0);
            }
        }
    }

    private void OnWebViewClosed()
    {
        Debug.Log("[DynamicTest] WebView closed");

        if (m_isAuthenticating)
        {
            m_isAuthenticating = false;

            if (m_authResult > 0)
            {
                _ = GetJWT(delay: 0.25f);
            }
        }
    }

    async Awaitable GetJWT(float delay)
    {
        var cancelToken = destroyCancellationToken;
        if (delay > 0)
        {
            await Awaitable.WaitForSecondsAsync(delay);
            if (cancelToken.IsCancellationRequested)
            {
                return;
            }
        }

        DynamicSDKManager.Instance.GetJwtToken();
    }
}

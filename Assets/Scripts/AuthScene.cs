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

    DynamicSDKManager m_sdk;
    DynamicSDKManifest m_manifest;
    bool m_isAuthenticating;
    int m_authResult = default;

    void Awake()
    {
        LogInButton.onClick.AddListener(ShowDynamicAuth);
    }

    void Start()
    {
        DynamicSDKManager.OnWalletConnected += OnWalletConnected;
        DynamicSDKManager.OnWalletDisconnected += OnWalletDisconnected;
        DynamicSDKManager.OnUserAuthenticated += OnUserAuthenticated;
        DynamicSDKManager.OnJwtTokenReceived += OnJwtTokenReceived;
        DynamicSDKManager.OnSDKError += OnSDKError;
        DynamicSDKManager.OnWebViewClosed += OnWebViewClosed;

        var config = DynamicSDKConfig.Instance;
        config.transitionDuration = 0.25f;
        config.enableClickOutsideToClose = false;
        config.enableWebViewPreload = false;

        m_sdk = DynamicSDKManager.Instance;
        ShowDynamicAuth();
    }

    /////////////////////////////////////////////////

    private void ShowDynamicAuth()
    {
        if (m_isAuthenticating)
        {
            return;
        }

        if (!m_sdk.IsInitialized)
        {
            m_sdk.InitializeSDK();
        }

        m_isAuthenticating = true;
        m_authResult = default;

        m_sdk.ConnectWallet();
    }

    //////////////////////////////////////////////////

    private void OnWalletConnected(string walletAddress)
    {
        Debug.Log($"[DynamicTest] Wallet connected: {walletAddress}");
        m_authResult = 1;
    }

    private void OnWalletDisconnected()
    {
        Debug.Log($"[DynamicTest] Wallet disconnected");
    }

    private void OnUserAuthenticated(UserInfo userInfo)
    {
        Debug.Log($"[DynamicTest] User authenticated: {userInfo.email}");
        m_authResult = 2;
    }

    private void OnJwtTokenReceived(JwtTokenResponseMessage jwtToken)
    {
        Debug.Log($"[DynamicTest] JWT token received");
        var jwt = jwtToken.data.token;

        Result.text = $"JWT token received:\n{jwt}";
    }

    private void OnSDKError(string error)
    {
        Debug.Log($"[DynamicTest] SDK Error: {error}");
        m_authResult = -1;
    }

    private void OnWebViewClosed()
    {
        Debug.Log("[DynamicTest] WebView closed");

        if (m_isAuthenticating)
        {
            m_isAuthenticating = false;

            if (m_authResult > 0)
            {
                DynamicSDKManager.Instance.GetJwtToken();
            }
        }
    }
}

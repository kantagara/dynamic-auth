using DynamicSDK.Unity.Core;
using DynamicSDK.Unity.Messages.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SampleScript : MonoBehaviour
{
    [Header("Input"), Space(10)]
    [SerializeField] private TMP_InputField messageToSignInput;

    [SerializeField] private TMP_InputField recipientAddressInput;
    [SerializeField] private TMP_InputField transactionAmountInput;
    [SerializeField] private TMP_Dropdown chainDropdown;

    [Header("Button"), Space(10)]
    [SerializeField] private Button connectButton;

    [SerializeField] private Button disconnectButton;
    [SerializeField] private Button signButton;
    [SerializeField] private Button sendTransactionButton;
    [SerializeField] private Button getJWTButton;
    [SerializeField] private Button openProfileButton;

    [Header("Text"), Space(10)]
    [SerializeField] private TMP_Text walletAddressText;

    [SerializeField] private TMP_Text transactionStatusText;
    [SerializeField] private TMP_Text messageSignedText;
    [SerializeField] private TMP_Text jwtTokenText;
    [SerializeField] private TMP_Text transactionHashText;
    [SerializeField] private TMP_Text statusText;

    private DynamicSDKManager sdk;

    private void Start()
    {
        sdk = DynamicSDKManager.Instance;
        SubscribeToEvents();
        SubcribeButtons();
        UpdateButtonInteractability();
    }

    private void SubscribeToEvents()
    {
        DynamicSDKManager.OnWalletConnected += OnWalletConnected;
        DynamicSDKManager.OnWalletDisconnected += OnWalletDisconnected;
        DynamicSDKManager.OnJwtTokenReceived += OnJwtTokenReceived;
        DynamicSDKManager.OnTransactionSent += OnTransactionSent;
        DynamicSDKManager.OnMessageSigned += OnMessageSigned;
        DynamicSDKManager.OnSDKError += OnSDKError;
        DynamicSDKManager.OnWebViewClosed += OnWebViewClosed;
    }

    private void SubcribeButtons()
    {
        connectButton.onClick.AddListener(OnConnectButtonClicked);
        disconnectButton.onClick.AddListener(OnDisconnectButtonClicked);
        signButton.onClick.AddListener(OnSignMessageButtonClicked);
        sendTransactionButton.onClick.AddListener(OnSendTransactionButtonClicked);
        getJWTButton.onClick.AddListener(OnGetJWTButtonClicked);
        openProfileButton.onClick.AddListener(OnOpenProfileButtonClicked);
    }

    #region Events

    private void OnWalletConnected(string walletAddress)
    {
        Debug.Log($"[SampleScript] Wallet connected: {walletAddress}");
        Debug.Log($"[SampleScript] Chain Selected: {GetSelectedNetwork()}");
        UpdateButtonInteractability();
        walletAddressText.text = "Wallet Connected: " + walletAddress;
        UpdateStatusText("Wallet connected");
    }

    private void OnWalletDisconnected()
    {
        Debug.Log($"[SampleScript] Wallet disconnected");
        UpdateButtonInteractability();
        walletAddressText.text = "Wallet Disconnected";
        UpdateStatusText("Wallet disconnected");
    }

    private void OnSDKError(string error)
    {
        Debug.Log($"[SampleScript] SDK Error: {error}");
        UpdateButtonInteractability();
        transactionStatusText.text = error;
        UpdateStatusText("SDK Error: " + error);
    }

    private void OnTransactionSent(string transactionHash)
    {
        Debug.Log($"[SampleScript] Transaction sent: {transactionHash}");
        UpdateButtonInteractability();
        transactionHashText.text = "Transaction Hash: " + transactionHash;
        UpdateStatusText("Transaction sent");
    }

    private void OnMessageSigned(string message)
    {
        Debug.Log($"[SampleScript] Message signed: {message}");
        UpdateButtonInteractability();
        messageSignedText.text = "Signed Message: " + message;
        UpdateStatusText("Message signed");
    }

    private void OnJwtTokenReceived(JwtTokenResponseMessage jwtToken)
    {
        Debug.Log($"[SampleScript] JWT token received: {jwtToken}");
        UpdateButtonInteractability();
        string token = jwtToken.data.token;
        string truncatedToken = token.Length > 20
            ? $"{token.Substring(0, 10)}...{token.Substring(token.Length - 10)}"
            : token;
        jwtTokenText.text = "JWT Token: " + truncatedToken;
        UpdateStatusText("JWT token received");
    }

    private void OnWebViewClosed()
    {
        Debug.Log("[SampleScript] WebView closed");
        UpdateButtonInteractability();
    }

    #endregion

    #region Button Events

    private void OnConnectButtonClicked()
    {
        Debug.Log("[SampleScript] Connect button clicked");
        Debug.Log($"[SampleScript] Chain Selected: {GetSelectedNetwork()}");
        if (sdk == null)
        {
            Debug.LogError("[SampleScript] SDK Manager not available!");
            UpdateStatusText("SDK Manager not available!");

            return;
        }

        if (!sdk.IsInitialized)
        {
            Debug.Log("[SampleScript] SDK not initialized, waiting...");
            UpdateStatusText("SDK not initialized, waiting...");

            return;
        }

        // Update UI
        connectButton.interactable = false;
        UpdateStatusText("Connecting wallet...");
        // Connect wallet
        sdk.ConnectWallet();
    }

    private void OnDisconnectButtonClicked()
    {
        Debug.Log("[SampleScript] Disconnect button clicked");

        if (sdk != null)
        {
            sdk.DisconnectWallet();
        }

        UpdateButtonInteractability();
        UpdateStatusText("Disconnecting wallet...");
    }

    public void OnSignMessageButtonClicked()
    {
        Debug.Log("[SampleScript] Sign message button clicked");

        if (sdk == null || !sdk.IsWalletConnected)
        {
            Debug.LogWarning("[SampleScript] No wallet connected");
            UpdateStatusText("No wallet connected");

            return;
        }

        string messageToSign = GetMessageToSign();

        if (string.IsNullOrEmpty(messageToSign))
        {
            Debug.LogWarning("[SampleScript] No message to sign");
            UpdateStatusText("No message to sign");

            return;
        }

        sdk.SignMessage(messageToSign);
        UpdateStatusText("Signing message...");
    }

    public void OnSendTransactionButtonClicked()
    {
        Debug.Log("[SampleScript] Send transaction button clicked");

        if (sdk == null || !sdk.IsWalletConnected)
        {
            Debug.LogWarning("[SampleScript] No wallet connected");
            UpdateStatusText("No wallet connected");

            return;
        }

        string recipientAddress = GetRecipientAddress();
        string transactionAmount = GetTransactionAmount();
        string selectedNetwork = "testnet"; //replace with your desired network

        // Validate inputs
        if (string.IsNullOrEmpty(recipientAddress))
        {
            Debug.LogWarning("[SampleScript] No recipient address");
            UpdateStatusText("No recipient address");

            return;
        }

        if (string.IsNullOrEmpty(transactionAmount))
        {
            Debug.LogWarning("[SampleScript] No transaction amount");
            UpdateStatusText("No transaction amount");

            return;
        }

        if (!IsValidAmount(transactionAmount))
        {
            Debug.LogWarning("[SampleScript] Invalid transaction amount");
            UpdateStatusText("Invalid transaction amount");

            return;
        }

        string amount = transactionAmount.Trim();

        if (string.IsNullOrEmpty(amount))
        {
            Debug.LogWarning("[SampleScript] Invalid transaction amount");
            UpdateStatusText("Invalid transaction amount");

            return;
        }

        sdk.SendTransaction(recipientAddress, amount, "", selectedNetwork);
        UpdateStatusText("Sending transaction...");
    }

    private void OnGetJWTButtonClicked()
    {
        Debug.Log("[SampleScript] Get JWT button clicked");

        if (sdk == null || !sdk.IsWalletConnected)
        {
            Debug.LogWarning("[SampleScript] No wallet connected");
            UpdateStatusText("No wallet connected");

            return;
        }

        sdk.GetJwtToken();
        UpdateStatusText("Getting JWT token...");
    }

    public void OnOpenProfileButtonClicked()
    {
        Debug.Log("[SampleScript] Open profile button clicked");

        if (sdk != null && sdk.IsWalletConnected)
        {
            sdk.OpenProfile();
            UpdateStatusText("Opening profile...");
        }
        else
        {
            Debug.LogWarning("[SampleScript] No wallet connected");
            UpdateStatusText("No wallet connected");
        }
    }

    #endregion

    #region Validation methods

    private bool IsValidAmount(string amount)
    {
        if (string.IsNullOrEmpty(amount))
            return false;

        if (decimal.TryParse(amount, out decimal value))
        {
            return value > 0 && value <= 100000; // Reasonable limits for SUI
        }

        return false;
    }

    #endregion

    #region Input field getters

    private string GetRecipientAddress() { return recipientAddressInput != null ? recipientAddressInput.text.Trim() : string.Empty; }
    private string GetMessageToSign() { return messageToSignInput != null ? messageToSignInput.text.Trim() : string.Empty; }
    private string GetTransactionAmount() { return transactionAmountInput != null ? transactionAmountInput.text.Trim() : string.Empty; }

    #endregion

    #region UI Update

    private void UpdateButtonInteractability()
    {
        //check if the wallet is connected
        if (sdk != null && sdk.IsWalletConnected)
        {
            connectButton.interactable = false;
            disconnectButton.interactable = true;
            signButton.interactable = true;
            sendTransactionButton.interactable = true;
            getJWTButton.interactable = true;
            openProfileButton.interactable = true;
        }
        else
        {
            connectButton.interactable = true;
            disconnectButton.interactable = false;
            signButton.interactable = false;
            sendTransactionButton.interactable = false;
            getJWTButton.interactable = false;
            openProfileButton.interactable = false;
        }
    }

    private void UpdateStatusText(string text) { statusText.text = text; }
    private string GetSelectedNetwork()
    {
        if (chainDropdown == null)
            return "SUI"; // fallback

        string selected = chainDropdown.options[chainDropdown.value].text;
        return selected;
    }
    #endregion
}
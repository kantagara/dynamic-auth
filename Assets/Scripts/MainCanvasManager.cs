using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DynamicSDK.Unity.Core;
using DynamicSDK.Unity.Messages;
using DynamicSDK.Unity.Messages.Wallet;
using TMPro;
using System;
using System.Collections;

public class MainCanvasManager : MonoBehaviour
{
    [Header("UI References")]

    [SerializeField]
    private TextMeshProUGUI walletAddressText;

    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private TextMeshProUGUI chainText;
    [SerializeField] private TextMeshProUGUI networkText;
    [SerializeField] private TextMeshProUGUI userInfoText;
    [SerializeField] private TextMeshProUGUI signatureText;
    [SerializeField] private TextMeshProUGUI transactionStatusText;
    [SerializeField] private Button disconnectButton;
    [SerializeField] private Button signMessageButton;
    [SerializeField] private Button getBalanceButton;
    [SerializeField] private Button sendTransactionButton;
    [SerializeField] private Button openProfileButton;
    [SerializeField]
    private TMP_Dropdown networksDropdown;

    [SerializeField] private TMP_Dropdown walletsDropdown;

    [Header("Input Fields")]
    [SerializeField]
    private TMP_InputField recipientAddressInput;

    [SerializeField] private TMP_InputField transactionAmountInput;
    [SerializeField] private TMP_InputField messageToSignInput;

    [Header("Scene Settings")]
    [SerializeField]
    private string loginSceneName = "LoginCanvas";

    [Header("Default Values")]
    [SerializeField]
    private string defaultMessage = "Hello from MainCanvas!";

    [SerializeField] private string defaultRecipientAddress = "0xc2ba73f17d17cebdd79dfa9e8c2efe35c9ade2fd6ce02adb38c61bd1e24e1180";
    [SerializeField] private string defaultTransactionValue = "0.001"; // 0.001 ETH

    private DynamicSDKManager sdkManager;

    // Store wallet and network data for dropdown switching
    private WalletCredential[] currentWallets;
    private NetworkInfo[] currentNetworks;

    void Start()
    {
        // Get SDK Manager instance
        sdkManager = DynamicSDKManager.Instance;

        // Setup UI
        SetupUI();

        // Subscribe to events
        SubscribeToEvents();

        // Check wallet connection status
        CheckWalletConnection();
    }

    private void SetupUI()
    {
        // Setup button listeners
        if (disconnectButton != null)
            disconnectButton.onClick.AddListener(OnDisconnectButtonClicked);

        if (signMessageButton != null)
            signMessageButton.onClick.AddListener(OnSignMessageButtonClicked);

        if (getBalanceButton != null)
            getBalanceButton.onClick.AddListener(OnGetBalanceButtonClicked);

        if (sendTransactionButton != null)
            sendTransactionButton.onClick.AddListener(OnSendTransactionButtonClicked);

        if (openProfileButton != null)
            openProfileButton.onClick.AddListener(OnOpenProfileButtonClicked);

        // Setup network dropdown
        if (networksDropdown != null)
            networksDropdown.onValueChanged.AddListener(OnNetworksDropdownValueChanged);


        if (walletsDropdown != null)
            walletsDropdown.onValueChanged.AddListener(OnWalletsDropdownValueChanged);

        // Setup default values for input fields
        SetupDefaultInputValues();
    }



    private void SetupDefaultInputValues()
    {
        if (recipientAddressInput != null)
        {
            recipientAddressInput.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter recipient address...";
            recipientAddressInput.text = defaultRecipientAddress;
        }

        if (transactionAmountInput != null)
        {
            transactionAmountInput.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter amount...";
            transactionAmountInput.text = defaultTransactionValue;
        }

        if (messageToSignInput != null)
        {
            messageToSignInput.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter message to sign...";
            messageToSignInput.text = defaultMessage;
        }
    }

    private void SubscribeToEvents()
    {
        // Subscribe to wallet events
        DynamicSDKManager.OnUserAuthenticated += OnUserAuthenticated;
        DynamicSDKManager.OnWalletInfoUpdated += OnWalletInfoUpdated;
        DynamicSDKManager.OnWalletDisconnected += OnWalletDisconnected;
        DynamicSDKManager.OnTransactionSent += OnTransactionSent;
        DynamicSDKManager.OnMessageSigned += OnMessageSigned;
        DynamicSDKManager.OnBalanceUpdated += OnBalanceUpdated;
        DynamicSDKManager.OnWalletSwitched += OnWalletSwitched;
        DynamicSDKManager.OnNetworkSwitched += OnNetworkSwitched;
        DynamicSDKManager.OnWalletsReceived += OnWalletsReceived;
        DynamicSDKManager.OnNetworksReceived += OnNetworksReceived;
        DynamicSDKManager.OnSDKError += OnSDKError;
    }

    private void CheckWalletConnection()
    {
        if (sdkManager == null)
        {
            Debug.LogError("[MainCanvasManager] SDK Manager not available!");
            ReturnToLogin();

            return;
        }

        if (!sdkManager.IsInitialized)
        {
            Debug.LogError("[MainCanvasManager] SDK not initialized!");
            ReturnToLogin();

            return;
        }

        if (!sdkManager.IsWalletConnected)
        {
            Debug.LogWarning("[MainCanvasManager] No wallet connected, returning to login");
            ReturnToLogin();

            return;
        }

        // Wallet is connected, update UI
        string walletAddress = sdkManager.CurrentWalletAddress;
        UpdateWalletInfo(walletAddress);

        // Update wallet chain/network info if available
        if (sdkManager.CurrentWalletInfo != null)
        {
            UpdateChainNetworkInfo(sdkManager.CurrentWalletInfo);
        }
        else
        {
            // If wallet is connected but we don't have wallet info, set default values
            UpdateChainNetworkInfo(null);
        }

        // Update user info if available
        if (sdkManager.CurrentUserInfo != null)
        {
            UpdateUserInfo(sdkManager.CurrentUserInfo);
        }
        else
        {
            UpdateUserInfo(walletAddress); // Fallback to wallet address
        }

        // Clear previous signature
        ClearSignatureInfo();

        // Automatically get balance (this will also potentially update chain/network info)
        sdkManager.GetWallets();
    }

    #region Button Handlers

    public void OnDisconnectButtonClicked()
    {
        Debug.Log("[MainCanvasManager] Disconnect button clicked");

        if (sdkManager != null)
        {
            sdkManager.DisconnectWallet();
        }
    }

    public void OnSignMessageButtonClicked()
    {
        Debug.Log("[MainCanvasManager] Sign message button clicked");

        if (sdkManager == null || !sdkManager.IsWalletConnected)
        {
            Debug.LogWarning("[MainCanvasManager] No wallet connected");
            UpdateTransactionStatus("Error: No wallet connected", true);

            return;
        }

        string messageToSign = GetMessageToSign();

        if (string.IsNullOrEmpty(messageToSign))
        {
            Debug.LogWarning("[MainCanvasManager] No message to sign");
            UpdateTransactionStatus("Error: Please enter a message to sign", true);

            return;
        }

        UpdateTransactionStatus("Signing message...", false);
        sdkManager.SignMessage(messageToSign);
    }

    public void OnGetBalanceButtonClicked()
    {
        Debug.Log("[MainCanvasManager] Get balance button clicked");

        if (sdkManager != null && sdkManager.IsWalletConnected)
        {
            sdkManager.GetBalance();
        }
        else
        {
            Debug.LogWarning("[MainCanvasManager] No wallet connected");
        }
    }

    public void OnSendTransactionButtonClicked()
    {
        Debug.Log("[MainCanvasManager] Send transaction button clicked");

        if (sdkManager == null || !sdkManager.IsWalletConnected)
        {
            Debug.LogWarning("[MainCanvasManager] No wallet connected");
            UpdateTransactionStatus("Error: No wallet connected", true);

            return;
        }

        string recipientAddress = GetRecipientAddress();
        string transactionAmount = GetTransactionAmount();

        // Get selected network from dropdown, or fallback to current wallet network
        string selectedNetwork = "mainnet"; // Default fallback
        if (networksDropdown != null)
        {
            selectedNetwork = networksDropdown.options[networksDropdown.value].text.ToLower();
        }
        else if (sdkManager?.CurrentWalletInfo != null && !string.IsNullOrEmpty(sdkManager.CurrentWalletInfo.network))
        {
            selectedNetwork = sdkManager.CurrentWalletInfo.network.ToLower();
        }

        // Validate inputs
        if (string.IsNullOrEmpty(recipientAddress))
        {
            UpdateTransactionStatus("Error: Please enter a recipient address", true);

            return;
        }

        if (!IsValidEthereumAddress(recipientAddress))
        {
            UpdateTransactionStatus("Error: Invalid recipient address format", true);

            return;
        }

        if (string.IsNullOrEmpty(transactionAmount))
        {
            UpdateTransactionStatus("Error: Please enter an amount", true);

            return;
        }

        if (!IsValidAmount(transactionAmount))
        {
            UpdateTransactionStatus("Error: Invalid amount format", true);

            return;
        }

        // Convert ETH to Wei for the transaction
        string amount = transactionAmount.Trim();

        if (string.IsNullOrEmpty(amount))
        {
            UpdateTransactionStatus("Error: Failed to convert amount", true);

            return;
        }

        UpdateTransactionStatus($"Sending {transactionAmount} SUI to {FormatAddress(recipientAddress)}...", false);
        sdkManager.SendTransaction(recipientAddress, amount, "", selectedNetwork);
    }

    public void OnOpenProfileButtonClicked()
    {
        Debug.Log("[MainCanvasManager] Open profile button clicked");

        if (sdkManager != null && sdkManager.IsWalletConnected)
        {
            sdkManager.OpenProfile();
        }
        else
        {
            Debug.LogWarning("[MainCanvasManager] No wallet connected");
        }
    }

    #endregion

    #region Event Handlers

    private void OnWalletDisconnected()
    {
        Debug.Log("[MainCanvasManager] Wallet disconnected");

        // Clear UI information before returning to login
        ClearAllUserInfo();

        // Return to login scene immediately
        ReturnToLogin();
    }

    private void OnTransactionSent(string transactionHash)
    {
        Debug.Log($"[MainCanvasManager] Transaction sent: {transactionHash}");
        UpdateTransactionStatus($"✅ Transaction sent! Hash: {FormatHash(transactionHash)}", false);
    }

    private void OnMessageSigned(string signature)
    {
        Debug.Log($"[MainCanvasManager] Message signed: {signature}");

        // Update signature display
        UpdateSignatureInfo(signature);

        // Update transaction status for successful signing
        UpdateTransactionStatus("Message signed successfully!", false);
    }

    private void OnBalanceUpdated(BalanceResponseData balance)
    {
        Debug.Log($"[MainCanvasManager] Balance updated: {balance}");
        UpdateBalanceText(balance.balance);
        UpdateNetworkText(balance.network);
    }
    private void OnWalletSwitched(BalanceResponseData balance)
    {
        // Refresh networks after wallet switch
        sdkManager?.GetNetworks();
        sdkManager?.GetBalance();
        Debug.Log($"[MainCanvasManager] Wallet switched: {balance}");
        UpdateBalanceText(balance.balance);
        UpdateNetworkText(balance.network);

    }
    private void OnNetworkSwitched(BalanceResponseData balance)
    {
        Debug.Log($"[MainCanvasManager] Network switched: {balance}");
        UpdateBalanceText(balance.balance);
        UpdateNetworkText(balance.network);
    }

    private void OnSDKError(string error)
    {
        Debug.LogError($"[MainCanvasManager] SDK error: {error}");

        // Provide more specific error messages for signing operations  
        string errorMessage = GetFriendlyErrorMessage(error);
        UpdateTransactionStatus($"Error: {errorMessage}", true);
    }

    private void OnUserAuthenticated(UserInfo userInfo)
    {
        Debug.Log($"[MainCanvasManager] User authenticated: {userInfo?.name}");
        UpdateUserInfo(userInfo);
    }

    private void OnWalletInfoUpdated(WalletCredential walletInfo)
    {
        Debug.Log($"[MainCanvasManager] Wallet info updated: {walletInfo}");
        UpdateChainNetworkInfo(walletInfo);

        // Update input placeholders based on chain
        UpdateInputPlaceholdersForChain(walletInfo?.chain);
    }

    private void OnWalletsReceived(WalletsResponseData walletsData)
    {
        Debug.Log($"[MainCanvasManager] Wallets received: {walletsData?.wallets?.Length ?? 0} wallets");

        // Store wallets data for dropdown switching
        currentWallets = walletsData?.wallets;

        // Here you can update UI elements with the wallets data
        try
        {
            if (walletsData != null && walletsData.wallets != null && walletsData.wallets.Length > 0)
            {
                // Populate the wallets dropdown
                UpdateWalletsDropdown(walletsData.wallets);

                Debug.Log($"[MainCanvasManager] Primary wallet: {walletsData.primaryWallet?.address}");
            }
            else
            {
                // Clear the dropdown if no wallets
                if (walletsDropdown != null)
                {
                    walletsDropdown.ClearOptions();
                    walletsDropdown.AddOptions(new System.Collections.Generic.List<string> { "No wallets available" });
                    walletsDropdown.interactable = false;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[MainCanvasManager] Failed to process wallets data: {ex.Message}");
        }
    }

    private void OnNetworksReceived(NetworksResponseData networksData)
    {
        Debug.Log($"[MainCanvasManager] Networks received: {networksData?.networks?.Length ?? 0} networks");

        // Store networks data for dropdown switching
        currentNetworks = networksData?.networks;

        // Here you can update UI elements with the networks data
        try
        {
            if (networksData != null && networksData.networks != null && networksData.networks.Length > 0)
            {
                // Populate the networks dropdown
                UpdateNetworksDropdown(networksData.networks);
            }
            else
            {
                // Clear the dropdown if no networks
                if (networksDropdown != null)
                {
                    networksDropdown.ClearOptions();
                    networksDropdown.AddOptions(new System.Collections.Generic.List<string> { "No networks available" });
                    networksDropdown.interactable = false;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[MainCanvasManager] Failed to process networks data: {ex.Message}");
        }
    }

    #endregion

    #region Scene Management

    private void ReturnToLogin()
    {
        try
        {
            Debug.Log($"[MainCanvasManager] Returning to login scene: {loginSceneName}");
            SceneManager.LoadScene(loginSceneName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[MainCanvasManager] Failed to load login scene '{loginSceneName}': {ex.Message}");
        }
    }

    #endregion

    #region UI Helpers

    private void UpdateWalletInfo(string address)
    {
        if (walletAddressText != null)
        {
            walletAddressText.text = $"{FormatAddress(address)}";
        }
    }

    private void UpdateBalanceText(string balance)
    {
        if (balanceText != null)
        {
            balanceText.text = $"{balance}";
        }
    }

    private void UpdateNetworkText(string network)
    {
        if (networkText != null)
        {
            networkText.text = $"{network}";
        }
    }

    private void UpdateChainNetworkInfo(WalletCredential walletInfo)
    {
        Debug.Log($"[MainCanvasManager] Updating chain/network info: {walletInfo?.chain}, {walletInfo?.network}");
        if (walletInfo != null)
        {
            // Update chain info
            if (chainText != null && !string.IsNullOrEmpty(walletInfo.chain))
            {
                chainText.text = GetDisplayNameForChain(walletInfo.chain);
            }
            else if (chainText == null)
            {
                chainText.text = "Connected";
            }

            // Update network info
            if (networkText != null && !string.IsNullOrEmpty(walletInfo.network))
            {
                networkText.text = walletInfo.network;
            }
            else if (networkText == null)
            {
                networkText.text = "Connected";
            }
        }
        else
        {
            // Wallet is connected but no detailed info available
            if (chainText != null)
            {
                chainText.text = "Connected";
            }

            if (networkText != null)
            {
                networkText.text = "Connected";
            }
        }
    }

    private void UpdateInputPlaceholdersForChain(string chain)
    {
        if (transactionAmountInput != null)
        {
            string symbol = GetSymbolForChain(chain);
            transactionAmountInput.placeholder.GetComponent<TextMeshProUGUI>().text = $"Enter amount in {symbol}...";
        }
    }

    private string GetSymbolForChain(string chain)
    {
        if (string.IsNullOrEmpty(chain))
            return "TOKEN";

        switch (chain.ToLower())
        {
            case "sui":
                return "SUI";
            case "ethereum":
            case "eth":
            case "evm":
                return "ETH";
            case "solana":
            case "sol":
                return "SOL";
            case "polygon":
            case "matic":
                return "MATIC";
            case "binance":
            case "bsc":
            case "bnb":
                return "BNB";
            case "avalanche":
            case "avax":
                return "AVAX";
            default:
                return "TOKEN";
        }
    }

    private string GetDisplayNameForChain(string chain)
    {
        if (string.IsNullOrEmpty(chain))
            return "Unknown";

        switch (chain.ToLower())
        {
            case "sui":
                return "SUI";
            case "ethereum":
            case "eth":
            case "evm":
                return "Ethereum";
            case "solana":
            case "sol":
                return "Solana";
            case "polygon":
            case "matic":
                return "Polygon";
            case "binance":
            case "bsc":
            case "bnb":
                return "Binance";
            case "avalanche":
            case "avax":
                return "Avalanche";
            case "arbitrum":
                return "Arbitrum";
            case "optimism":
                return "Optimism";
            case "base":
                return "Base";
            default:
                // Return capitalized version of the original chain name
                return char.ToUpper(chain[0]) + chain.Substring(1).ToLower();
        }
    }

    private void UpdateUserInfo(UserInfo userInfo)
    {
        if (userInfoText != null && userInfo != null)
        {
            var infoLines = new System.Collections.Generic.List<string>();

            // Add user ID if available
            if (!string.IsNullOrEmpty(userInfo.userId))
            {
                infoLines.Add($"User ID: {userInfo.userId}");
            }

            // Add email if available
            if (!string.IsNullOrEmpty(userInfo.email))
            {
                infoLines.Add($"Email: {userInfo.email}");
            }

            // Add wallet address if available
            if (!string.IsNullOrEmpty(sdkManager.CurrentWalletAddress))
            {
                infoLines.Add($"Address: {sdkManager.CurrentWalletAddress}");
            }

            userInfoText.text = string.Join("\n", infoLines);
        }
    }

    private void UpdateUserInfo(string walletAddress)
    {
        if (userInfoText != null)
        {
            userInfoText.text = $"User: {FormatAddress(walletAddress)}\nConnected at: {System.DateTime.Now:HH:mm:ss}";
        }
    }

    private void UpdateSignatureInfo(string signature)
    {
        if (signatureText != null)
        {
            string message = GetMessageToSign();
            signatureText.text = $"Message: \"{message}\"\nSignature: {(signature)}\nSigned at: {System.DateTime.Now:HH:mm:ss}";
        }
    }

    private void ClearSignatureInfo()
    {
        if (signatureText != null)
        {
            signatureText.text = "No message signed yet";
        }
    }

    private void ClearTransactionStatus()
    {
        if (transactionStatusText != null)
        {
            transactionStatusText.text = "Ready for operations";
            transactionStatusText.color = Color.white;
        }
    }

    private void ClearAllUserInfo()
    {
        if (userInfoText != null)
        {
            userInfoText.text = "No user connected";
        }

        if (signatureText != null)
        {
            signatureText.text = "No message signed yet";
        }

        ClearTransactionStatus();

        if (walletAddressText != null)
        {
            walletAddressText.text = "Not connected";
        }

        if (balanceText != null)
        {
            balanceText.text = "0 TOKEN";
        }

        if (chainText != null)
        {
            chainText.text = "No chain";
        }

        if (networkText != null)
        {
            networkText.text = "No network";
        }

        // Reset input placeholders to generic
        if (transactionAmountInput != null)
        {
            transactionAmountInput.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter amount...";
        }
    }

    private string FormatAddress(string address)
    {
        if (string.IsNullOrEmpty(address) || address.Length < 10)
            return address;

        return $"{address.Substring(0, 6)}...{address.Substring(address.Length - 4)}";
    }

    private string FormatHash(string hash)
    {
        if (string.IsNullOrEmpty(hash) || hash.Length < 10)
            return hash;

        return $"{hash.Substring(0, 8)}...{hash.Substring(hash.Length - 6)}";
    }

    // Input field getters
    private string GetRecipientAddress() { return recipientAddressInput != null ? recipientAddressInput.text.Trim() : string.Empty; }

    private string GetTransactionAmount() { return transactionAmountInput != null ? transactionAmountInput.text.Trim() : string.Empty; }

    private string GetMessageToSign() { return messageToSignInput != null ? messageToSignInput.text.Trim() : string.Empty; }

    // Validation methods
    private bool IsValidEthereumAddress(string address)
    {
        if (string.IsNullOrEmpty(address))
            return false;

        // Remove 0x prefix if present
        if (address.StartsWith("0x") || address.StartsWith("0X"))
            address = address.Substring(2);

        // Check length (64 characters for Sui address)
        if (address.Length != 64)
            return false;

        // Check if all characters are valid hex
        return System.Text.RegularExpressions.Regex.IsMatch(address, @"^[0-9a-fA-F]{64}$");
    }

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

    // Utility methods
    private string ConvertEthToWei(string ethAmount)
    {
        try
        {
            if (decimal.TryParse(ethAmount, out decimal suiValue))
            {
                // Convert SUI to MIST (1 SUI = 10^9 MIST)
                decimal mistValue = suiValue * 1000000000m; // 10^9

                return mistValue.ToString("F0"); // No decimal places for mist
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[MainCanvasManager] Failed to convert SUI to MIST: {ex.Message}");
        }

        return string.Empty;
    }

    private void UpdateTransactionStatus(string message, bool isError)
    {
        if (transactionStatusText != null)
        {
            transactionStatusText.text = message;
            transactionStatusText.color = isError ? Color.red : Color.green;
        }

        Debug.Log($"[MainCanvasManager] Transaction Status: {message}");
    }

    private string GetFriendlyErrorMessage(string error)
    {
        if (string.IsNullOrEmpty(error))
            return "Unknown error occurred";

        // Check for specific error codes and return user-friendly messages
        if (error.Contains(ErrorCodes.AUTH_USER_REJECTED) || error.Contains(ErrorCodes.WALLET_USER_REJECTED))
        {
            return "User cancelled the signing operation";
        }
        else if (error.Contains(ErrorCodes.WALLET_SIGNATURE_FAILED))
        {
            return "Failed to sign message. Please try again";
        }
        else if (error.Contains(ErrorCodes.WALLET_NOT_CONNECTED))
        {
            return "Wallet not connected. Please connect your wallet first";
        }
        else if (error.Contains(ErrorCodes.AUTH_NETWORK_ERROR) || error.Contains(ErrorCodes.WALLET_NETWORK_ERROR))
        {
            return "Network connection error. Please check your internet connection";
        }
        else if (error.Contains(ErrorCodes.AUTH_SESSION_EXPIRED))
        {
            return "Session expired. Please reconnect your wallet";
        }
        else if (error.Contains(ErrorCodes.WALLET_INSUFFICIENT_FUNDS))
        {
            return "Insufficient funds to complete the operation";
        }
        else if (error.Contains(ErrorCodes.WALLET_INVALID_ADDRESS))
        {
            return "Invalid wallet address provided";
        }
        else if (error.Contains(ErrorCodes.WALLET_INVALID_CHAIN))
        {
            return "Invalid blockchain network";
        }
        else if (error.Contains(ErrorCodes.WALLET_TRANSACTION_FAILED))
        {
            return "Transaction failed. Please try again";
        }
        else if (error.Contains(ErrorCodes.WALLET_GAS_BUDGET_ERROR))
        {
            return "Gas budget error. Please check gas settings";
        }
        else if (error.Contains(ErrorCodes.WALLET_INVALID_TRANSACTION_DATA))
        {
            return "Invalid transaction data provided";
        }
        else if (error.Contains(ErrorCodes.WALLET_OBJECT_NOT_FOUND))
        {
            return "Required object not found";
        }
        else if (error.Contains(ErrorCodes.AUTH_INVALID_EMAIL_DOMAIN))
        {
            return "Invalid email domain for authentication";
        }
        else if (error.ToLower().Contains("timeout"))
        {
            return "Operation timed out. Please try again";
        }
        else if (error.ToLower().Contains("cancelled"))
        {
            return "Operation was cancelled";
        }
        else
        {
            // For unknown errors, return a generic but helpful message
            return $"Error: {error}";
        }
    }

    private void OnNetworksDropdownValueChanged(int value)
    {
        // Ignore if no valid selection
        if (value < 0 || networksDropdown.options.Count <= value)
            return;

        string selectedNetwork = networksDropdown.options[value].text.ToLower();
        Debug.Log($"[MainCanvasManager] Network changed to: {selectedNetwork}");

        // Update network text
        if (networkText != null)
        {
            networkText.text = selectedNetwork;
        }

        // Get the actual network chain ID and switch
        if (currentNetworks != null && value < currentNetworks.Length)
        {
            var selectedNetworkInfo = currentNetworks[value];
            string chainId = selectedNetworkInfo.chainId ?? selectedNetworkInfo.networkId;

            if (!string.IsNullOrEmpty(chainId))
            {
                Debug.Log($"[MainCanvasManager] Switching to network with chain ID: {chainId}");
                sdkManager?.SwitchNetwork(chainId);
            }
        }
    }

    private void OnWalletsDropdownValueChanged(int value)
    {
        if (walletsDropdown != null && walletsDropdown.options.Count > value && value >= 0)
        {
            string selectedWallet = walletsDropdown.options[value].text;
            Debug.Log($"[MainCanvasManager] Wallet changed to: {selectedWallet}");

            // Get the actual wallet ID and switch
            if (currentWallets != null && value < currentWallets.Length)
            {
                var selectedWalletInfo = currentWallets[value];
                string walletId = selectedWalletInfo.id;

                if (!string.IsNullOrEmpty(walletId))
                {
                    Debug.Log($"[MainCanvasManager] Switching to wallet with ID: {walletId}");
                    sdkManager?.SwitchWallet(walletId);


                }
            }
        }
    }

    private void UpdateWalletsDropdown(WalletCredential[] wallets)
    {
        if (walletsDropdown == null || wallets == null || wallets.Length == 0)
            return;
        walletsDropdown.ClearOptions();
        // Clear existing options
        walletsDropdown.ClearOptions();

        // Create list of wallet options
        var walletOptions = new System.Collections.Generic.List<string>();

        foreach (var wallet in wallets)
        {
            if (wallet != null && !string.IsNullOrEmpty(wallet.address))
            {
                // Format: "WalletName - Chain (0x1234...5678)"
                string chain = !string.IsNullOrEmpty(wallet.chain) ? wallet.chain : "Unknown";
                string id = wallet.id;

                string optionText = $"{chain}-{id}";
                walletOptions.Add(optionText);
            }
        }

        if (walletOptions.Count > 0)
        {
            // Add options to dropdown
            walletsDropdown.AddOptions(walletOptions);
            walletsDropdown.interactable = true;
            walletsDropdown.value = -1;
            Debug.Log($"[MainCanvasManager] Populated wallets dropdown with {walletOptions.Count} options");
        }
        else
        {
            // No valid wallets found
            walletsDropdown.AddOptions(new System.Collections.Generic.List<string> { "No valid wallets found" });
            walletsDropdown.interactable = false;
        }
    }

    private void UpdateNetworksDropdown(NetworkInfo[] networks)
    {
        if (networksDropdown == null || networks == null || networks.Length == 0)
            return;

        // Clear existing options
        networksDropdown.ClearOptions();

        // Create list of network options
        var networkOptions = new System.Collections.Generic.List<string>();

        foreach (var network in networks)
        {
            if (network != null && !string.IsNullOrEmpty(network.name))
            {
                // Format: "NetworkName (Symbol)" or "VanityName - NetworkName (Symbol)"
                string displayName = !string.IsNullOrEmpty(network.vanityName) ? network.vanityName : network.name;
                string chainId = network.chainId ?? network.networkId ?? "N/A";
                string optionText = $"{displayName}-{chainId}";
                networkOptions.Add(optionText);
            }
        }

        if (networkOptions.Count > 0)
        {
            // Add options to dropdown
            networksDropdown.AddOptions(networkOptions);
            networksDropdown.interactable = true;

            Debug.Log($"[MainCanvasManager] Populated networks dropdown with {networkOptions.Count} options");
        }
        else
        {
            // No valid networks found
            networksDropdown.AddOptions(new System.Collections.Generic.List<string> { "No valid networks found" });
            networksDropdown.interactable = false;
        }
    }

    #endregion

    #region Unity Lifecycle

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        DynamicSDKManager.OnUserAuthenticated -= OnUserAuthenticated;
        DynamicSDKManager.OnWalletInfoUpdated -= OnWalletInfoUpdated;
        DynamicSDKManager.OnWalletDisconnected -= OnWalletDisconnected;
        DynamicSDKManager.OnTransactionSent -= OnTransactionSent;
        DynamicSDKManager.OnMessageSigned -= OnMessageSigned;
        DynamicSDKManager.OnBalanceUpdated -= OnBalanceUpdated;
        DynamicSDKManager.OnWalletSwitched -= OnWalletSwitched;
        DynamicSDKManager.OnNetworkSwitched -= OnNetworkSwitched;
        DynamicSDKManager.OnWalletsReceived -= OnWalletsReceived;
        DynamicSDKManager.OnNetworksReceived -= OnNetworksReceived;
        DynamicSDKManager.OnSDKError -= OnSDKError;
    }

    #endregion
}
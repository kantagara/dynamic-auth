using System;
using UnityEngine;

namespace DynamicSDK.Unity.Messages.Wallet
{
    // ============================================================================
    // BALANCE RESPONSE
    // ============================================================================
    
    [Serializable]
    public class BalanceResponseData
    {
        public string walletAddress;
        public string chain;
        public string balance;
        public string symbol;
        public int decimals;
        public string tokenAddress;
        public float usdValue;
        public bool success;
        public string error;
    }

    [Serializable]
    public class BalanceResponseMessage : BaseMessage
    {
        public BalanceResponseData data;

        public BalanceResponseMessage()
        {
            type = "wallet";
            action = WalletActions.BALANCE_RESPONSE;
        }
    }

    // ============================================================================
    // SIGN MESSAGE RESPONSE
    // ============================================================================
    
    [Serializable]
    public class SignMessageResponseData
    {
        public string walletAddress;
        public string signature;
        public string message;
        public bool success;
        public string error;
    }

    [Serializable]
    public class SignMessageResponseMessage : BaseMessage
    {
        public SignMessageResponseData data;

        public SignMessageResponseMessage()
        {
            type = "wallet";
            action = WalletActions.SIGN_MESSAGE_RESPONSE;
        }
    }

    // ============================================================================
    // TRANSACTION RESPONSE
    // ============================================================================
    
    [Serializable]
    public class TransactionResponseData
    {
        public string walletAddress;
        public string transactionHash;
        public bool success;
        public string error;
        public string gasUsed;
        public long blockNumber;
        public int confirmations;
    }

    [Serializable]
    public class TransactionResponseMessage : BaseMessage
    {
        public TransactionResponseData data;

        public TransactionResponseMessage()
        {
            type = "wallet";
            action = WalletActions.TRANSACTION_RESPONSE;
        }
    }

    // ============================================================================
    // WALLET CONNECTION MESSAGES
    // ============================================================================
    
    [Serializable]
    public class WalletConnectedData
    {
        public WalletCredential wallet;
        public string walletAddress;
        public string chain;
        public bool success;
    }

    [Serializable]
    public class WalletConnectedMessage : BaseMessage
    {
        public WalletConnectedData data;

        public WalletConnectedMessage()
        {
            type = "wallet";
            action = WalletActions.WALLET_CONNECTED;
        }
    }

    [Serializable]
    public class WalletDisconnectedData
    {
        public string walletAddress;
        public string reason;
        public bool success;
    }

    [Serializable]
    public class WalletDisconnectedMessage : BaseMessage
    {
        public WalletDisconnectedData data;

        public WalletDisconnectedMessage()
        {
            type = "wallet";
            action = WalletActions.WALLET_DISCONNECTED;
        }
    }

    // ============================================================================
    // WALLET ERROR
    // ============================================================================
    
    [Serializable]
    public class WalletErrorData : ErrorInfo
    {
        public string walletAddress;
        public string action;
    }

    [Serializable]
    public class WalletErrorMessage : BaseMessage
    {
        public WalletErrorData data;

        public WalletErrorMessage()
        {
            type = "wallet";
            action = WalletActions.WALLET_ERROR;
        }
    }
} 
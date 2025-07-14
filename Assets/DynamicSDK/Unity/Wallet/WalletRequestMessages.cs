using System;
using UnityEngine;

namespace DynamicSDK.Unity.Messages.Wallet
{
    // ============================================================================
    // GET BALANCE REQUEST
    // ============================================================================

    [Serializable]
    public class GetBalanceData
    {
        public string walletAddress;
        public string chain;
        public string tokenAddress;
        public string network;
    }

    [Serializable]
    public class GetBalanceMessage : BaseMessage
    {
        public GetBalanceData data;

        public GetBalanceMessage()
        {
            type   = "wallet";
            action = WalletActions.GET_BALANCE;
        }
    }

    // ============================================================================
    // SIGN MESSAGE REQUEST
    // ============================================================================

    [Serializable]
    public class SignMessageData
    {
        public string walletAddress;
        public string message;
    }

    [Serializable]
    public class SignMessageMessage : BaseMessage
    {
        public SignMessageData data;

        public SignMessageMessage()
        {
            type   = "wallet";
            action = WalletActions.SIGN_MESSAGE;
        }
    }

    // ============================================================================
    // TRANSACTION REQUEST
    // ============================================================================

    [Serializable]
    public class TransactionData
    {
        public string walletAddress;
        public string to;
        public string value;
        public string data;
        public string gasLimit;
        public string gasPrice;
        public string maxFeePerGas;
        public string maxPriorityFeePerGas;
        public string chain;
        public string network;
        public string type;
    }

    [Serializable]
    public class TransactionMessage : BaseMessage
    {
        public TransactionData data;

        public TransactionMessage()
        {
            type   = "wallet";
            action = WalletActions.TRANSACTION;
        }
    }

    // ============================================================================
    // OPEN PROFILE REQUEST
    // ============================================================================

    [Serializable]
    public class OpenProfileData
    {
        public string walletAddress;
        public string network;
    }

    [Serializable]
    public class OpenProfileMessage : BaseMessage
    {
        public OpenProfileData data;

        public OpenProfileMessage()
        {
            type   = "wallet";
            action = WalletActions.OPEN_PROFILE;
        }
    }
}
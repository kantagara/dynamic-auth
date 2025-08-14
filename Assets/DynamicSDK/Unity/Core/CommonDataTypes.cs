using System;
using UnityEngine;

namespace DynamicSDK.Unity.Messages
{
    /// <summary>
    /// User information data structure
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        public string userId;
        public string email;
        public string name;
        public string avatar;
        public bool isVerified;
    }



    /// <summary>
    /// Wallet credential data structure
    /// </summary>
    [Serializable]
    public class WalletCredential
    {
        public string address;
        public string walletName;
        public string chain;
        public string format;
        public string id;
        public string network;
        public string balance;
        public int decimals;
        public string symbol;

        public override string ToString()
        {
            return $"WalletCredential {{ Address: {address}, Name: {walletName}, Chain: {chain}, Format: {format}, ID: {id}, Network: {network}, Balance: {balance}, Decimals: {decimals}, Symbol: {symbol} }}";
        }
    }

    /// <summary>
    /// Error information data structure
    /// </summary>
    [Serializable]
    public class ErrorInfo
    {
        public string error;
        public string errorCode;
        public string reason;
    }
}
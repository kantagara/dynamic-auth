using Newtonsoft.Json;
using DynamicSDK.Unity.Messages;
using DynamicSDK.Unity.Messages.Auth;
using DynamicSDK.Unity.Messages.Wallet;
using System;

namespace DynamicSDK.Unity.Utils
{
    // ============================================================================
    // REQUEST MESSAGE CLASSES - MANAGED CODE STRIPPING SAFE
    // ============================================================================

    /// <summary>
    /// Base request data class
    /// </summary>
    [Serializable]
    public class RequestData
    {
        // Empty base class for requests that don't need data
    }

    /// <summary>
    /// Generic request message class
    /// </summary>
    [Serializable]
    public class RequestMessage : BaseMessage
    {
        public RequestData data;

        public RequestMessage(string messageType, string messageAction)
        {
            type   = messageType;
            action = messageAction;
            data   = new RequestData();
        }
    }

    /// <summary>
    /// Sign message request data
    /// </summary>
    [Serializable]
    public class SignMessageRequestData : RequestData
    {
        public string walletAddress;
        public string message;
    }

    /// <summary>
    /// Sign message request message
    /// </summary>
    [Serializable]
    public class SignMessageRequestMessage : BaseMessage
    {
        public SignMessageRequestData data;

        public SignMessageRequestMessage(string walletAddress, string message)
        {
            type   = "wallet";
            action = "signMessage";

            data = new SignMessageRequestData
            {
                walletAddress = walletAddress,
                message       = message
            };
        }
    }

    /// <summary>
    /// Transaction request data
    /// </summary>
    [Serializable]
    public class TransactionRequestData : RequestData
    {
        public string walletAddress;
        public string to;
        public string value;
        public string chain;
        public string network;
    }

    /// <summary>
    /// Transaction request message
    /// </summary>
    [Serializable]
    public class TransactionRequestMessage : BaseMessage
    {
        public TransactionRequestData data;

        public TransactionRequestMessage(string walletAddress, string toAddress, string amount, string chain, string network)
        {
            type   = "wallet";
            action = "transaction";

            data = new TransactionRequestData
            {
                walletAddress = walletAddress,
                to            = toAddress.Trim(),
                value         = amount.Trim(),
                chain         = chain,
                network       = network
            };
        }
    }

    /// <summary>
    /// Get balance request data
    /// </summary>
    [Serializable]
    public class GetBalanceRequestData : RequestData
    {
        public string walletAddress;
        public string chain;
    }

    /// <summary>
    /// Get balance request message
    /// </summary>
    [Serializable]
    public class GetBalanceRequestMessage : BaseMessage
    {
        public GetBalanceRequestData data;

        public GetBalanceRequestMessage(string walletAddress, string chain)
        {
            type   = "wallet";
            action = "getBalance";

            data = new GetBalanceRequestData
            {
                walletAddress = walletAddress,
                chain         = chain
            };
        }
    }

    /// <summary>
    /// Utility class for building request messages
    /// </summary>
    public static class RequestBuilder
    {
        /// <summary>
        /// Build connect wallet request message
        /// </summary>
        public static string BuildConnectWalletRequest()
        {
            var message = new RequestMessage("auth", "connectWallet");

            return JsonConvert.SerializeObject(message);
        }

        /// <summary>
        /// Build disconnect wallet request message
        /// </summary>
        public static string BuildDisconnectRequest()
        {
            var message = new RequestMessage("auth", "disconnect");

            return JsonConvert.SerializeObject(message);
        }

        /// <summary>
        /// Build sign message request
        /// </summary>
        public static string BuildSignMessageRequest(string walletAddress, string message)
        {
            var requestMessage = new SignMessageRequestMessage(walletAddress, message);

            return JsonConvert.SerializeObject(requestMessage);
        }

        /// <summary>
        /// Build transaction request
        /// </summary>
        public static string BuildTransactionRequest(string walletAddress, string toAddress, string amount, string chain = "sui", string network = "mainnet")
        {
            var message = new TransactionRequestMessage(walletAddress, toAddress, amount, chain, network);

            return JsonConvert.SerializeObject(message);
        }

        /// <summary>
        /// Build get balance request
        /// </summary>
        public static string BuildGetBalanceRequest(string walletAddress, string chain = "sui")
        {
            var message = new GetBalanceRequestMessage(walletAddress, chain);

            return JsonConvert.SerializeObject(message);
        }

        /// <summary>
        /// Build open profile request
        /// </summary>
        public static string BuildOpenProfileRequest(string walletAddress)
        {
            var message = new DynamicSDK.Unity.Messages.Auth.OpenProfileMessage();
            message.data = new DynamicSDK.Unity.Messages.Auth.OpenProfileData { walletAddress = walletAddress };

            return JsonConvert.SerializeObject(message);
        }

        /// <summary>
        /// Build get JWT token request
        /// </summary>
        public static string BuildGetJwtTokenRequest()
        {
            var message = new GetJwtTokenMessage();

            return JsonConvert.SerializeObject(message);
        }

        /// <summary>
        /// Input validation helpers
        /// </summary>
        public static class Validator
        {
            public static bool IsValidAddress(string address) { return !string.IsNullOrEmpty(address) && address.Length >= 32; }

            public static bool IsValidAmount(string amount) { return decimal.TryParse(amount, out decimal result) && result > 0; }

            public static bool IsValidMessage(string message) { return !string.IsNullOrEmpty(message); }

            public static bool IsValidChain(string chain) { return !string.IsNullOrEmpty(chain); }
        }
    }
}
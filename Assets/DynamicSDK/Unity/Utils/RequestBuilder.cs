using Newtonsoft.Json;
using DynamicSDK.Unity.Messages;
using DynamicSDK.Unity.Messages.Auth;
using DynamicSDK.Unity.Messages.Wallet;

namespace DynamicSDK.Unity.Utils
{
    /// <summary>
    /// Base request model to prevent stripping issues
    /// </summary>
    [System.Serializable]
    public class StripSafeBaseRequest
    {
        [JsonProperty("type")]
        public string type;
        
        [JsonProperty("action")]
        public string action;
        
        [JsonProperty("requestId")]
        public string requestId;
    }

    /// <summary>
    /// Authentication request model
    /// </summary>
    [System.Serializable]
    public class StripSafeAuthRequest : StripSafeBaseRequest
    {
        [JsonProperty("data")]
        public object data;
    }

    /// <summary>
    /// Wallet request model
    /// </summary>
    [System.Serializable]
    public class StripSafeWalletRequest : StripSafeBaseRequest
    {
        [JsonProperty("data")]
        public object data;
    }

    /// <summary>
    /// Data models for specific requests
    /// </summary>
    [System.Serializable]
    public class StripSafeSignMessageData
    {
        [JsonProperty("walletAddress")]
        public string walletAddress;
        
        [JsonProperty("message")]
        public string message;
    }

    [System.Serializable]
    public class StripSafeTransactionData
    {
        [JsonProperty("walletAddress")]
        public string walletAddress;
        
        [JsonProperty("to")]
        public string to;
        
        [JsonProperty("value")]
        public string value;
        
        [JsonProperty("chain")]
        public string chain;
        
        [JsonProperty("network")]
        public string network;
    }

    [System.Serializable]
    public class StripSafeOpenProfileData
    {
        [JsonProperty("walletAddress")]
        public string walletAddress;
    }

    [System.Serializable]
    public class StripSafeSwitchWalletData
    {
        [JsonProperty("walletId")]
        public string walletId;
    }

    [System.Serializable]
    public class StripSafeSwitchNetworkData
    {
        [JsonProperty("networkChainId")]
        public string networkChainId;
    }

    /// <summary>
    /// Utility class for building request messages with stripping-safe models
    /// </summary>
    public static class RequestBuilder
    {
        /// <summary>
        /// Build connect wallet request message
        /// </summary>
        public static string BuildConnectWalletRequest()
        {
            var message = new StripSafeAuthRequest
            {
                type = "auth",
                action = "connectWallet",
                data = new object(), // no data needed
                requestId = System.Guid.NewGuid().ToString()
            };

            return JsonConvert.SerializeObject(message);
        }

        /// <summary>
        /// Build disconnect wallet request message
        /// </summary>
        public static string BuildDisconnectRequest()
        {
            var message = new StripSafeAuthRequest
            {
                type = "auth",
                action = "disconnect",
                data = new object(),
                requestId = System.Guid.NewGuid().ToString()
            };

            return JsonConvert.SerializeObject(message);
        }

        /// <summary>
        /// Build sign message request
        /// </summary>
        public static string BuildSignMessageRequest(string walletAddress, string message)
        {
            var json = new StripSafeWalletRequest
            {
                type = "wallet",
                action = "signMessage",
                data = new StripSafeSignMessageData
                {
                    walletAddress = walletAddress,
                    message = message
                },
                requestId = System.Guid.NewGuid().ToString()
            };

            return JsonConvert.SerializeObject(json);
        }

        /// <summary>
        /// Build transaction request
        /// </summary>
        public static string BuildTransactionRequest(string walletAddress, string toAddress, string amount, string chain = "sui", string network = "mainnet")
        {
            var transactionRequest = new StripSafeWalletRequest
            {
                type = "wallet",
                action = "transaction",
                data = new StripSafeTransactionData
                {
                    walletAddress = walletAddress,
                    to = toAddress.Trim(),
                    value = amount.Trim(),
                    chain = chain,
                    network = network
                },
                requestId = System.Guid.NewGuid().ToString()
            };

            return JsonConvert.SerializeObject(transactionRequest);
        }

        /// <summary>
        /// Build get balance request
        /// </summary>
        public static string BuildGetBalanceRequest()
        {
            var balanceRequest = new StripSafeWalletRequest
            {
                type = "wallet",
                action = "getBalance",
                data = null,
                requestId = System.Guid.NewGuid().ToString()
            };

            return JsonConvert.SerializeObject(balanceRequest);
        }

        /// <summary>
        /// Build get wallets request
        /// </summary>
        public static string BuildGetWalletsRequest()
        {
            var walletsRequest = new StripSafeWalletRequest
            {
                type = "wallet",
                action = "getWallets",
                data = null,
                requestId = System.Guid.NewGuid().ToString()
            };

            return JsonConvert.SerializeObject(walletsRequest);
        }

        /// <summary>
        /// Build get networks request
        /// </summary>
        public static string BuildGetNetworksRequest()
        {
            var networksRequest = new StripSafeWalletRequest
            {
                type = "wallet",
                action = "getNetworks",
                data = null,
                requestId = System.Guid.NewGuid().ToString()
            };

            return JsonConvert.SerializeObject(networksRequest);
        }

        /// <summary>
        /// Build open profile request
        /// </summary>
        public static string BuildOpenProfileRequest(string walletAddress)
        {
            var openProfileRequest = new StripSafeAuthRequest
            {
                type = "auth",
                action = "openProfile",
                data = new StripSafeOpenProfileData
                {
                    walletAddress = walletAddress
                },
                requestId = System.Guid.NewGuid().ToString()
            };

            return JsonConvert.SerializeObject(openProfileRequest);
        }

        /// <summary>
        /// Build get JWT token request
        /// </summary>
        public static string BuildGetJwtTokenRequest()
        {
            var jwtTokenRequest = new StripSafeAuthRequest
            {
                type = "auth",
                action = "getJwtToken",
                data = new object(), // no data needed
                requestId = System.Guid.NewGuid().ToString()
            };

            return JsonConvert.SerializeObject(jwtTokenRequest);
        }

        /// <summary>
        /// Build switch wallet request
        /// </summary>
        public static string BuildSwitchWalletRequest(string walletId)
        {
            var switchWalletRequest = new StripSafeWalletRequest
            {
                type = "wallet",
                action = "switchWallet",
                data = new StripSafeSwitchWalletData
                {
                    walletId = walletId
                },
                requestId = System.Guid.NewGuid().ToString()
            };

            return JsonConvert.SerializeObject(switchWalletRequest);
        }

        /// <summary>
        /// Build switch network request
        /// </summary>
        public static string BuildSwitchNetworkRequest(string networkChainId)
        {
            var switchNetworkRequest = new StripSafeWalletRequest
            {
                type = "wallet",
                action = "switchNetwork",
                data = new StripSafeSwitchNetworkData
                {
                    networkChainId = networkChainId
                },
                requestId = System.Guid.NewGuid().ToString()
            };

            return JsonConvert.SerializeObject(switchNetworkRequest);
        }

        /// <summary>
        /// Validate required parameters for requests
        /// </summary>
        public static class Validator
        {
            public static bool IsValidAddress(string address)
            {
                return !string.IsNullOrEmpty(address) && address.Length >= 66; // 32 bytes (64 chars) + "0x" prefix
            }

            public static bool IsValidAmount(string amount)
            {
                return !string.IsNullOrEmpty(amount) &&
                       decimal.TryParse(amount, out decimal value) &&
                       value > 0;
            }

            public static bool IsValidMessage(string message) { return !string.IsNullOrEmpty(message); }
        }
    }
}
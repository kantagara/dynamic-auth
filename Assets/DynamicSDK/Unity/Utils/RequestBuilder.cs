using Newtonsoft.Json;
using DynamicSDK.Unity.Messages;
using DynamicSDK.Unity.Messages.Auth;
using DynamicSDK.Unity.Messages.Wallet;

namespace DynamicSDK.Unity.Utils
{
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
            var message = new
            {
                type      = "auth",
                action    = "connectWallet",
                data      = new { }, // no data needed
                requestId = System.Guid.NewGuid().ToString()
            };

            return JsonConvert.SerializeObject(message);
        }

        /// <summary>
        /// Build disconnect wallet request message
        /// </summary>
        public static string BuildDisconnectRequest()
        {
            var message = new
            {
                type      = "auth",
                action    = "disconnect",
                data      = new { },
                requestId = System.Guid.NewGuid().ToString()
            };

            return JsonConvert.SerializeObject(message);
        }

        /// <summary>
        /// Build sign message request
        /// </summary>
        public static string BuildSignMessageRequest(string walletAddress, string message)
        {
            var json = new
            {
                type   = "wallet",
                action = "signMessage",
                data = new
                {
                    walletAddress = walletAddress,
                    message       = message
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
            var transactionRequest = new
            {
                type   = "wallet",
                action = "transaction",
                data = new
                {
                    walletAddress = walletAddress,
                    to            = toAddress.Trim(),
                    value         = amount.Trim(),
                    chain         = chain,
                    network       = network
                },
                requestId = System.Guid.NewGuid().ToString()
            };

            return JsonConvert.SerializeObject(transactionRequest);
        }

        /// <summary>
        /// Build get balance request
        /// </summary>
        public static string BuildGetBalanceRequest(string walletAddress, string chain = "sui")
        {
            var balanceRequest = new
            {
                type   = "wallet",
                action = "getBalance",
                data = new
                {
                    walletAddress = walletAddress,
                    chain         = chain
                },
                requestId = System.Guid.NewGuid().ToString()
            };

            return JsonConvert.SerializeObject(balanceRequest);
        }

        /// <summary>
        /// Build open profile request
        /// </summary>
        public static string BuildOpenProfileRequest(string walletAddress)
        {
            var openProfileRequest = new
            {
                type   = "auth",
                action = "openProfile",
                data = new
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
            var jwtTokenRequest = new
            {
                type      = "auth",
                action    = "getJwtToken",
                data      = new { }, // no data needed
                requestId = System.Guid.NewGuid().ToString()
            };

            return JsonConvert.SerializeObject(jwtTokenRequest);
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
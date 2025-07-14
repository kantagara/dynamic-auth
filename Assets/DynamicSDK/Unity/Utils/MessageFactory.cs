using DynamicSDK.Unity.Messages.Auth;
using DynamicSDK.Unity.Messages.Wallet;

namespace DynamicSDK.Unity.Messages
{
    /// <summary>
    /// Factory class for creating messages
    /// </summary>
    public static class MessageFactory
    {
        // ============================================================================
        // AUTH MESSAGE CREATORS
        // ============================================================================

        public static AuthRequestMessage CreateAuthRequest(string gameId, string[] requiredChains, int sessionExpiry = 3600)
        {
            return new AuthRequestMessage
            {
                data = new AuthRequestData
                {
                    gameId         = gameId,
                    requiredChains = requiredChains,
                    sessionExpiry  = sessionExpiry
                }
            };
        }

        public static LogoutMessage CreateLogoutMessage(string reason = "user_requested")
        {
            return new LogoutMessage
            {
                data = new LogoutData { reason = reason }
            };
        }

        // ============================================================================
        // WALLET MESSAGE CREATORS
        // ============================================================================

        public static GetBalanceMessage CreateGetBalanceRequest(string walletAddress, string chain, string tokenAddress = null, string network = "mainnet")
        {
            return new GetBalanceMessage
            {
                data = new GetBalanceData
                {
                    walletAddress = walletAddress,
                    chain         = chain,
                    tokenAddress  = tokenAddress,
                    network       = network
                }
            };
        }

        public static SignMessageMessage CreateSignMessageRequest(string walletAddress, string message, string chain, string messageType = MessageTypes.PERSONAL)
        {
            return new SignMessageMessage
            {
                data = new SignMessageData
                {
                    walletAddress = walletAddress,
                    message       = message,
                }
            };
        }

        public static TransactionMessage CreateTransactionRequest(string walletAddress, string to, string value, string data = "", string chain = "sui", string network = "mainnet",
            string type = TransactionTypes.SEND)
        {
            return new TransactionMessage
            {
                data = new TransactionData
                {
                    walletAddress = walletAddress,
                    to            = to,
                    value         = value,
                    data          = data,
                    chain         = chain,
                    network       = network,
                    type          = type
                }
            };
        }
    }
}
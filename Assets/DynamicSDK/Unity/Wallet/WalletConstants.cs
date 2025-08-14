namespace DynamicSDK.Unity.Messages.Wallet
{
    /// <summary>
    /// Wallet actions enumeration
    /// </summary>
    public static class WalletActions
    {
        public const string GET_BALANCE = "getBalance";
        public const string BALANCE_RESPONSE = "balanceResponse";
        public const string SWITCH_NETWORK = "switchNetwork";
        public const string SWITCH_WALLET = "switchWallet";
        public const string SIGN_MESSAGE = "signMessage";
        public const string SIGN_MESSAGE_RESPONSE = "signMessageResponse";
        public const string TRANSACTION = "transaction";
        public const string TRANSACTION_RESPONSE = "transactionResponse";
        public const string WALLET_CONNECTED = "walletConnected";
        public const string WALLET_DISCONNECTED = "walletDisconnected";
        public const string WALLET_ERROR = "walletError";
        public const string OPEN_PROFILE = "openProfile";
        public const string WALLETS_RESPONSE = "walletsResponse";
        public const string NETWORKS_RESPONSE = "networksResponse";
    }

    /// <summary>
    /// Message types for signing
    /// </summary>
    public static class MessageTypes
    {
        public const string PERSONAL = "personal";
        public const string TYPED = "typed";
        public const string TEXT = "text";
    }

    /// <summary>
    /// Transaction types
    /// </summary>
    public static class TransactionTypes
    {
        public const string SEND = "send";
        public const string CONTRACT = "contract";
        public const string APPROVAL = "approval";
    }
}
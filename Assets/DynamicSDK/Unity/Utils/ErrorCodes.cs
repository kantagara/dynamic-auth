namespace DynamicSDK.Unity.Messages
{
    /// <summary>
    /// Error code constants
    /// </summary>
    public static class ErrorCodes
    {
        // ============================================================================
        // AUTH ERRORS
        // ============================================================================

        public const string AUTH_USER_REJECTED        = "AUTH_USER_REJECTED";
        public const string AUTH_NETWORK_ERROR        = "NETWORK_ERROR";
        public const string AUTH_INVALID_EMAIL_DOMAIN = "INVALID_EMAIL_DOMAIN";
        public const string AUTH_SESSION_EXPIRED      = "SESSION_EXPIRED";

        // ============================================================================
        // WALLET ERRORS
        // ============================================================================

        public const string WALLET_NOT_CONNECTED            = "WALLET_NOT_CONNECTED";
        public const string WALLET_USER_REJECTED            = "USER_REJECTED";
        public const string WALLET_INSUFFICIENT_FUNDS       = "INSUFFICIENT_FUNDS";
        public const string WALLET_NETWORK_ERROR            = "NETWORK_ERROR";
        public const string WALLET_INVALID_ADDRESS          = "INVALID_ADDRESS";
        public const string WALLET_INVALID_CHAIN            = "INVALID_CHAIN";
        public const string WALLET_TRANSACTION_FAILED       = "TRANSACTION_FAILED";
        public const string WALLET_GAS_BUDGET_ERROR         = "GAS_BUDGET_ERROR";
        public const string WALLET_SIGNATURE_FAILED         = "SIGNATURE_FAILED";
        public const string WALLET_OBJECT_NOT_FOUND         = "OBJECT_NOT_FOUND";
        public const string WALLET_INVALID_TRANSACTION_DATA = "INVALID_TRANSACTION_DATA";
    }
}
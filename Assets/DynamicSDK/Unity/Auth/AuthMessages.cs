using System;
using UnityEngine;

namespace DynamicSDK.Unity.Messages.Auth
{
    // ============================================================================
    // AUTH SUCCESS MESSAGE
    // ============================================================================

    [Serializable]
    public class AuthSuccessData
    {
        public WalletCredential primaryWallet;
        public UserInfo user;
        public WalletCredential[] wallets;
        public string authMethod;
        public string provider;
        public string sessionToken;
    }

    [Serializable]
    public class AuthSuccessMessage : BaseMessage
    {
        public AuthSuccessData data;

        public AuthSuccessMessage()
        {
            type = "auth";
            action = AuthActions.AUTH_SUCCESS;
        }
    }

    // ============================================================================
    // AUTH FAILED MESSAGE
    // ============================================================================

    [Serializable]
    public class AuthFailedData : ErrorInfo
    {
    }

    [Serializable]
    public class AuthFailedMessage : BaseMessage
    {
        public AuthFailedData data;

        public AuthFailedMessage()
        {
            type = "auth";
            action = AuthActions.AUTH_FAILED;
        }
    }

    // ============================================================================
    // LOGOUT MESSAGE
    // ============================================================================

    [Serializable]
    public class LogoutData
    {
        public string reason;
    }

    [Serializable]
    public class LogoutMessage : BaseMessage
    {
        public LogoutData data;

        public LogoutMessage()
        {
            type = "auth";
            action = AuthActions.LOGOUT;
            data = new LogoutData { reason = "user_requested" };
        }
    }

    // ============================================================================
    // LOGGED OUT MESSAGE
    // ============================================================================

    [Serializable]
    public class LoggedOutData
    {
        public bool success;
        public long timestamp;
    }

    [Serializable]
    public class LoggedOutMessage : BaseMessage
    {
        public LoggedOutData data;

        public LoggedOutMessage()
        {
            type = "auth";
            action = AuthActions.LOGGED_OUT;
        }
    }

    // ============================================================================
    // HANDLE AUTHENTICATED USER MESSAGE
    // ============================================================================

    [Serializable]
    public class HandleAuthenticatedUserData
    {
        public UserInfo user;
        public WalletCredential[] wallets;
        public string sessionToken;
    }

    [Serializable]
    public class HandleAuthenticatedUserMessage : BaseMessage
    {
        public HandleAuthenticatedUserData data;

        public HandleAuthenticatedUserMessage()
        {
            type = "auth";
            action = AuthActions.HANDLE_AUTHENTICATED_USER;
        }
    }

    // ============================================================================
    // AUTH REQUEST MESSAGE
    // ============================================================================

    [Serializable]
    public class AuthRequestData
    {
        public string gameId;
        public string[] requiredChains;
        public int sessionExpiry;
    }

    [Serializable]
    public class AuthRequestMessage : BaseMessage
    {
        public AuthRequestData data;

        public AuthRequestMessage()
        {
            type = "auth";
            action = AuthActions.AUTH_REQUEST;
        }
    }

    // ============================================================================
    // OPEN PROFILE MESSAGE
    // ============================================================================

    [Serializable]
    public class OpenProfileData
    {
        public string walletAddress;
    }

    [Serializable]
    public class OpenProfileMessage : BaseMessage
    {
        public OpenProfileData data;

        public OpenProfileMessage()
        {
            type = "auth";
            action = AuthActions.OPEN_PROFILE;
        }
    }

    // ============================================================================
    // GET JWT TOKEN MESSAGE
    // ============================================================================

    [Serializable]
    public class GetJwtTokenData
    {
        // No data needed for JWT token request
    }

    [Serializable]
    public class GetJwtTokenMessage : BaseMessage
    {
        public GetJwtTokenData data;

        public GetJwtTokenMessage()
        {
            type = "auth";
            action = AuthActions.GET_JWT_TOKEN;
            data = new GetJwtTokenData();
        }
    }

    // ============================================================================
    // JWT TOKEN RESPONSE MESSAGE
    // ============================================================================

    [Serializable]
    public class JwtTokenResponseData
    {
        public string token;
        public string userId;
        public string email;
        public long timestamp;
    }

    [Serializable]
    public class JwtTokenResponseMessage : BaseMessage
    {
        public JwtTokenResponseData data;

        public JwtTokenResponseMessage()
        {
            type = "auth";
            action = AuthActions.JWT_TOKEN_RESPONSE;
        }
    }
}
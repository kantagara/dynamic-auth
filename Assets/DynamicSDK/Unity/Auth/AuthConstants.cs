namespace DynamicSDK.Unity.Messages.Auth
{
    /// <summary>
    /// Auth actions enumeration
    /// </summary>
    public static class AuthActions
    {
        public const string AUTH_SUCCESS              = "authSuccess";
        public const string AUTH_FAILED               = "authFailed";
        public const string LOGOUT                    = "logout";
        public const string LOGGED_OUT                = "loggedOut";
        public const string HANDLE_AUTHENTICATED_USER = "handleAuthenticatedUser";
        public const string AUTH_REQUEST              = "authRequest";
        public const string OPEN_PROFILE              = "openProfile";
        public const string GET_JWT_TOKEN             = "getJwtToken";
        public const string JWT_TOKEN_RESPONSE        = "jwtTokenResponse";
    }
}
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicSDK.Unity.Core
{
    /// <summary>
    /// Handles deep linking for OAuth callbacks and other app-specific URLs
    /// </summary>
    public class DeepLinkHandler : MonoBehaviour
    {
        private static DeepLinkHandler instance;
        private DynamicSDKConfig config;
        private WebViewService webViewService;
        
        // Events
        public System.Action<string> OnDeepLinkReceived;
        public System.Action<string, Dictionary<string, string>> OnOAuthCallback;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                config = DynamicSDKConfig.Instance;
                
                // Register for deep link events
                Application.deepLinkActivated += HandleDeepLink;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            webViewService = GetComponent<WebViewService>();
            
            // Check if the app was launched with a deep link
            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                HandleDeepLink(Application.absoluteURL);
            }
        }
        
        /// <summary>
        /// Handle incoming deep links
        /// </summary>
        private void HandleDeepLink(string url)
        {
            Debug.Log($"[DeepLinkHandler] Received deep link: {url}");
            
            OnDeepLinkReceived?.Invoke(url);
            
            // Check if this is an OAuth callback
            if (IsOAuthCallback(url))
            {
                Debug.Log($"[DeepLinkHandler] Detected OAuth callback, processing...");
                ProcessOAuthCallback(url);
            }
            else
            {
                Debug.Log($"[DeepLinkHandler] Not an OAuth callback: {url}");
            }
        }
        
        /// <summary>
        /// Check if the URL is an OAuth callback
        /// </summary>
        private bool IsOAuthCallback(string url)
        {
            // Check for our hardcoded deeplink scheme
            // This should match what we set in ModifyRedirectUri
            string[] appDeeplinks = new string[]
            {
                "dynamicunity://auth/callback",
                "dynamicunity://oauth/callback",
                "dynamicunity://auth",
                "dynamicunity://"
            };
            
            foreach (var deeplink in appDeeplinks)
            {
                if (url.StartsWith(deeplink))
                {
                    if (config.enableDebugLogs)
                    {
                        Debug.Log($"[DeepLinkHandler] OAuth callback detected: {url}");
                    }
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Process OAuth callback and extract parameters
        /// </summary>
        private void ProcessOAuthCallback(string url)
        {
            if (config.enableDebugLogs)
            {
                Debug.Log($"[DeepLinkHandler] Processing OAuth callback: {url}");
            }
            
            // Parse URL parameters
            var parameters = ParseUrlParameters(url);
            
            // Check for common OAuth parameters
            if (parameters.ContainsKey("code"))
            {
                // Authorization code flow
                HandleAuthorizationCode(parameters["code"], parameters);
            }
            else if (parameters.ContainsKey("access_token"))
            {
                // Implicit flow
                HandleAccessToken(parameters["access_token"], parameters);
            }
            else if (parameters.ContainsKey("error"))
            {
                // OAuth error
                HandleOAuthError(parameters["error"], parameters);
            }
            
            // Trigger callback event
            OnOAuthCallback?.Invoke(url, parameters);
            
            // Return to app and close system browser if needed
            ReturnToApp();
        }
        
        /// <summary>
        /// Parse URL parameters into dictionary
        /// </summary>
        private Dictionary<string, string> ParseUrlParameters(string url)
        {
            var parameters = new Dictionary<string, string>();
            
            // Find the query string or fragment
            int queryStart = url.IndexOf('?');
            int fragmentStart = url.IndexOf('#');
            
            string paramString = "";
            if (queryStart >= 0)
            {
                paramString = url.Substring(queryStart + 1);
            }
            else if (fragmentStart >= 0)
            {
                paramString = url.Substring(fragmentStart + 1);
            }
            
            // Parse parameters
            if (!string.IsNullOrEmpty(paramString))
            {
                var pairs = paramString.Split('&');
                foreach (var pair in pairs)
                {
                    var keyValue = pair.Split('=');
                    if (keyValue.Length == 2)
                    {
                        var key = UnityEngine.Networking.UnityWebRequest.UnEscapeURL(keyValue[0]);
                        var value = UnityEngine.Networking.UnityWebRequest.UnEscapeURL(keyValue[1]);
                        parameters[key] = value;
                    }
                }
            }
            
            return parameters;
        }
        
        /// <summary>
        /// Handle authorization code from OAuth callback
        /// </summary>
        private void HandleAuthorizationCode(string code, Dictionary<string, string> parameters)
        {
            if (config.enableDebugLogs)
            {
                Debug.Log($"[DeepLinkHandler] Received authorization code: {code}");
            }
            
            // Reopen WebView and complete OAuth
            if (webViewService != null)
            {
                // Make sure WebView is visible
                webViewService.OpenBottomSheet();
                
                // Build URL properly with encoded parameters
                // Use the base URL from config instead of hardcoding
                string baseUrl = config.startUrl.Split('?')[0]; // Get base URL without query params
                var uriBuilder = new System.UriBuilder(baseUrl);
                var query = System.Web.HttpUtility.ParseQueryString("");
                
                // Parse original manifest from config
                if (config.startUrl.Contains("manifest="))
                {
                    var startUri = new System.Uri(config.startUrl);
                    var startQuery = System.Web.HttpUtility.ParseQueryString(startUri.Query);
                    string manifestValue = startQuery["manifest"];
                    if (!string.IsNullOrEmpty(manifestValue))
                    {
                        query["manifest"] = manifestValue;
                    }
                }
                
                // Add OAuth parameters
                query["dynamicOauthCode"] = code;
                
                if (parameters.ContainsKey("state"))
                {
                    query["dynamicOauthState"] = parameters["state"];
                }
                
                uriBuilder.Query = query.ToString();
                string callbackUrl = uriBuilder.ToString();
                
                if (config.enableDebugLogs)
                {
                    Debug.Log($"[DeepLinkHandler] Loading callback in WebView: {callbackUrl}");
                }
                
                webViewService.Load(callbackUrl);
            }
        }
        
        /// <summary>
        /// Handle access token from OAuth callback
        /// </summary>
        private void HandleAccessToken(string accessToken, Dictionary<string, string> parameters)
        {
            if (config.enableDebugLogs)
            {
                Debug.Log($"[DeepLinkHandler] Received access token");
            }
            
            // Send the access token back to the WebView if it's open
            if (webViewService != null)
            {
                var message = new
                {
                    type = "oauth_callback",
                    action = "access_token",
                    access_token = accessToken,
                    token_type = parameters.ContainsKey("token_type") ? parameters["token_type"] : null,
                    expires_in = parameters.ContainsKey("expires_in") ? parameters["expires_in"] : null,
                    state = parameters.ContainsKey("state") ? parameters["state"] : null
                };
                
                string jsonMessage = JsonUtility.ToJson(message);
                webViewService.SendMessage(jsonMessage);
            }
        }
        
        /// <summary>
        /// Handle OAuth error
        /// </summary>
        private void HandleOAuthError(string error, Dictionary<string, string> parameters)
        {
            string errorDescription = parameters.ContainsKey("error_description") ? 
                parameters["error_description"] : "OAuth authentication failed";
                
            Debug.LogError($"[DeepLinkHandler] OAuth error: {error} - {errorDescription}");
            
            // Send error back to WebView
            if (webViewService != null)
            {
                var message = new
                {
                    type = "oauth_callback",
                    action = "error",
                    error = error,
                    error_description = errorDescription
                };
                
                string jsonMessage = JsonUtility.ToJson(message);
                webViewService.SendMessage(jsonMessage);
            }
        }
        
        /// <summary>
        /// Return focus to the app after OAuth callback
        /// </summary>
        private void ReturnToApp()
        {
            // On mobile, this will bring the app back to foreground
            // The system browser will remain in background
            
            if (config.enableDebugLogs)
            {
                Debug.Log("[DeepLinkHandler] Returning to app after OAuth callback");
            }
            
            // WebView will be shown automatically when LoadCallbackUrl is called
            // No need to manually show it here
        }
        
        private void OnDestroy()
        {
            if (instance == this)
            {
                Application.deepLinkActivated -= HandleDeepLink;
                instance = null;
            }
        }
        
        /// <summary>
        /// Register a custom URL scheme for the app
        /// </summary>
        public static void RegisterCustomScheme(string scheme)
        {
            if (instance != null && instance.config.enableDebugLogs)
            {
                Debug.Log($"[DeepLinkHandler] Registering custom scheme: {scheme}");
            }
            
            // This is handled by platform-specific build settings
            // The actual registration happens in:
            // - iOS: Info.plist CFBundleURLSchemes
            // - Android: AndroidManifest.xml intent-filter
        }
    }
}
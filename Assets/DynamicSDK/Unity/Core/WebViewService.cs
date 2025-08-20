using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;

namespace DynamicSDK.Unity.Core
{
    /// <summary>
    /// Service to manage UniWebView operations
    /// </summary>
    public class WebViewService : MonoBehaviour
    {
        private UniWebView webView;
        private DynamicSDKConfig config;
        private bool isWebViewVisible = false;
        private bool isWebViewReady = false;
        private Rect webViewRect;
        private string currentUrl;

        // Events
        public System.Action<UniWebViewMessage> OnMessageReceived;
        public System.Action OnWebViewClosed;
        public System.Action OnWebViewReady;
        public System.Action<string> OnUrlChanged;

        private void Awake()
        {
            config = DynamicSDKConfig.Instance;
        }

        private void Update()
        {
            // Handle click outside to close webview (only if enabled)
            if (config.enableClickOutsideToClose && isWebViewVisible && webView != null && webView.gameObject.activeInHierarchy)
            {
                // Check for mouse click or touch
                bool inputDetected = false;
                Vector2 inputPosition = Vector2.zero;

                // Handle mouse input
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    inputDetected = true;
                    inputPosition = Mouse.current.position.ReadValue();
                }
                // Handle touch input
                else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                {
                    inputDetected = true;
                    inputPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                }

                if (inputDetected)
                {
                    // Convert Unity screen coordinates to match webview coordinates
                    Vector2 convertedPosition = new Vector2(inputPosition.x, Screen.height - inputPosition.y);

                    if (config.enableDebugLogs)
                    {
                        Debug.Log($"[WebViewService] Input detected at {inputPosition} -> converted: {convertedPosition}, webview rect: {webViewRect}, contains: {webViewRect.Contains(convertedPosition)}");
                    }

                    if (!webViewRect.Contains(convertedPosition))
                    {
                        if (config.enableDebugLogs)
                        {
                            Debug.Log($"[WebViewService] Click outside detected - closing webview");
                        }
                        HideWithAnimation();
                    }
                }
            }
        }

        /// <summary>
        /// Open bottom sheet WebView
        /// </summary>
        public void OpenBottomSheet()
        {
            if (webView == null)
            {
                // Create webview for first time
                CreateWebView();
                SetupWebViewFrame();
                ConfigureWebView();
            }
            else
            {
                // Reuse existing webview
                if (config.enableDebugLogs)
                {
                    Debug.Log("[WebViewService] Reusing existing WebView");
                }
            }

            LoadAndShow();
        }

        /// <summary>
        /// Hide the WebView (using SetActive instead of destroy)
        /// </summary>
        public void HideWebView()
        {
            if (webView != null)
            {
                webView.gameObject.SetActive(false);
                if (config.enableDebugLogs)
                {
                    Debug.Log("[WebViewService] WebView hidden (SetActive false)");
                }
            }
            isWebViewVisible = false;
            OnWebViewClosed?.Invoke();
        }

        /// <summary>
        /// Completely close and destroy the WebView (only when really needed)
        /// </summary>
        public void CloseWebView()
        {
            if (webView != null)
            {
                Destroy(webView);
                webView = null;
                if (config.enableDebugLogs)
                {
                    Debug.Log("[WebViewService] WebView destroyed");
                }
            }
            isWebViewVisible = false;
            isWebViewReady = false;
        }

        /// <summary>
        /// Reset WebView (useful when URL config changes)
        /// </summary>
        public void ResetWebView()
        {
            if (config.enableDebugLogs)
            {
                Debug.Log("[WebViewService] Resetting WebView...");
            }

            CloseWebView();
            // Next OpenBottomSheet() call will create a fresh webview
        }

        /// <summary>
        /// Get the current URL of the WebView
        /// </summary>
        public string GetCurrentUrl()
        {
            if (webView != null)
            {
                return webView.Url;
            }
            return string.Empty;
        }

        /// <summary>
        /// Load a URL in the WebView
        /// </summary>
        public void Load(string url)
        {
            if (webView != null)
            {
                webView.Load(url);
                if (config.enableDebugLogs)
                {
                    Debug.Log($"[WebViewService] Loading URL: {url}");
                }
            }
            else
            {
                Debug.LogError("[WebViewService] Cannot load URL - WebView is null");
            }
        }


        /// <summary>
        /// Pre-load WebView in background without showing it
        /// </summary>
        public void PreloadWebView()
        {
            if (webView == null)
            {
                if (config.enableDebugLogs)
                {
                    Debug.Log("[WebViewService] Pre-loading WebView in background...");
                }

                // Create and setup webview
                CreateWebView();
                SetupWebViewFrame();
                ConfigureWebView();

                // Load URL but keep hidden
                PreloadURL();
            }
            else
            {
                if (config.enableDebugLogs)
                {
                    Debug.Log("[WebViewService] WebView already exists, skipping preload");
                }
            }
        }

        /// <summary>
        /// Send a message to the WebView using the correct custom event pattern
        /// </summary>
        public void SendMessage(string jsonMessage)
        {
            if (webView != null && webView.gameObject.activeInHierarchy && isWebViewReady)
            {
                if (config.enableDebugLogs)
                {
                    Debug.Log($"[WebViewService] Sending message: {jsonMessage}");
                }

                // Determine event type based on message content
                string eventType = DetermineEventType(jsonMessage);

                // Send message via JavaScript custom event (matching original implementation)
                string script = $@"
                window.dispatchEvent(new CustomEvent('{eventType}', {{
                    detail: {jsonMessage}
                }}));
                ";

                webView.EvaluateJavaScript(script, (result) =>
                {
                    if (config.enableDebugLogs)
                    {
                        Debug.Log($"[WebViewService] JS sent to WebView: {script}");
                    }
                });
            }
            else
            {
                string reason = webView == null ? "WebView is not initialized" :
                               !webView.gameObject.activeInHierarchy ? "WebView is not active" :
                               "WebView is not ready";
                Debug.LogWarning($"[WebViewService] Cannot send message - {reason}");

                // If webview exists but not ready, retry after a short delay
                if (webView != null && webView.gameObject.activeInHierarchy && !isWebViewReady)
                {
                    StartCoroutine(RetryMessageAfterDelay(jsonMessage, 0.5f));
                }
            }
        }

        private IEnumerator RetryMessageAfterDelay(string jsonMessage, float delay)
        {
            yield return new WaitForSeconds(delay);

            // Try again, but only once to avoid infinite loops
            if (webView != null && webView.gameObject.activeInHierarchy && isWebViewReady)
            {
                SendMessage(jsonMessage);
            }
            else
            {
                Debug.LogWarning($"[WebViewService] Retry failed - WebView still not ready");
            }
        }

        /// <summary>
        /// Determine the correct event type based on message content
        /// </summary>
        private string DetermineEventType(string jsonMessage)
        {
            try
            {
                // Parse the JSON to determine message type
                var messageObj = JsonUtility.FromJson<BaseMessageInfo>(jsonMessage);

                switch (messageObj.type?.ToLower())
                {
                    case "auth":
                        return "unityAuthRequest";
                    case "wallet":
                        return "unityWalletRequest";
                    default:
                        return "unityRequest"; // fallback
                }
            }
            catch
            {
                // Fallback if parsing fails
                return "unityRequest";
            }
        }

        /// <summary>
        /// Helper class for determining message type
        /// </summary>
        [System.Serializable]
        private class BaseMessageInfo
        {
            public string type;
            public string action;
        }

        /// <summary>
        /// Hide WebView with animation (using SetActive instead of destroy)
        /// </summary>
        public void HideWithAnimation()
        {
            if (webView != null && isWebViewVisible)
            {
                isWebViewVisible = false;
                webView.Hide(
                    fade: false,
                    edge: UniWebViewTransitionEdge.Bottom,
                    duration: config.transitionDuration,
                    completionHandler: () =>
                    {
                        // Use SetActive instead of Destroy
                        webView.gameObject.SetActive(false);
                        OnWebViewClosed?.Invoke();

                        if (config.enableDebugLogs)
                        {
                            Debug.Log("[WebViewService] WebView hidden with animation (SetActive false)");
                        }
                    }
                );
            }
        }

        /// <summary>
        /// Retry operation with delay
        /// </summary>
        public void RetryWithDelay(System.Action operation, float delay = -1f)
        {
            float retryDelay = delay > 0 ? delay : config.retryDelay;
            StartCoroutine(WaitAndRetry(operation, retryDelay));
        }

        private IEnumerator WaitAndRetry(System.Action operation, float delay)
        {
            yield return new WaitForSeconds(delay);
            operation?.Invoke();
        }

        private void CreateWebView()
        {
            webView = gameObject.AddComponent<UniWebView>();
            if (config.enableDebugLogs)
            {
                Debug.Log("[WebViewService] New WebView created");
            }
        }

        private void SetupWebViewFrame()
        {
#if !UNITY_EDITOR
            // Calculate size and position for bottom sheet
            float screenHeight = Screen.height;
            float screenWidth = Screen.width;
            float sheetHeight = screenHeight * config.heightRatio;
            float offsetHeight = screenHeight * config.bottomOffset;
            float bottomPosition = screenHeight - sheetHeight - offsetHeight;

            // Set frame for webview with bottom offset to display higher
            webViewRect = new Rect(0, bottomPosition, screenWidth, sheetHeight);
            webView.Frame = webViewRect;

            if (config.enableDebugLogs)
            {
                Debug.Log($"[WebViewService] WebView Frame: x=0, y={bottomPosition}, width={screenWidth}, height={sheetHeight}, offset={offsetHeight}");
            }
#else
            // In Unity Editor, use full screen for easier testing
            webViewRect = new Rect(0, 0, Screen.width, Screen.height);
            webView.Frame = webViewRect;
            UniWebView.SetWebContentsDebuggingEnabled(true);
#endif
        }

        private void ConfigureWebView()
        {
            // Set other properties
            webView.SetShowToolbar(false, false, false, false);
            webView.BackgroundColor = new Color(0, 0, 0, 0);

            // Setup event handlers
            webView.OnMessageReceived += HandleMessage;
            webView.OnShouldClose += HandleShouldClose;
            webView.OnPageStarted += HandlePageStarted;
            webView.OnPageFinished += HandlePageFinished;

            // Register URL interception handler for OAuth providers
            webView.RegisterShouldHandleRequest(ShouldHandleRequest);

            if (config.enableDebugLogs)
            {
                Debug.Log("[WebViewService] WebView configured");
            }
        }

        private void LoadAndShow()
        {
            // Make sure webview is active before loading
            if (!webView.gameObject.activeInHierarchy)
            {
                webView.gameObject.SetActive(true);
                if (config.enableDebugLogs)
                {
                    Debug.Log("[WebViewService] WebView activated");
                }
            }

            // Set visible flag immediately when starting to show
            isWebViewVisible = true;

            // Only load if not ready (avoid reloading on reuse)
            if (!isWebViewReady)
            {
                webView.Load(config.startUrl);
                if (config.enableDebugLogs)
                {
                    Debug.Log($"[WebViewService] Loading URL: {config.startUrl}");
                }
            }

            // Show with slide up animation from bottom
            webView.Show(
                fade: false,
                edge: UniWebViewTransitionEdge.Bottom,
                duration: config.transitionDuration,
                completionHandler: () =>
                {
                    if (config.enableDebugLogs)
                    {
                        Debug.Log("[WebViewService] WebView is now visible and ready for interaction");
                    }
                }
            );
        }

        private void PreloadURL()
        {
            // Keep webview hidden but load the URL
            // webView.gameObject.SetActive(false);

            // Load URL in background
            webView.Load(config.startUrl);

            if (config.enableDebugLogs)
            {
                Debug.Log($"[WebViewService] Pre-loading URL in background: {config.startUrl}");
            }
        }

        private void HandleMessage(UniWebView view, UniWebViewMessage msg)
        {
            OnMessageReceived?.Invoke(msg);
        }

        private void HandlePageStarted(UniWebView view, string url)
        {
            if (config.enableDebugLogs)
            {
                Debug.Log($"[WebViewService] Page navigation started to: {url}");
            }
            
            // Handle special case for Dynamic SDK OAuth redirect
            if (url.Contains("app.dynamicauth.com") && url.Contains("/redirect"))
            {
                if (config.enableDebugLogs)
                {
                    Debug.Log($"[WebViewService] Dynamic SDK OAuth redirect detected, waiting for final redirect...");
                }
                // Don't update current URL yet, wait for final redirect
                return;
            }
            
            // Check if URL actually changed
            if (currentUrl != url)
            {
                string previousUrl = currentUrl;
                currentUrl = url;
                
                if (config.enableDebugLogs)
                {
                    Debug.Log($"[WebViewService] URL changed from '{previousUrl}' to '{url}'");
                }
                
                // Trigger URL changed event
                OnUrlChanged?.Invoke(url);
            }
        }

        private void HandlePageFinished(UniWebView view, int statusCode, string url)
        {
            isWebViewReady = true;
            currentUrl = url; // Update current URL when page finishes loading
            
            if (config.enableDebugLogs)
            {
                Debug.Log($"[WebViewService] WebView page finished loading: {url} (Status: {statusCode})");
            }
            
            // Inject JavaScript to mark this as WebView context
            string jsCode = @"
                // Mark as Unity WebView
                localStorage.setItem('isUnityWebView', 'true');
                window.isUnityWebView = true;
                console.log('[Unity] Marked as WebView context');
            ";
            
            webView.EvaluateJavaScript(jsCode, (result) => 
            {
                if (config.enableDebugLogs)
                {
                    Debug.Log("[WebViewService] Injected WebView context marker");
                }
            });
            
            OnWebViewReady?.Invoke();
        }

        private bool HandleShouldClose(UniWebView view)
        {
            HideWithAnimation();
            return true;
        }

        private bool ShouldHandleRequest(UniWebViewChannelMethodHandleRequest request)
        {
            string url = request.Url;
            
            if (config.enableDebugLogs)
            {
                Debug.Log($"[WebViewService] Checking if should handle URL: {url}");
            }

            // List of OAuth provider domains that should open in system browser
            string[] oauthProviders = new string[]
            {
                "accounts.google.com",
                "www.facebook.com",
                "facebook.com",
                "appleid.apple.com",
                "login.microsoftonline.com",
                "github.com",
                "twitter.com",
                "x.com",
                "discord.com",
                "linkedin.com"
            };

            // Check if URL is from OAuth provider
            foreach (string provider in oauthProviders)
            {
                if (url.Contains(provider))
                {
                    // Replace redirect_uri with deeplink
                    string modifiedUrl = ReplaceRedirectUriWithDeeplink(url);
                    
                    if (config.enableDebugLogs)
                    {
                        Debug.Log($"[WebViewService] OAuth provider detected ({provider})");
                        Debug.Log($"[WebViewService] Original URL: {url}");
                        Debug.Log($"[WebViewService] Modified URL: {modifiedUrl}");
                    }

                    // Open in system browser with modified URL
                    OpenInSystemBrowser(modifiedUrl);
                    
                    // Return false to cancel navigation in WebView
                    return false;
                }
            }

            // Allow WebView to handle all other URLs
            return true;
        }

        private string ReplaceRedirectUriWithDeeplink(string url)
        {
            // Simply store the original redirect URI for OAuth providers
            // Don't modify the URL since we're opening in system browser
            try
            {
                var uri = new System.Uri(url);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                
                // For OAuth providers, store the original redirect
                if (uri.Host.Contains("accounts.google.com") || 
                    uri.Host.Contains("facebook.com") || 
                    uri.Host.Contains("appleid.apple.com"))
                {
                    string redirectUri = query["redirect_uri"];
                    if (!string.IsNullOrEmpty(redirectUri))
                    {
                        PlayerPrefs.SetString("oauth_original_redirect", redirectUri);
                        PlayerPrefs.SetString("oauth_opened_from_unity", "true");
                        PlayerPrefs.Save();
                        
                        if (config.enableDebugLogs)
                        {
                            Debug.Log($"[WebViewService] Stored original redirect URI: {redirectUri}");
                            Debug.Log($"[WebViewService] Marked OAuth as opened from Unity app");
                        }
                    }
                }
                
                return url;
            }
            catch (System.Exception e)
            {
                if (config.enableDebugLogs)
                {
                    Debug.LogError($"[WebViewService] Error parsing URL: {e.Message}");
                }
                return url;
            }
        }

        private void OpenInSystemBrowser(string url)
        {
            if (config.enableDebugLogs)
            {
                Debug.Log($"[WebViewService] Opening URL in system browser: {url}");
            }

            // Simply open URL in system browser
            Application.OpenURL(url);
            
            // Hide the WebView while OAuth is in progress
            HideWebView();
        }

        private void OnDestroy()
        {
            if (webView != null)
            {
                // Unregister handlers before destroying
                webView.UnregisterShouldHandleRequest();
            }
            CloseWebView();
        }
    }
}
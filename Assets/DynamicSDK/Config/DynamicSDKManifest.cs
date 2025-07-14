using UnityEngine;

namespace DynamicSDK.Unity.Core
{
    [CreateAssetMenu(fileName = "DynamicSDKManifest", menuName = "DynamicSDK/Manifest Configuration")]
    public class DynamicSDKManifest : ScriptableObject
    {
        [Header("Platform Configuration")]
        [Tooltip("Platform type - typically 'browser' for WebView integration")]
        public string platform = "browser";

        [Tooltip("Client version identifier")]
        public string clientVersion = "1";

        [Tooltip("Environment ID for Dynamic services")]
        public string environmentId = "";

        [Tooltip("App origin domain")]
        public string appOrigin = "appOrigin.com";

        [Tooltip("API base URL for Dynamic services")]
        public string apiBaseUrl = "";

        [Tooltip("URL for app logo")]
        public string appLogoUrl = "";

        [Tooltip("Application name")]
        public string appName = "";

        [Tooltip("CSS overrides for WebView styling")]
        [TextArea(3, 10)]
        public string cssOverrides = "";

        // [Tooltip("Redirect URL after authentication")]
        // public string redirectUrl = "";

        /// <summary>
        /// Convert configuration to JSON string for WebView
        /// </summary>
        public string ToJson()
        {
            var configData = new WebViewConfigData
            {
                platform = this.platform,
                clientVersion = this.clientVersion,
                environmentId = this.environmentId,
                appOrigin = this.appOrigin,
                apiBaseUrl = this.apiBaseUrl,
                appLogoUrl = this.appLogoUrl,
                appName = this.appName,
                cssOverrides = this.cssOverrides,
                // redirectUrl = this.redirectUrl
            };

            return JsonUtility.ToJson(configData);
        }

        /// <summary>
        /// Validate configuration completeness
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(environmentId) &&
                   !string.IsNullOrEmpty(appOrigin) &&
                   !string.IsNullOrEmpty(appName);
        }
    }

    /// <summary>
    /// Serializable data structure for JSON conversion
    /// </summary>
    [System.Serializable]
    public class WebViewConfigData
    {
        public string platform;
        public string clientVersion;
        public string environmentId;
        public string appOrigin;
        public string apiBaseUrl;
        public string appLogoUrl;
        public string appName;
        public string cssOverrides;
        // public string redirectUrl;
    }
}
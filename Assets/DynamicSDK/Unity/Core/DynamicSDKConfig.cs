using UnityEngine;
using UnityEngine.Serialization;

namespace DynamicSDK.Unity.Core
{
    using System.ComponentModel;

    /// <summary>
    /// Centralized configuration for Dynamic SDK
    /// </summary>
    [System.Serializable]
    public class DynamicSDKConfig
    {
        [Header("WebView Settings")]

        private string _startUrl = "http://localhost:3000";

        public string startUrl
        {
            get
            {
                var manifest = Resources.Load<DynamicSDKManifest>("DynamicSDKManifest");
                if (manifest != null)
                {
                    return _startUrl + "?manifest=" + manifest.ToJson();
                }
                return _startUrl;
            }
            set
            {
                _startUrl = value;
            }
        }

        [Range(0.2f, 0.8f)]
        public float heightRatio = 0.6f;

        [Range(0.0f, 0.3f)]
        [Tooltip("Offset from bottom to move webview higher (0 = at bottom, 0.1 = 10% from bottom)")]
        public float bottomOffset = 0f;

        [Header("Unity Editor Safe Area Settings")]
        [Tooltip("Handle Unity Editor safe area automatically (for better simulator testing)")]
        public bool handleEditorSafeAreaAutomatically = true;

        public bool ShowWebViewWhenSigningMessage = false;

        [Range(0.0f, 0.1f)]
        [Tooltip("Additional bottom padding for Unity Editor safe area (0.05 = 5% of screen height)")]
        public float editorSafeAreaBottomPadding = 0.05f;
        
        [Tooltip("Use webview for OAuth in Unity Editor instead of system browser (for simulator testing)")]
        public bool useWebViewForOAuthInEditor = true;

        public float transitionDuration = 0.35f;

        [Tooltip("Allow closing webview by clicking outside of it")]
        public bool enableClickOutsideToClose = true;

        [Tooltip("Pre-load webview in background during SDK initialization for faster first open")]
        public bool enableWebViewPreload = true;

        [Header("UI Colors")]
        public Color connectedColor = Color.green;
        public Color disconnectedColor = Color.red;
        public Color processingColor = Color.yellow;

        [Header("Retry Settings")]
        public int maxRetryAttempts = 3;
        public float retryDelay = 1.0f;

        [Header("Debug Settings")]
        public bool enableDebugLogs = true;
        public bool logRawMessages = false;

        // Static instance for easy access
        private static DynamicSDKConfig _instance;

        public static DynamicSDKConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DynamicSDKConfig();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Load configuration from ScriptableObject if available
        /// </summary>
        public static void LoadFromAsset(DynamicSDKConfig asset)
        {
            if (asset != null)
            {
                _instance = asset;
            }
        }
    }
}
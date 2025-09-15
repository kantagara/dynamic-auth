using UnityEngine;
using DynamicSDK.Unity.Core;

namespace DynamicSDK.Config
{
    /// <summary>
    /// ScriptableObject configuration for Dynamic SDK
    /// Allows easy configuration in Unity Editor
    /// </summary>
    [CreateAssetMenu(fileName = "DynamicSDKConfig", menuName = "DynamicSDK/SDK Configuration")]
    public class DynamicSDKConfigAsset : ScriptableObject
    {
        [Header("Server Settings")]
        [Tooltip("Base URL for the Dynamic SDK React app")]
        public string serverUrl = "https://dynamic-sdk-react-app.vercel.app";
        
        [Header("Mobile Settings")]
        [Tooltip("Use ngrok or public URL for mobile testing")]
        public string mobileServerUrl = "https://dynamic-sdk-react-app.vercel.app";
        
        [Header("WebView Settings")]
        [Range(0.2f, 0.8f)]
        public float heightRatio = 0.6f;
        
        [Range(0.0f, 0.3f)]
        [Tooltip("Offset from bottom to move webview higher")]
        public float bottomOffset = 0f;
        
        [Header("Unity Editor Safe Area Settings")]
        [Tooltip("Handle Unity Editor safe area automatically (for better simulator testing)")]
        public bool handleEditorSafeAreaAutomatically = true;
        
        [Range(0.0f, 0.1f)]
        [Tooltip("Additional bottom padding for Unity Editor safe area")]
        public float editorSafeAreaBottomPadding = 0.05f;
        
        [Tooltip("Use webview for OAuth in Unity Editor instead of system browser (for simulator testing)")]
        public bool useWebViewForOAuthInEditor = true;
        
        public float transitionDuration = 0.35f;
        
        [Tooltip("Allow closing webview by clicking outside")]
        public bool enableClickOutsideToClose = true;
        
        [Tooltip("Pre-load webview in background")]
        public bool enableWebViewPreload = true;
        
        [Header("Debug Settings")]
        public bool enableDebugLogs = true;
        public bool logRawMessages = false;
        
        /// <summary>
        /// Get the appropriate server URL based on platform
        /// </summary>
        public string GetServerUrl()
        {
            #if UNITY_IOS || UNITY_ANDROID
            if (!string.IsNullOrEmpty(mobileServerUrl))
            {
                return mobileServerUrl;
            }
            #endif
            
            return serverUrl;
        }
        
        /// <summary>
        /// Apply this configuration to the DynamicSDKConfig singleton
        /// </summary>
        public void ApplyConfiguration()
        {
            var config = DynamicSDKConfig.Instance;
            config.startUrl = GetServerUrl();
            config.heightRatio = heightRatio;
            config.bottomOffset = bottomOffset;
            config.handleEditorSafeAreaAutomatically = handleEditorSafeAreaAutomatically;
            config.editorSafeAreaBottomPadding = editorSafeAreaBottomPadding;
            config.useWebViewForOAuthInEditor = useWebViewForOAuthInEditor;
            config.transitionDuration = transitionDuration;
            config.enableClickOutsideToClose = enableClickOutsideToClose;
            config.enableWebViewPreload = enableWebViewPreload;
            config.enableDebugLogs = enableDebugLogs;
            config.logRawMessages = logRawMessages;
            
            Debug.Log($"[DynamicSDKConfig] Applied configuration with server URL: {GetServerUrl()}");
            Debug.Log($"[DynamicSDKConfig] Unity Editor Safe Area handling: {handleEditorSafeAreaAutomatically}");
            Debug.Log($"[DynamicSDKConfig] Unity Editor OAuth in WebView: {useWebViewForOAuthInEditor}");
        }
    }
}
using UnityEngine;
using UnityEditor;

public class MobileBuildSettings
{
    [MenuItem("DynamicSDK/Setup Mobile Build")]
    public static void SetupMobileBuild()
    {
        // iOS Settings
        PlayerSettings.iOS.targetOSVersionString = "12.0";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.yourcompany.dynamicunity");
        
        // Android Settings  
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23; // Android 6.0
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33; // Android 13
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.yourcompany.dynamicunity");
        
        // Internet permissions
        PlayerSettings.Android.forceInternetPermission = true;
        
        Debug.Log("[MobileBuildSettings] Mobile build settings configured");
        Debug.Log("iOS Bundle ID: com.yourcompany.dynamicunity");
        Debug.Log("Android Package: com.yourcompany.dynamicunity");
        Debug.Log("Deeplink scheme: dynamicunity://");
    }
}
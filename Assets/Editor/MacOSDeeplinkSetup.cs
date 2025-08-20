#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public class MacOSDeeplinkSetup
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.StandaloneOSX)
        {
            // Path to Info.plist
            string plistPath = path + "/Contents/Info.plist";
            
            // Read existing plist
            string plistContent = File.ReadAllText(plistPath);
            
            // Check if URL types already exists
            if (!plistContent.Contains("CFBundleURLTypes"))
            {
                // Add URL scheme for deeplink
                string urlScheme = @"
    <key>CFBundleURLTypes</key>
    <array>
        <dict>
            <key>CFBundleURLName</key>
            <string>com.yourcompany.dynamicunity</string>
            <key>CFBundleURLSchemes</key>
            <array>
                <string>dynamicunity</string>
            </array>
        </dict>
    </array>";
                
                // Insert before closing </dict>
                int insertIndex = plistContent.LastIndexOf("</dict>");
                plistContent = plistContent.Insert(insertIndex, urlScheme);
                
                // Write back
                File.WriteAllText(plistPath, plistContent);
                
                Debug.Log("[MacOSDeeplinkSetup] Added URL scheme 'dynamicunity' to Info.plist");
            }
            else
            {
                Debug.Log("[MacOSDeeplinkSetup] URL scheme already exists in Info.plist");
            }
        }
    }
}
#endif
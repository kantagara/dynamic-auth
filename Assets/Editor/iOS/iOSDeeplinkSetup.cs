#if UNITY_IOS
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using UnityEditor.Build;

public class iOSDeeplinkSetup
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.iOS)
        {
            var targetGroup = BuildPipeline.GetBuildTargetGroup(target);
            var namedTargetGroup = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
            var appId = PlayerSettings.GetApplicationIdentifier(namedTargetGroup);

            // Setup Info.plist
            string plistPath = path + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            
            PlistElementDict rootDict = plist.root;
            
            // Add URL schemes for deeplink
            PlistElementArray urlTypes = rootDict.CreateArray("CFBundleURLTypes");
            PlistElementDict urlScheme = urlTypes.AddDict();
            urlScheme.SetString("CFBundleURLName", appId);
            
            PlistElementArray schemes = urlScheme.CreateArray("CFBundleURLSchemes");
            schemes.AddString("dynamicunity");
            
            // Add LSApplicationQueriesSchemes for checking if apps are installed
            PlistElementArray queriesSchemes = rootDict.CreateArray("LSApplicationQueriesSchemes");
            queriesSchemes.AddString("dynamicunity");
            
            // Write back
            plist.WriteToFile(plistPath);
            
            Debug.Log("[iOSDeeplinkSetup] Added URL scheme 'dynamicunity' to Info.plist");
        }
    }
}
#endif
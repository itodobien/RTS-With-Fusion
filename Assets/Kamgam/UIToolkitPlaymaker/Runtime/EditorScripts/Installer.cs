#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kamgam.UIToolkitPlaymaker
{
    public class Installer
    {
        public const string Version = "1.1.0";
        public const string Define = "PLAYMAKER";
        public const string ManualUrl = "https://kamgam.com/unity/UIToolkitPlaymakerManual.pdf";

        public static string AssetRootPath = "Assets/Kamgam/UIToolkitPlaymaker/";


        public static Version GetVersion() => new Version(Version);

        [UnityEditor.Callbacks.DidReloadScripts(998001)]
        public static void InstallIfNeeded()
        {
            bool versionChanged = VersionHelper.UpgradeVersion(GetVersion, out Version oldVersion, out Version newVersion);
            if (versionChanged)
            {
                if (versionChanged)
                {
                    Debug.Log("UIToolkitPlaymaker Version changed from " + oldVersion + " to " + newVersion);
                    showWelcomeMessage();
                }
            }
        }

        static void showWelcomeMessage()
        {
            bool openExample = EditorUtility.DisplayDialog(
                "UI Toolkit Playmaker",
                "Thank you for choosing UI Toolkit Playmaker.\n\n" +
                "Please start by reading the manual.\n\n" +
                "If you can find the time I would appreciate your feedback in the form of a review.\n\n" +
                "I have prepared some examples for you.",
                "Open Example", "Open manual (web)"
                );

#if !PLAYMAKER
            EditorApplication.delayCall += () =>
            {
                EditorUtility.DisplayDialog(
                    "Playmaker is not installed!",
                    "This asset only works if Playmaker is installed. Please install it from the Asset Store. The demo may cause errors if Playmaker is not installed.",
                    "Understood"
                    );
            };
#endif

            if (openExample)
                OpenExample();
            else
                OpenManual();

        }


        [MenuItem("Tools/UIToolkit Playmaker/Manual", priority = 101)]
        public static void OpenManual()
        {
            Application.OpenURL(ManualUrl);
        }

        [MenuItem("Tools/UIToolkit Playmaker/Open Example Scene", priority = 103)]
        public static void OpenExample()
        {
            EditorApplication.delayCall += () =>
            {
                string path = "Assets/Kamgam/UIToolkitPlaymaker/Examples/ButtonClickDemo/ButtonClickDemo.unity";
                var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                EditorGUIUtility.PingObject(scene);
                EditorSceneManager.OpenScene(path);
            };
        }

        [MenuItem("Tools/UIToolkit Playmaker/Please leave a review :-)", priority = 510)]
        public static void LeaveReview()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/slug/264903");
        }

        [MenuItem("Tools/UIToolkit Playmaker/More Asset by KAMGAM", priority = 511)]
        public static void MoreAssets()
        {
            Application.OpenURL("https://kamgam.com/unity");
        }

        [MenuItem("Tools/UIToolkit Playmaker/Version " + Version, priority = 512)]
        public static void LogVersion()
        {
            Debug.Log("UIToolkit Playmaker v" + Version + ", Unity: " + Application.unityVersion);
        }
    }
}
#endif
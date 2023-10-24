using System.IO;
using UnityEditor;
using UnityEngine;

namespace ubc.ok.ovilab.roadmap.editor
{
    [InitializeOnLoad]
    public class RoadmapSetup
    {
        public const string keystoreName = "UserSettings/user.keystore";
        public static string keystoreFullPathLocation;

        // initializeonload static constructor method
        static RoadmapSetup()
        {
            keystoreFullPathLocation = Path.GetFullPath(Path.Combine(Application.dataPath, "../", RoadmapSetup.keystoreName));
            Update();
        }

        public static void Update()
        {
            PlayerSettings.Android.keystoreName = keystoreName;
            PlayerSettings.Android.keystorePass = RoadmapSettings.instance.keystorePass;
            PlayerSettings.Android.keyaliasPass = RoadmapSettings.instance.keyAliasPass;
        }
    }
}

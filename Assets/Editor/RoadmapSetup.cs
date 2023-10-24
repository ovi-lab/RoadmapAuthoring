using System.IO;
using System.Linq;
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

        public static void EnsureConfigLoaded()
        {
            if (RoadmapSettings.instance.activeConfig == null)
            {
                throw new System.Exception("The active RoadmapApplicationConfig is not set! Select an RoadmapApplicationConfig instance and click `Make this the active config`.");
            }
            else
            {
                // Making sure only the active settings is pre-loaded
                var assets = UnityEditor.PlayerSettings.GetPreloadedAssets().Where(obj => obj != null && !(obj is RoadmapApplicationConfig)).ToList();
                assets.Add(RoadmapSettings.instance.activeConfig);
                UnityEditor.PlayerSettings.SetPreloadedAssets(assets.ToArray());
            }
        }

        public static void Update()
        {
            PlayerSettings.Android.keystoreName = keystoreName;
            PlayerSettings.Android.keystorePass = RoadmapSettings.instance.keystorePass;
            PlayerSettings.Android.keyaliasPass = RoadmapSettings.instance.keyAliasPass;

            EnsureConfigLoaded();
            EditorApplication.playModeStateChanged += (state) =>
            {
                if (state == PlayModeStateChange.ExitingEditMode)
                {
                    EnsureConfigLoaded();
                }
            };
        }
    }
}

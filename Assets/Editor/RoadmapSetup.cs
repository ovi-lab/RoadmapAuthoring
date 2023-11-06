using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.SceneManagement;
using UnityEditor.SceneTemplate;
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
            ValidateVersionChange();
        }

        public static void EnsureConfigLoaded()
        {
            if (RoadmapSettings.instance.activeConfig == null)
            {
                Debug.LogError("The active RoadmapApplicationConfig is not set! Select an RoadmapApplicationConfig instance and click `Make this the active config`.");
            }
            else
            {
                // Making sure only the active settings is pre-loaded
                var assets = UnityEditor.PlayerSettings.GetPreloadedAssets().Where(obj => obj != null && !(obj is RoadmapApplicationConfig)).ToList();
                assets.Add(RoadmapSettings.instance.activeConfig);
                UnityEditor.PlayerSettings.SetPreloadedAssets(assets.ToArray());
            }
        }

        public static void EnsureScenesCreated(bool removeOld=false)
        {
            string applicationDataPath = Path.Join(Application.dataPath, "..");

            if (removeOld)
            {
                string scenePath = Path.Join(applicationDataPath, RoadmapSettings.AR_Scene);
                if (File.Exists(scenePath))
                {
                    UnityEngine.Debug.Log($"Deleting {scenePath}");
                    File.Delete(scenePath);
                }
                scenePath = Path.Join(applicationDataPath, RoadmapSettings.VR_Scene);
                if (File.Exists(scenePath))
                {
                    UnityEngine.Debug.Log($"Deleting {scenePath}");
                    File.Delete(scenePath);
                }
            }

            if (!File.Exists(Path.Join(applicationDataPath, RoadmapSettings.AR_Scene)) || !File.Exists(Path.Join(applicationDataPath, RoadmapSettings.VR_Scene)))
            {
                SceneSetup[] sceneSetups = EditorSceneManager.GetSceneManagerSetup();

                try
                {
                    SceneTemplateService.Instantiate(AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>(RoadmapSettings.AR_SceneTemplate), false, RoadmapSettings.AR_Scene);
                    UnityEngine.Debug.Log($"Generated build AR scene.");
                    SceneTemplateService.Instantiate(AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>(RoadmapSettings.VR_SceneTemplate), false, RoadmapSettings.VR_Scene);
                    UnityEngine.Debug.Log($"Generated build VR scene.");
                }
                catch
                { }

                EditorSceneManager.RestoreSceneManagerSetup(sceneSetups);
            }
        }

        private static void ValidateVersionChange()
        {
            ListRequest listRequest = Client.List();
            while (!listRequest.IsCompleted)
            {
                Thread.Sleep(100);
            }
            
            if (listRequest.IsCompleted)
            {
                UnityEditor.PackageManager.PackageInfo packageInfo = listRequest.Result.FirstOrDefault(q => q.name == "ubc.ok.ovilab.roadmapauthoring");
                if (packageInfo == null)
                {
                    Debug.LogError($"Roadmap validating version change failing.");
                }
                else
                {
                    if (RoadmapSettings.instance.installedVersion != packageInfo.version)
                    {
                        Debug.Log($"Version changed from {RoadmapSettings.instance.installedVersion} to {packageInfo.version}");
                        RoadmapSettings.instance.installedVersion = packageInfo.version;
                        Update();
                        EnsureScenesCreated(true);
                    }
                }
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

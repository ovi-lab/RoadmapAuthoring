using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ubc.ok.ovilab.roadmap
{
    public class RoadmapBuildSetupEditor: EditorWindow
    {
        private static Dictionary<Platform, string> platformLoaderNames = new Dictionary<Platform, string> {
            {Platform.ARCore, "UnityEngine.XR.ARCore.ARCoreLoader"},
            {Platform.Oculus, null},
        };

        private string buildPath = "Builds/build.apk";

        void OnEnable()
        {
        }

        void OnDisable()
        {
        }

        [MenuItem("Roadmap/Build and run", false, 100)]
        static void Init()
        {
            EditorWindow.GetWindow<RoadmapBuildSetupEditor>(false, "Roadmap Build Setup", true);
        }

        // [MenuItem("Roadmap/Setup URP default assets", false, 100)]
        // static void test()
        // {
        //     UniversalRenderPipelineAsset sqAsset = (UniversalRenderPipelineAsset) AssetDatabase.LoadAssetAtPath("Packages/ubc.ok.hcilab.roadmap-unity/Assets/Essentials/Settings/UniversalRenderPipelineAsset_StandardQuality.asset", typeof(UniversalRenderPipelineAsset));
        //     QualitySettings.renderPipeline = sqAsset;
        //     GraphicsSettings.defaultRenderPipeline = sqAsset;

        //     // FIXME: This forces the creation of the UniversalRenderPipelineGlobalSettings? Couldn't figure out anoahter way to to this.
        //     new UniversalRenderPipeline(sqAsset);
        // }

        private void OnGUI()
        {
            // TODO: Make these static values
            SceneAsset oculusScene = (SceneAsset) EditorGUILayout.ObjectField("Oculus Scene", RoadmapSettings.instance.oculusScene, typeof(SceneAsset), false);
            SceneAsset arcoreScene = (SceneAsset) EditorGUILayout.ObjectField("ARCore Scene", RoadmapSettings.instance.arcoreScene, typeof(SceneAsset), false);

            if (oculusScene != RoadmapSettings.instance.oculusScene)
            {
                RoadmapSettings.instance.oculusScene = oculusScene;
                RoadmapSettings.instance.Save();
            }
            if (arcoreScene != RoadmapSettings.instance.arcoreScene)
            {
                RoadmapSettings.instance.arcoreScene = arcoreScene;
                RoadmapSettings.instance.Save();
            }

            buildPath = EditorGUILayout.TextField("Built APK Path", buildPath);

            Platform prevPlatform = RoadmapSettings.instance.CurrentPlatform();
            Platform currentPlatform = (Platform) EditorGUILayout.EnumPopup("Target platform", prevPlatform);
            if (prevPlatform != currentPlatform)
            {
                RoadmapSettings.instance.SetPlatformm(currentPlatform);
                switch(currentPlatform)
                {
                    case Platform.Oculus:
                        OculusSettings();
                        break;
                    case Platform.ARCore:
                        ARCoreSettings();
                        break;
                }
            }

            if (GUILayout.Button("Start build"))
            {
                UnityEngine.Debug.Log("Starting Build!");
                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
                switch(currentPlatform)
                {
                    case Platform.Oculus:
                        OculusSettings();
                        buildPlayerOptions.scenes = new[] { AssetDatabase.GetAssetPath(RoadmapSettings.instance.oculusScene) };
                        break;
                    case Platform.ARCore:
                        ARCoreSettings();
                        buildPlayerOptions.scenes = new[] { AssetDatabase.GetAssetPath(RoadmapSettings.instance.arcoreScene) };
                        break;
                }
                buildPlayerOptions.options = BuildOptions.None;
                buildPlayerOptions.target = BuildTarget.Android;
                buildPlayerOptions.locationPathName = buildPath;
                BuildPipeline.BuildPlayer(buildPlayerOptions);
            }

            if (GUILayout.Button("Deploy"))
            {
                DeployAPK();
            }
        }

        private void OculusSettings()
        {
            SetXRLoader(Platform.Oculus);
            ActivateScene(RoadmapSettings.instance.oculusScene);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        }

        private void ARCoreSettings()
        {
            SetXRLoader(Platform.ARCore);
            ActivateScene(RoadmapSettings.instance.arcoreScene);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        }

        private static void SetXRLoader(Platform platform)
        {
            // See https://forum.unity.com/threads/editor-programmatically-set-the-vr-system-in-xr-plugin-management.972285/
            XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out buildTargetSettings);
            XRGeneralSettings settings = buildTargetSettings.SettingsForBuildTarget(BuildTargetGroup.Android);
            foreach (var keyValuePair in platformLoaderNames)
            {
                if (keyValuePair.Key == platform && !string.IsNullOrEmpty(keyValuePair.Value))
                {
                    XRPackageMetadataStore.AssignLoader(settings.Manager, keyValuePair.Value, BuildTargetGroup.Android);
                }
                else
                {
                    XRPackageMetadataStore.RemoveLoader(settings.Manager, keyValuePair.Value, BuildTargetGroup.Android);
                }
            }
            EditorUtility.SetDirty(settings);
        }

        private void ActivateScene(SceneAsset sceneAsset)
        {
            if (sceneAsset == null)
            {
                throw new Exception("Respective scene is empty.");
            }

            string scenepath = AssetDatabase.GetAssetPath(sceneAsset);
            bool found = false;

            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();

            foreach (var editorScene in scenes)
            {
                if (editorScene.path != scenepath)
                {
                    editorScene.enabled = false;
                }
                else
                {
                    found = true;
                    editorScene.enabled = true;
                }
            }

            if (!found)
            {
                scenes.Add(new EditorBuildSettingsScene(scenepath, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private void DeployAPK()
        {
            /// Heavily borrowed from OVR's scripts
            string androidSdkRoot = "";

            bool useEmbedded = EditorPrefs.GetBool("SdkUseEmbedded") || string.IsNullOrEmpty(EditorPrefs.GetString("AndroidSdkRoot"));
            if (useEmbedded)
            {
                androidSdkRoot = Path.Combine(BuildPipeline.GetPlaybackEngineDirectory(BuildTarget.Android, BuildOptions.None), "SDK");
            }
            androidSdkRoot = androidSdkRoot.Replace("/", "\\");

            if (string.IsNullOrEmpty(androidSdkRoot))
            {
                UnityEngine.Debug.LogError("Android SDK root not found");
            }
            
            if (androidSdkRoot.EndsWith("\\") || androidSdkRoot.EndsWith("/"))
            {
                androidSdkRoot = androidSdkRoot.Remove(androidSdkRoot.Length - 1);
            }
            string androidPlatformToolsPath = Path.Combine(androidSdkRoot, "platform-tools");
            string adbPath = Path.Combine(androidPlatformToolsPath, "adb.exe");

            string buildFilePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), buildPath);
            UnityEngine.Debug.Log("Deploying :" + buildFilePath);
            ProcessStartInfo startInfo = new ProcessStartInfo(adbPath, $"install {buildFilePath}");
            startInfo.WorkingDirectory = androidSdkRoot;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            RunProcess(startInfo);
        }

        public static void RunProcess(ProcessStartInfo startInfo)
        {
            StringBuilder outputStringBuilder = new StringBuilder("");
            StringBuilder errorStringBuilder = new StringBuilder("");

            Process process = Process.Start(startInfo);
            process.OutputDataReceived += new DataReceivedEventHandler((object sendingProcess, DataReceivedEventArgs args) =>
            {
                // Collect the sort command output.
                if (!string.IsNullOrEmpty(args.Data))
                {
                    // Add the text to the collected output.
                    outputStringBuilder.Append(args.Data);
                    outputStringBuilder.Append(Environment.NewLine);
                };
            });
            process.ErrorDataReceived += new DataReceivedEventHandler((object sendingProcess, DataReceivedEventArgs args) =>
            {
                    // Collect the sort command output.
                    if (!string.IsNullOrEmpty(args.Data))
                {
                        // Add the text to the collected output.
                        errorStringBuilder.Append(args.Data);
                    errorStringBuilder.Append(Environment.NewLine);
                }
            });

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            try
            {
                do {} while (!process.WaitForExit(100));

                process.WaitForExit();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarningFormat("exception {0}", e.Message);
            }

            int exitCode = process.ExitCode;

            process.Close();

            string outputString = outputStringBuilder.ToString();
            string errorString = errorStringBuilder.ToString();

            outputStringBuilder = null;
            errorStringBuilder = null;

            UnityEngine.Debug.Log($"Ran:: {startInfo.FileName} {startInfo.Arguments}");
            if (!string.IsNullOrEmpty(errorString))
            {
                if (errorString.Contains("Warning"))
                {
                    UnityEngine.Debug.LogWarning(errorString);
                }
                else
                {
                    UnityEngine.Debug.LogError(errorString);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(outputString))
                {
                    UnityEngine.Debug.Log(outputString);
                }
                UnityEngine.Debug.Log("Done!");
            }
        }
    }
}

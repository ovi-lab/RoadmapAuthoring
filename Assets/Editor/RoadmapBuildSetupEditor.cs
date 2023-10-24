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

namespace ubc.ok.ovilab.roadmap.editor
{
    public class RoadmapBuildSetupEditor: EditorWindow
    {
        private static Dictionary<Platform, string> platformLoaderNames = new Dictionary<Platform, string> {
            {Platform.ARCore, "UnityEngine.XR.ARCore.ARCoreLoader"},
            {Platform.Oculus, null},
        };

        private string buildPath;
        private bool showKeystoreSettings;
        private string keystorePass;
        private string keyAliasPass;

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
            SceneAsset arScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(RoadmapSettings.AR_Scene);
            SceneAsset vrScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(RoadmapSettings.VR_Scene);

            if (vrScene != RoadmapSettings.instance.vrScene)
            {
                RoadmapSettings.instance.vrScene = vrScene;
                RoadmapSettings.instance.Save();
            }
            if (arScene != RoadmapSettings.instance.arScene)
            {
                RoadmapSettings.instance.arScene = arScene;
                RoadmapSettings.instance.Save();
            }

            Platform prevPlatform = RoadmapSettings.instance.CurrentPlatform();
            Platform currentPlatform = (Platform) EditorGUILayout.EnumPopup("Target platform", prevPlatform);
            if (prevPlatform != currentPlatform)
            {
                RoadmapSettings.instance.SetPlatformm(currentPlatform);
                switch(currentPlatform)
                {
                    case Platform.Oculus:
                        buildPath = RoadmapSettings.VR_build_path;
                        VRSettings();
                        break;
                    case Platform.ARCore:
                        buildPath = RoadmapSettings.AR_build_path;
                        ARSettings();
                        break;
                }
            }

            EditorGUILayout.Space();
            if (RoadmapSettings.instance.activeConfig == null)
            {
                EditorGUILayout.HelpBox("The active RoadmapApplicationConfig is not set! Select an RoadmapApplicationConfig instance and click `Make this the active config`.", MessageType.Error);
            }
            else
            {
                EditorGUILayout.LabelField("Active config:", AssetDatabase.GetAssetPath(RoadmapSettings.instance.activeConfig.GetInstanceID()));
            }
            if (GUILayout.Button(new GUIContent("Refresh active config", "In case the active config wasn't properly loaded in."), GUILayout.Width(130)))
            {
                RoadmapSetup.EnsureConfigLoaded();
            }
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Built APK Path:", buildPath);

            EditorGUILayout.Space();

            // Keystore related settings
            if (!System.IO.File.Exists(RoadmapSetup.keystoreFullPathLocation))
            {
                EditorGUILayout.HelpBox($"The keystore is missing. It is expected to be in {RoadmapSetup.keystoreFullPathLocation}", MessageType.Error);
            }
            showKeystoreSettings = EditorGUILayout.Foldout(showKeystoreSettings, "Keystore settings");
            if (showKeystoreSettings)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.indentLevel++;
                keystorePass = EditorGUILayout.PasswordField("keystore pass", keystorePass);
                keyAliasPass = EditorGUILayout.PasswordField("keyAlias pass", keyAliasPass);
                EditorGUI.indentLevel--;
                if (EditorGUI.EndChangeCheck())
                {
                    RoadmapSettings.instance.keystorePass = keystorePass;
                    RoadmapSettings.instance.keyAliasPass = keyAliasPass;
                    RoadmapSettings.instance.Save();
                    RoadmapSetup.Update();
                }
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Start build"))
            {
                UnityEngine.Debug.Log("Starting Build!");

                RoadmapSetup.EnsureConfigLoaded();

                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
                switch(currentPlatform)
                {
                    case Platform.Oculus:
                        VRSettings();
                        buildPlayerOptions.scenes = new[] { AssetDatabase.GetAssetPath(RoadmapSettings.instance.vrScene) };
                        break;
                    case Platform.ARCore:
                        ARSettings();
                        buildPlayerOptions.scenes = new[] { AssetDatabase.GetAssetPath(RoadmapSettings.instance.arScene) };
                        break;
                }
                buildPlayerOptions.options = BuildOptions.None;
                buildPlayerOptions.target = BuildTarget.Android;
                buildPlayerOptions.locationPathName = buildPath;

                GooglePlayServices.PlayServicesResolver.MenuForceResolve();

                BuildPipeline.BuildPlayer(buildPlayerOptions);
            }

            if (GUILayout.Button("Deploy"))
            {
                DeployAPK();
            }
        }

        private void VRSettings()
        {
            SetXRLoader(Platform.Oculus);
            ActivateScene(RoadmapSettings.instance.vrScene);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        }

        private void ARSettings()
        {
            SetXRLoader(Platform.ARCore);
            ActivateScene(RoadmapSettings.instance.arScene);
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

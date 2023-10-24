using System;
using UnityEditor;
using UnityEngine;

namespace ubc.ok.ovilab.roadmap.editor
{
    [FilePath("UserSettings/Roadmap.state", FilePathAttribute.Location.ProjectFolder)]
    public class RoadmapSettings : ScriptableSingleton<RoadmapSettings>
    {
        public const string AR_Scene = "Packages/ubc.ok.ovilab.roadmapauthoring/Assets/Scenes/AR_Scene.unity";
        public const string VR_Scene = "Packages/ubc.ok.ovilab.roadmapauthoring/Assets/Scenes/VR_Scene.unity";
        public const string AR_build_path = "Builds/ar_build.apk";
        public const string VR_build_path = "Builds/vr_build.apk";

        [SerializeField]
        string targetPlatform = Platform.Oculus.ToString();
        [SerializeField] public SceneAsset vrScene;
        [SerializeField] public SceneAsset arScene;
        [SerializeField] public string groupID;
        [SerializeField] public string keystorePass;
        [SerializeField] public string keyAliasPass;
        [SerializeField] public RoadmapApplicationConfig activeConfig;

        public void SetPlatformm(Platform platform)
        {
            targetPlatform = platform.ToString();
            Save(true);
        }

        public Platform CurrentPlatform()
        {
            return Enum.Parse<Platform>(targetPlatform);
        }

        public void Save()
        {
            Save(true);
        }
    }
}

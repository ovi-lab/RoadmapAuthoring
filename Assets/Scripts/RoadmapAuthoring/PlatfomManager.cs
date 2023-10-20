using UnityEngine;

namespace ubc.ok.ovilab.roadmap
{
    public enum Platform { Oculus, ARCore }

    /// <summary>
    /// Singleton Class
    /// Handles switching between different platforms. Depending on which paltform is detected
    /// the appropriate components will be configured and used.
    /// </summary>
    [DefaultExecutionOrder(-100)] // We want all the configs done before any related sripts are executed
    public class PlatformManager : Singleton<PlatformManager>
    {
        [SerializeField] public Platform currentPlatform;
    }
}

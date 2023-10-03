using System;
using UnityEngine;
using UnityEngine.Events;

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

        [SerializeField] public Camera oculusCamera;
        [SerializeField] public Camera arCoreCamera;

        [SerializeField] private GameObject oculusMenu;
        [SerializeField] private GameObject arCoreMenu;

        [SerializeField] private GameObject platformSpecificManagerObject;

        public UnityEvent OculusDetected;
        public UnityEvent ARCoreDetected;

        void Start()
        {
            // currentPlatform = DetectPlatform();
            switch (currentPlatform)
            {
                case Platform.ARCore:
                    SetupARCore();
                    // Do any setup to deactivate other platforms
                    SetupOculus(isActive: false);
                    // Call events attached through the UI
                    ARCoreDetected?.Invoke();
                    break;
                case Platform.Oculus:
                    SetupOculus();
                    // Do any setup to deactivate other platforms
                    SetupARCore(isActive: false);
                    // Call events attached through the UI
                    OculusDetected?.Invoke();
                    break;
            }
        }

        // private Platform DetectPlatform()
        // {
        //     return Platform.Oculus;
        // }

        private void SetupARCore(bool isActive=true)
        {
        }

        private void SetupOculus(bool isActive=true)
        {
        }
    }
}

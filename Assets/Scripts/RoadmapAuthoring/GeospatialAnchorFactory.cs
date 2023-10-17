using System;
using Google.XR.ARCoreExtensions;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ubc.ok.ovilab.roadmap
{
    /// <summary>
    /// Factory to generate groups.
    /// </summary>
    public class GeospatialAnchorFactory : GroupAnchorFactory
    {
        [SerializeField] private AREarthManager earthManager;
        [SerializeField] private ARAnchorManager anchorManager;
        [SerializeField] private ARCoreExtensions arCoreExtensions;
        [Header("[ Accuracy Minimums ] - Required to start experience")]
        [SerializeField] private bool checkMinimums = false;
        [SerializeField] private float minimumHorizontalAccuracy = 10;//10
        [SerializeField] private float minimumOrientationAccuracy = 10;//15
        [SerializeField] private float minimumVerticalAccuracy = 10.5f;//1.5f

        private bool trackingIsValid = false,
            requestCamPerm,
            requestLocPerm,
            enablingGeospatial,
            startedAR;
        private float initTime = 3f;

        /// <summary>
        /// Initializes a new instance of PlaceblesGroup and return it."
        /// </summary>
        public override PlaceablesGroup GetPlaceablesGroup(GroupData groupData, System.Action<PlaceableObject> onClickedCallback)
        {
            GameObject groupObject = null;

            if (groupData != null)
            {
                Quaternion quaternion = Quaternion.AngleAxis((float)groupData.Heading, Vector3.up);
                groupObject = anchorManager.AddAnchor(groupData.Latitude, groupData.Longitude, groupData.Altitude, quaternion)?.gameObject;
            }
            else
            {
                GeospatialPose pose = earthManager.CameraGeospatialPose;
                // Get forward from the EunRotation then project that on the xz plane. Use that to get rotation around y axis
                Quaternion quaternion = Quaternion.LookRotation(Vector3.ProjectOnPlane(pose.EunRotation * Vector3.forward, Vector3.up), Vector3.up);
                groupObject = anchorManager.AddAnchor(pose.Latitude, pose.Longitude, pose.Altitude, quaternion)?.gameObject;
            }

            if (groupObject == null)
            {
                throw new Exception("Failed to initilize group");
            }

            PlaceablesGroup placeablesGroup = groupObject.AddComponent<PlaceablesGroup>();
            placeablesGroup.Init(groupData, onClickedCallback);
            return placeablesGroup;
        }

        /// <summary>
        /// Ensure we have Camera usage permission
        /// </summary>
        /// <returns></returns>
        private bool CheckCameraPermission()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                if (!requestCamPerm)
                {
                    Permission.RequestUserPermission(Permission.Camera);
                }
                requestCamPerm = true;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Ensure we have Location usage permission
        /// </summary>
        /// <returns></returns>
        private bool CheckLocationPermission()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                if (!requestLocPerm)
                {
                    Permission.RequestUserPermission(Permission.FineLocation);
                }
                requestLocPerm = true;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns whether or not both conditions are true:
        /// <list type="bullet">
        /// <item> Earth Manager is tracking correctly</item>
        /// <item> Current accuracy meets the specified minimums</item>
        /// </list>
        /// Sets error state appropriately.
        /// </summary>
        private bool TrackingIsValid()
        {
            if (earthManager.EarthTrackingState == TrackingState.Tracking)
            {
                if (!checkMinimums)
                {
                    return true;
                }
                /// Have we met the minimums?
                return earthManager.CameraGeospatialPose.OrientationYawAccuracy <= minimumOrientationAccuracy &&
                    earthManager.CameraGeospatialPose.VerticalAccuracy <= minimumVerticalAccuracy &&
                    earthManager.CameraGeospatialPose.HorizontalAccuracy <= minimumHorizontalAccuracy;
            }
            return false;
        }

        // Unity method
        protected override void Update()
        {
            if (!CheckCameraPermission())
            {
                DebugMessages.Instance.LogToDebugText($"Check camera permission failed");
                return;
            }
            
            if (!CheckLocationPermission())
            {
                DebugMessages.Instance.LogToDebugText($"Check location permission failed");
                return;
            }

            if (ARSession.state != ARSessionState.SessionInitializing &&
                ARSession.state != ARSessionState.SessionTracking)
            {
                return;
            }

            if (!startedAR)
            {
                DebugMessages.Instance.LogToDebugText($"ARSession started");
                arCoreExtensions.gameObject.SetActive(true);
                arCoreExtensions.enabled = true;
                startedAR = true;
            }

            FeatureSupported featureSupport = earthManager.IsGeospatialModeSupported(GeospatialMode.Enabled);

            switch (featureSupport)
            {
                case FeatureSupported.Unknown:
                    DebugMessages.Instance.LogToDebugText("Geospatial API encountered an unknown error.");
                    return;
                case FeatureSupported.Unsupported:
                    DebugMessages.Instance.LogToDebugText("Geospatial API is not supported by this device.");
                    enabled = false;
                    return;
                case FeatureSupported.Supported:
                    if (arCoreExtensions.ARCoreExtensionsConfig.GeospatialMode == GeospatialMode.Disabled)
                    {
                        DebugMessages.Instance.LogToDebugText("Enabling Geospatial Mode...");
                        arCoreExtensions.ARCoreExtensionsConfig.GeospatialMode =
                            GeospatialMode.Enabled;
                        enablingGeospatial = true;
                        return;
                    }
                    break;
            }

            // Waiting for new configuration taking effect
            if (enablingGeospatial)
            {
                initTime -= Time.deltaTime;

                if (initTime < 0)
                {
                    DebugMessages.Instance.LogToDebugText($"Enabling geo spatial");
                    enablingGeospatial = false;
                }
                else
                {
                    return;
                }
            }

            // Check earth state
            EarthState earthState = earthManager.EarthState;

            if (earthState != EarthState.Enabled)
            {
                DebugMessages.Instance.LogToDebugText($"Earth state not enabled  {earthState}");
                if (earthState != EarthState.ErrorEarthNotReady)
                {
                    DebugMessages.Instance.LogToDebugText($"Disabling app!");
                    enabled = false;
                }
                return;
            }

            if (!trackingIsValid)
            {
                trackingIsValid = TrackingIsValid();
                if (trackingIsValid)
                {
                    DebugMessages.Instance.LogToDebugText("Tracking is valid");
                }
            }
            
            if (trackingIsValid)
            {
                // Execute all actions that have been queued.
                RunAllActions();
            }
        }
    }
}

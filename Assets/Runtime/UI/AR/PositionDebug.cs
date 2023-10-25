using TMPro;
using UnityEngine;
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

namespace ubc.ok.ovilab.roadmap
{
    public class PositionDebug : MonoBehaviour
    {
        [SerializeField] private AREarthManager earthManager;
        [SerializeField] private GroupManager groupManager;
        public TextMeshProUGUI debugText;

        private void Start()
        {
            if (debugText == null)
            {
                debugText = GetComponent<TextMeshProUGUI>();
            }
        }

        public void Update()
        {
            if (debugText != null)
            {
                var earthTrackingState = earthManager.EarthTrackingState;
                if (earthTrackingState == TrackingState.Tracking)
                {
                    // camera_geospatial_pose contains geodetic location, rotation, and
                    // confidences values.
                    GeospatialPose cameraGeospatialPose = earthManager.CameraGeospatialPose;
                    Vector3 distToCenter = groupManager.GetPlaceablesGroup("center").transform.position - Camera.main.transform.position;
                    Vector3 distToTims = groupManager.GetPlaceablesGroup("N-tims-lake").transform.position - Camera.main.transform.position;
                    List<PlaceableObject> data = groupManager.GetAllPlaceableObjects();

                    string other = "--";
                    if (data.Count > 0)
                    {
                        other = $"obj ({data[0].GetPlaceableObjectData().identifier}) - {(data[0].transform.position - Camera.main.transform.position).magnitude} ({data[0].transform.position})";
                    }

                    debugText.text = $"lat {cameraGeospatialPose.Latitude} lon {cameraGeospatialPose.Longitude}\n pos {Camera.main.transform.position}\n"+
                        $"dist to center: {distToCenter.magnitude} ({distToCenter})\n"+
                        $"dist to center: {distToTims.magnitude} ({distToTims})\n"+
                        $"\n {other}";
                }
            }
        }

        public void SpawnInFrontOfCamer()
        {
            Transform t = Camera.main.transform;
            Debug.Log($"SPAWNING at Camera at {t.position}  on {t.name}");
            Vector3 newPosition = t.position + t.forward.normalized * 1.5f;
            PlaceableObject placeableObject = PlaceableObject.SetupPlaceableObject("cofeecup44", "", null, 10000000);

            placeableObject.SetLocalPose(newPosition, Quaternion.identity, Vector3.one);
            placeableObject.SetObjectManipulationEnabled(true);
        }
    }
}

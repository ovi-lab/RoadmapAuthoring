using System;
using System.Collections.Generic;
using UnityEngine;
using Google.XR.ARCoreExtensions;

namespace ubc.ok.ovilab.roadmap
{
    /// <summary>
    /// A Gameobject/Transform that serves as a parent for grouping Placeable Objects.
    /// This object will ask for and align itself to Geospatial Anchors, and keep record of any child Placeable Objects.
    /// During the session it is first created, its world position is flexible; it will align itself to a new Geospatial Anchor any time tracking accuracy increases.
    /// By grouping Placeables under a single parent anchor, we can maintain their positions relative to each other better than if each Placeable was subject to individual Geo anchor accuracy.
    /// </summary>
    public class PlaceablesGroup : MonoBehaviour
    {
        private List<PlaceableObject> placeableObjects;

        // TODO platform specific details goes here?
        public void Init(GroupData data=null)
        {
            placeableObjects = new List<PlaceableObject>();

            if (data != null)
            {
                transform.position = new Vector3((float)data.Longitude, (float)data.Altitude, (float)data.Latitude);
                // -z is the north!
                transform.rotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
                transform.Rotate(Vector3.up, (float)data.Heading);

                foreach(PlaceableObjectData objectData in data.PlaceableDataList)
                {
                    PlaceableObject placeableObject = AddPlaceableObject(objectData.PrefabIdentifier, objectData.localPosition, objectData.localRotation);
                }
            }
            else
            {
                /// -z is north, x is west
                /// heading is the camera's forward vector projected on the xz plane (y is the plane normal vector)
                // Heading not needed in the VR scene
                // float heading = Vector3.Angle(-Vector3.forward,
                //                               Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up));
                transform.position = Camera.main.transform.position;
                transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
            }
        }

        public PlaceableObject AddPlaceableObject(string identifier)
        {
            Transform t = Camera.main.transform;
            return AddPlaceableObject(identifier, transform.InverseTransformPoint(t.position + t.forward.normalized * 1.5f), Quaternion.identity);
        }

        public PlaceableObject AddPlaceableObject(string identifier, Vector3 position, Quaternion rotation)
        {
            PlaceableObject placeableObject = PlaceableObject.SetupPlaceableObject(identifier, transform);
            placeableObject.SetLocalPose(position, rotation);
            placeableObjects.Add(placeableObject);
            return placeableObject;
        }

        public void RemovePlaceable(PlaceableObject placeableObject)
        {
            placeableObjects.Remove(placeableObject);
            Destroy(placeableObject.gameObject);
        }

        public GroupData GetGroupData()
        {
            Vector3 position = transform.position;
            float headingAngle = Vector3.Angle(-Vector3.forward,
                                               Vector3.ProjectOnPlane(transform.forward, Vector3.up));
            List<PlaceableObjectData> data = new List<PlaceableObjectData>();
            foreach(PlaceableObject obj in placeableObjects)
            {
                data.Add(obj.GetPlaceableObjectData());
            }

            return new GroupData(position.z,
                                 position.x,
                                 position.y,
                                 headingAngle,
                                 data);
        }
    }
}

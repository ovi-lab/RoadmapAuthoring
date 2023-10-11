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
        private string identifier;

        /// <summary>
        /// Initialize this.
        /// <param name="data">The GroupData to use to initilize this
        /// group. If a new group, this can be null. Will initialize
        /// the transform to the current camera transform.</param>
        /// <param name="onClickedCallback">Callback when a
        /// PlaceableObject is clicked. Used when `data` is not null
        /// and populating placeable objects from the data.</param>
        /// </summary>
        public void Init(GroupData data, System.Action<PlaceableObject> onClickedCallback)
        {
            placeableObjects = new List<PlaceableObject>();
            identifier = System.Guid.NewGuid().ToString();

            if (data != null)
            {
                foreach(PlaceableObjectData objectData in data.PlaceableDataList)
                {
                    PlaceableObject placeableObject = AddPlaceableObject(objectData.prefabIdentifier, objectData.identifier, onClickedCallback,
                                                                         objectData.localPosition, objectData.localRotation, objectData.localScale, objectData.lastUpdate);
                }
            }
        }

        /// <summary>
        /// Initilize and add a PlaceableObject to this group.
        /// </summary>
        public PlaceableObject AddPlaceableObject(string prefabIdentifier, string identifier, System.Action<PlaceableObject> onClickedCallback)
        {
            Transform t = Camera.main.transform;
            return AddPlaceableObject(prefabIdentifier, identifier, onClickedCallback, transform.InverseTransformPoint(t.position + t.forward.normalized * 1.5f), Quaternion.identity, Vector3.one);
        }

        /// <summary>
        /// Initilize and add a PlaceableObject to this group.
        /// See `PlaceableObject.SetupPlacebleObject for details on
        /// `prefabIdentifier`, `identifier` and `lastUpdate`.
        /// See `PlaceableObject.SetLocalPose` for information on
        /// `position` and `rotation`.
        /// `onClickedCallback` is a function that subscribes to the
        /// `PlaceableObject.onClickedCallback` event.
        /// </summary>
        public PlaceableObject AddPlaceableObject(string prefabIdentifier, string identifier, System.Action<PlaceableObject> onClickedCallback, Vector3 position, Quaternion rotation, Vector3 scale, long lastUpdate=-1)
        {
            PlaceableObject placeableObject = PlaceableObject.SetupPlaceableObject(prefabIdentifier, identifier, this, lastUpdate);
            placeableObject.onClickedCallback += onClickedCallback;
            placeableObject.SetLocalPose(position, rotation, scale);
            placeableObject.SetObjectManipulationEnabled(PlaceablesManager.Instance.Modifyable);
            placeableObjects.Add(placeableObject);
            return placeableObject;
        }

        /// <summary>
        /// Remove given placeableObject from this group.
        /// </summary>
        public void RemovePlaceable(PlaceableObject placeableObject)
        {
            placeableObjects.Remove(placeableObject);
        }

        /// <summary>
        /// Set the modifiable for placeableObjects in this group.
        /// </summary>
        public void SetPlaceablesModifiable(bool modifyable)
        {
            foreach(PlaceableObject p in placeableObjects)
            {
                p.SetObjectManipulationEnabled(modifyable);
            }
        }

        /// <summary>
        /// Get GroupData representing this group.
        /// </summary>
        public GroupData GetGroupData()
        {
            Vector3 position = transform.position;
            // Rotation around -gravity (+y) from north (+z)
            float headingAngle = Vector3.Angle(Vector3.forward,
                                               Vector3.ProjectOnPlane(transform.forward, Vector3.up));
            List<PlaceableObjectData> data = new List<PlaceableObjectData>();
            foreach(PlaceableObject obj in placeableObjects)
            {
                data.Add(obj.GetPlaceableObjectData());
            }

            return new GroupData(identifier,
                                 position.z,
                                 position.x,
                                 position.y,
                                 headingAngle,
                                 data);
        }
    }
}

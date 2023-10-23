using System;
using System.Collections.Generic;
using UnityEngine;

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
        public List<PlaceableObject> placeableObjects { get ; private set;}
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
        public void Init(string identifier)
        {
            this.identifier = identifier;
            this.placeableObjects = new List<PlaceableObject>();
        }

        /// <summary>
        /// Add given placeableObject to this group.
        /// </summary>
        public void AddPlaceable(PlaceableObject placeableObject)
        {
            placeableObjects.Add(placeableObject);
        }

        /// <summary>
        /// Remove given placeableObject from this group.
        /// </summary>
        public void RemovePlaceable(PlaceableObject placeableObject)
        {
            placeableObjects.Remove(placeableObject);
        }

        /// <summary>
        /// Get GroupData representing this group.
        /// </summary>
        public GroupData GetGroupData()
        {
            // Vector3 position = transform.position;
            // // Rotation around -gravity (+y) from north (+z)
            // float headingAngle = Vector3.Angle(Vector3.forward,
            //                                    Vector3.ProjectOnPlane(transform.forward, Vector3.up));
            List<PlaceableObjectData> data = new List<PlaceableObjectData>();
            foreach(PlaceableObject obj in placeableObjects)
            {
                data.Add(obj.GetPlaceableObjectData());
            }

            return new GroupData(identifier,
                                 data);
        }

        /// <summary>
        /// Clear all PlaceableObjects
        /// </summary>
        public void Clear()
        {
            foreach (PlaceableObject obj in placeableObjects)
            {
                Destroy(obj.gameObject);
            }
            placeableObjects.Clear();
        }
    }
}

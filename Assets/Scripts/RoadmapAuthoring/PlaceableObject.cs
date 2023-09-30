using System;
using System.Collections.Generic;
// using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
// using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;

namespace ubc.ok.ovilab.roadmap
{
    /// <summary>
    /// An object that can be placed in the scene on an ARPlane by the PlaceablesManager.
    /// Will register with and be parented to a PlaceablesGroup.
    /// </summary>
    public class PlaceableObject : MonoBehaviour
    {
        private string prefabIdentifier;

        /// <summary>
        /// Initialze the PlaceableObject.
        /// </summary>
        public void Init(string prefabIdentifier)
        {
            this.prefabIdentifier = prefabIdentifier;
        }

        public void SetLocalPose(Pose pose)
        {
            SetLocalPose(pose.position, pose.rotation);
        }

        public void SetLocalPose(Vector3 position, Quaternion rotation)
        {
            transform.localPosition = position;
            transform.localRotation = rotation;
        }

        internal PlaceableObjectData GetPlaceableObjectData()
        {
            return new PlaceableObjectData(prefabIdentifier, new Pose(transform.localPosition, transform.localRotation));
        }

        #region Factory methods
        public static PlaceableObject SetupPlaceableObject(GameObject placeableGameObject, string identifier)
        {
            AddBoundsToAllChildren(placeableGameObject);

            SetupMRTKControls(placeableGameObject);

            PlaceableObject placeableObject = placeableGameObject.GetComponent<PlaceableObject>();
            if (placeableObject == null)
            {
                placeableObject = placeableGameObject.AddComponent<PlaceableObject>();
            }

            placeableObject.Init(identifier);

            return placeableObject;
        }

        // From https://gamedev.stackexchange.com/questions/129116/how-to-create-a-box-collider-that-surrounds-an-object-and-its-children
        /// <summary>
        /// Creates a collider that encapsulates all the objects in `newObject`
        /// </summary>
        private static void AddBoundsToAllChildren(GameObject newObject)
        {
            Collider collider;
            collider = newObject.GetComponent<Collider>();
            if (collider != null)
            {
                return;
            }
            else
            {
                collider = newObject.AddComponent<BoxCollider>();   
            }
            BoxCollider boxCol = (BoxCollider)collider;
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            Renderer thisRenderer = newObject.transform.GetComponent<Renderer>();
            if (thisRenderer != null)
            {
                bounds.Encapsulate(thisRenderer.bounds);
                boxCol.center = bounds.center - newObject.transform.position;
                boxCol.size = bounds.size;
            }

            var allDescendants = newObject.GetComponentsInChildren<Transform>();
            foreach (Transform desc in allDescendants)
            {
                Renderer childRenderer = desc.GetComponent<Renderer>();
                if (childRenderer != null)
                {
                    bounds.Encapsulate(childRenderer.bounds);
                }
                boxCol.center = bounds.center - newObject.transform.position;
                boxCol.size = bounds.size;
            }
        }

        // TODO: SetupMRTKControls
        private static void SetupMRTKControls(GameObject newObject)
        {
            // if (newObject.GetComponent<TapToPlace>() == null)
            // {
            //     TapToPlace tapToPlace = newObject.AddComponent<TapToPlace>();
            //     tapToPlace.DefaultPlacementDistance = 10;
            //     tapToPlace.MaxRaycastDistance = 50;
            //     tapToPlace.UseDefaultSurfaceNormalOffset = false;

            //     SolverHandler solverHandler = newObject.GetComponent<SolverHandler>();
            //     solverHandler.TrackedTargetType = Microsoft.MixedReality.Toolkit.Utilities.TrackedObjectType.Head;
            // }

            // if (newObject.GetComponent<BoundsControl>() == null)
            // {
            //    BoundsControl boundsControl = newObject.AddComponent<BoundsControl>();
            //    boundsControl.BoundsControlActivation = Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes.BoundsControlActivationType.ActivateByPointer;
            //    boundsControl.BoxDisplayConfig = boxDisplayConfiguration;
            // }
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ubc.ok.ovilab.roadmap
{
    [CreateAssetMenu(fileName = "Data", menuName = "Roadmap/RoadmapApplicationData", order = 1)]
    public class RoadmapApplicationConfig : ScriptableObject
    {
        [SerializeField] public string identifier = "Data";
        /// <summary>
        /// PlaceableObject prefabs available to be instantiated. At least 1 is required.
        /// Defaults to index 0. Custom UI needed to change during runtime.
        /// </summary>
        [SerializeField] private List<PlaceableContainer> placables;

        public PlaceableObject GetPlacable(string identifier, Transform parent=null)
        {
            PlaceableContainer placable = null;
            foreach (PlaceableContainer item in placables)
            {
                if (item.identifier == identifier)
                {
                    placable = item;
                }
            }

            if (placable == null)
            {
                return null;
            }
            return SetupPrefab(placable, parent);
        }

        public PlaceableObject GetPlacable(PlaceableContainer placable, Transform parent=null)
        {
            return SetupPrefab(placable, parent);
        }

        internal IEnumerable<PlaceableContainer> PlacableIdentifierList()
        {
            return placables;
        }

        private PlaceableObject SetupPrefab(PlaceableContainer placeable, Transform parent)
        {
            GameObject newObject = GameObject.Instantiate(placeable.prefab, parent);

            AddBoundsToAllChildren(newObject);

            SetupMRTKControls(newObject);

            if (newObject.GetComponent<PlaceableObject>() == null)
            {
                newObject.AddComponent<PlaceableObject>();
            }

            PlaceableObject obj = newObject.GetComponent<PlaceableObject>();
            // TODO PlaceableObject.Init
            // obj.Init(placeable.identifier);
            return obj;
        }

        // From https://gamedev.stackexchange.com/questions/129116/how-to-create-a-box-collider-that-surrounds-an-object-and-its-children
        /// <summary>
        /// Creates a collider that encapsulates all the objects in `newObject`
        /// </summary>
        private void AddBoundsToAllChildren(GameObject newObject)
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
        private void SetupMRTKControls(GameObject newObject)
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
    }

    [System.Serializable]
    public class PlaceableContainer
    {
        public string identifier;
        public GameObject prefab;
    }
}

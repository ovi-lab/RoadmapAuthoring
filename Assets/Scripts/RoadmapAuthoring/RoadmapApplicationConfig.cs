using System;
using System.Collections.Generic;
using System.Linq;
using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace ubc.ok.ovilab.roadmap
{
    [CreateAssetMenu(fileName = "Data", menuName = "Roadmap/RoadmapApplicationData", order = 1)]
    public class RoadmapApplicationConfig : ScriptableObject
    {
        private static string boundingBoxWithHandlesPath = "Packages/org.mixedrealitytoolkit.spatialmanipulation/BoundsControl/Prefabs/BoundingBoxWithHandles.prefab";
        private static GameObject boundingBoxWithHandlesPrefab;

        [SerializeField] public string identifier = "Data";
        /// <summary>
        /// PlaceableObject prefabs available to be instantiated. At least 1 is required.
        /// Defaults to index 0. Custom UI needed to change during runtime.
        /// </summary>
        [SerializeField] private List<PlaceableContainer> placables;

        // Editor-only magic function
        private void OnValidate()
        {
            if (boundingBoxWithHandlesPrefab == null)
            {
                boundingBoxWithHandlesPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(boundingBoxWithHandlesPath);
            }
        }

        public GameObject GetPleaceableGameObject(string identifier, Transform parent = null)
        {
            PlaceableContainer placeable = null;
            foreach (PlaceableContainer item in placables)
            {
                if (item.identifier == identifier)
                {
                    placeable = item;
                }
            }

            if (placeable == null)
            {
                return null;
            }

            GameObject boundsControlObj = new GameObject();
            boundsControlObj.transform.parent = parent;
            GameObject newObject = GameObject.Instantiate(placeable.prefab, boundsControlObj.transform);

            if (newObject.GetComponent<Collider>() == null)
            {
                AddBoundsToAllChildren(newObject);
            }
            SetupMRTKBounds(boundsControlObj);
            return boundsControlObj;
        }


        private void SetupMRTKBounds(GameObject boundsControlObj)
        {
            boundsControlObj.AddComponent<ConstraintManager>().AutoConstraintSelection = true;
            MinMaxScaleConstraint minMaxScaleConstraint = boundsControlObj.AddComponent<MinMaxScaleConstraint>();
            minMaxScaleConstraint.ProximityType = ManipulationProximityFlags.Near | ManipulationProximityFlags.Far;
            minMaxScaleConstraint.HandType = ManipulationHandFlags.OneHanded | ManipulationHandFlags.TwoHanded;
            minMaxScaleConstraint.RelativeToInitialState = true;

            boundsControlObj.AddComponent<UGUIInputAdapterDraggable>();

            ObjectManipulator objectManipulator = boundsControlObj.AddComponent<ObjectManipulator>();
            objectManipulator.selectMode = InteractableSelectMode.Multiple;

            BoundsControl boundsControl = boundsControlObj.AddComponent<BoundsControl>();
            boundsControl.BoundsVisualsPrefab = boundingBoxWithHandlesPrefab;
            boundsControl.BoundsCalculationMethod = BoundsCalculator.BoundsCalculationMethod.RendererOverCollider;
            boundsControl.HandlesActive = true;
        }

        // From https://gamedev.stackexchange.com/questions/129116/how-to-create-a-box-collider-that-surrounds-an-object-and-its-children
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

        public IEnumerable<string> PlacableIdentifierList()
        {
            return (placables.Select(p => p.identifier));
        }

        public string PlaceableIndexToName(int index)
        {
            return placables[index].identifier;
        }

        public int NumberOfPlaceables()
        {
            return placables.Count();
        }
    }

    [System.Serializable]
    public class PlaceableContainer
    {
        public string identifier;
        public GameObject prefab;
    }
}

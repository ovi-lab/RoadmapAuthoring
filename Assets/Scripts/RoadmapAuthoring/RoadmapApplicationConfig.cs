using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ubc.ok.ovilab.roadmap
{
    [CreateAssetMenu(fileName = "Data", menuName = "Roadmap/RoadmapApplicationData", order = 1)]
    public class RoadmapApplicationConfig : ScriptableObject
    {
        private static string boundingBoxWithHandlesPath = "Packages/org.mixedrealitytoolkit.spatialmanipulation/BoundsControl/Prefabs/BoundingBoxWithHandles.prefab";
        internal static string boundingBoxWithHandlesName = "BoundingBoxWithHandles";
        internal static GameObject boundingBoxWithHandlesPrefab;

        [SerializeField] public string identifier = "Data";
        [Tooltip("Changing this key will wipe all saved data first time a new build is run")]
        [SerializeField] public string buildKey = "00001";
        [SerializeField] public string groupID = "00001";
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

            return boundsControlObj;
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

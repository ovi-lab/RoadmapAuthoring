using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ubc.ok.ovilab.roadmap
{
    [InitializeOnLoad]
    [CreateAssetMenu(fileName = "Data", menuName = "Roadmap/RoadmapApplicationData", order = 1)]
    public class RoadmapApplicationConfig : ScriptableObject
    {
        private static string boundingBoxWithHandlesPath = "Packages/org.mixedrealitytoolkit.spatialmanipulation/BoundsControl/Prefabs/BoundingBoxWithHandles.prefab";
        internal static string boundingBoxWithHandlesName = "BoundingBoxWithHandles";

        public static RoadmapApplicationConfig activeApplicationConfig;

        [SerializeField] public string identifier = "Data";
        [Tooltip("Changing this key will wipe all saved data first time a new build is run")]
        [SerializeField] public string buildKey = "00001";
        [HideInInspector][SerializeField] public string groupID = "00001";
        [HideInInspector][SerializeField] public GameObject boundingBoxWithHandlesPrefab;
        [HideInInspector][SerializeField] public bool stateChanged;

        /// <summary>
        /// PlaceableObject prefabs available to be instantiated. At least 1 is required.
        /// Defaults to index 0. Custom UI needed to change during runtime.
        /// </summary>
        [SerializeField] private List<PlaceableContainer> placables;

        /// <summary>
        /// Given the `identifier`, instantiate the corresponding
        /// prfab and return the resulting GameObject.  If `parent` is
        /// provided, the returned GameObject will be made child of
        /// it.
        /// </summary>
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

        /// <summary>
        /// Return list of names of the placeables registered with this config.
        /// </summary>
        public IEnumerable<string> PlacableIdentifierList()
        {
            return (placables.Select(p => p.identifier));
        }

        /// <summary>
        /// Given an index, return the prefab at that index in the list of placeables.
        /// </summary>
        public string PlaceableIndexToName(int index)
        {
            return placables[index].identifier;
        }

        /// <summary>
        /// Return number of placeables
        /// </summary>
        public int NumberOfPlaceables()
        {
            return placables.Count();
        }

#if !UNITY_EDITOR
        // During runtime, make sure to set this to be the active config
        public void OnEnable()
        {
            Debug.Log($"Using applicationConfig `{identifier}`");
            activeApplicationConfig = this;
        }
#endif

#if UNITY_EDITOR
        // Editor-only magic function
        private void OnValidate()
        {
            if (boundingBoxWithHandlesPrefab == null)
            {
                boundingBoxWithHandlesPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(boundingBoxWithHandlesPath);
            }
            stateChanged = true;
        }

        /// <summary>
        /// Regsiter a asset (prefab/GameObject) with this config with the given identifier.
        /// </summary>
        public void AddPrefab(string identifier, GameObject prefab)
        {
            placables.Add(new PlaceableContainer(identifier, prefab));
        }

        /// <summary>
        /// Test if there are any duplicate prefabs. Used in the inspector GUI.
        /// </summary>
        public bool VerifyAssetDuplicates()
        {
            bool dups = placables.GroupBy(x => x.prefab).Any(g => g.Count() > 1);
            return dups;
        }

        /// <summary>
        /// Test if there are any duplicates in the identifiers. Used in the inspector GUI.
        /// </summary>
        public bool VerifyIdentifierDuplicates()
        {
            bool dups = placables.GroupBy(x => x.identifier).Any(g => g.Count() > 1);
            return dups;
        }

        /// <summary>
        /// Remove any duplicate identifiers. Used in the inspector GUI.
        /// </summary>
        public void RemoveDuplicateNames()
        {
            placables = placables.GroupBy(x => x.identifier).Select(g => g.First()).ToList();
            OnValidate();
        }

        /// <summary>
        /// Remove any duplicate assets. Used in the inspector GUI.
        /// </summary>
        public void RemoveDuplicatePrefabs()
        {
            placables = placables.GroupBy(x => x.prefab).Select(g => g.First()).ToList();
            OnValidate();
        }

#endif
    }

    /// <summary>
    /// Container class used for placeables in the roadmapapplicationconfig.
    /// </summary>
    [System.Serializable]
    public class PlaceableContainer
    {
        public string identifier;
        public GameObject prefab;

        public PlaceableContainer(string identifier, GameObject prefab)
        {
            this.prefab = prefab;
            this.identifier = identifier;
        }
    }
}

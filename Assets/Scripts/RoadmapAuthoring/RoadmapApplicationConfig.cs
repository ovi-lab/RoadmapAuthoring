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

        public GameObject GetPleaceableGameObject(string identifier, Transform parent=null)
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
            return GameObject.Instantiate(placeable.prefab, parent);
        }

        public IEnumerable<string> PlacableIdentifierList()
        {
            return (placables.Select(p => p.identifier));
        }
    }

    [System.Serializable]
    public class PlaceableContainer
    {
        public string identifier;
        public GameObject prefab;
    }
}

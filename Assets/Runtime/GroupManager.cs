using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ubc.ok.ovilab.roadmap
{
    /// <summary>
    /// Factory to generate groups.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GroupManager : MonoBehaviour
    {
        [SerializeField] protected SceneGroupsData sceneGroupsData;

        protected bool groupsInitialized = false;
        protected System.Action actionAfterInitalized;
        protected Dictionary<string, PlaceablesGroup> groups;

        private void Start()
        {
            SetupGroups();
        }

        /// <summary>
        /// Setup the groups
        /// </summary>
        protected void SetupGroups()
        {
            groups = new Dictionary<string, PlaceablesGroup>();
            foreach(GroupCoordinateData data in sceneGroupsData.groups)
            {
                groups.Add(data.identifier, Init(data));
            }
            groupsInitialized = true;
            actionAfterInitalized?.Invoke();
        }

        /// <summary>
        /// Action that would have to wait to be executed after the groups are initialized.
        /// </summary>
        public void RunAfterInitialized(System.Action action)
        {
            if (groupsInitialized)
            {
                action.Invoke();
            }
            else
            {
                actionAfterInitalized += action;
            }
        }

        /// <summary>
        /// Initializes a new instance of PlaceblesGroup and return it."
        /// </summary>
        protected virtual PlaceablesGroup Init(GroupCoordinateData data)
        {
            GameObject groupObject = new GameObject($"Group {data.identifier}");
            groupObject.transform.position = data.position;
            groupObject.transform.rotation = data.rotation;

            PlaceablesGroup placeablesGroup = groupObject.AddComponent<PlaceablesGroup>();
            placeablesGroup.Init(data.identifier);
            return placeablesGroup;
        }

        /// <summary>
        /// Return the closest group to the point passed.
        /// </summary>
        public virtual PlaceablesGroup GetClosestGroup(Vector3 point)
        {
            return groups.OrderByDescending(g => (g.Value.transform.position - point).magnitude).First().Value;
        }

        /// <summary>
        /// Get the PlaceablesGroup with identifier passed.
        /// </summary>
        public PlaceablesGroup GetPlaceablesGroup(string identifier)
        {
            return groups[identifier];
        }

        /// <summary>
        /// Get the PlaceablesGroupData list.
        /// </summary>
        public List<GroupData> GetPlaceablesGroupData()
        {
            return groups.Select(g => g.Value.GetGroupData()).ToList();
        }

        /// <summary>
        /// Clear all PlaceableObjects
        /// </summary>
        public void ClearAllPlaceableObjects()
        {
            groups.Select(kvp => kvp.Value).ToList().ForEach(g => g.Clear());
        }

        /// <summary>
        /// Get all placeableObjects
        /// </summary>
        public List<PlaceableObject> GetAllPlaceableObjects()
        {
            return groups.SelectMany(kvp => kvp.Value.placeableObjects).ToList();
        }
    }
}

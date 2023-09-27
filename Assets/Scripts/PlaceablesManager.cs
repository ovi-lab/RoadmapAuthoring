using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using TMPro;
using System;

namespace ubco.hcilab.roadmap
{
    /// <summary>
    /// Singleton Class
    /// Manages PlaceablesGroups and world placement of PlaceableObjects, as well as saving and restoring session data.
    /// </summary>
    public class PlaceablesManager : Singleton<PlaceablesManager>
    {
        [SerializeField] public RoadmapApplicationConfig applicationConfig;

        /// <summary>
        /// Required prefab for instantiating PlaceablesGroups
        /// </summary>
        [SerializeField] private GameObject _placeablesGroupPrefab;

        /// <summary>
        /// Optional visual element shown at current valid placement position
        /// </summary>
        [SerializeField] private GameObject _placingReticlePrefab;

        /// <summary>
        /// If startup geo position is greater than this distance from any previous stored PlaceablesGroup, a new one will be created. 
        /// </summary>
        [Tooltip("Distance (m) at startup past which a new Group Anchor will be created")]
        [SerializeField] private float _maxGroupDistance = 10;

        [Tooltip("Changing this key will wipe all saved data first time a new build is run")]
        [SerializeField] private string _buildKey = "00001";

        /// <summary>
        /// Get all PlaceablesGroups that exist
        /// </summary>
        public List<PlaceablesGroup> PlaceablesGroups { get => _placeablesGroups; }

        /// <summary>
        /// Get the current active session PlaceablesGroup
        /// </summary>
        public PlaceablesGroup CurrentGroup { get => _placeablesGroups.Count == 0 ? null : _placeablesGroups[_groupIndex]; }

        /// <summary>
        /// Get the currently selected prefab index
        /// </summary>
        public string CurrentPrefabIdentifier { get => _currentPrefabIdentifier; }

        /// <summary>
        /// Do we currently have a valid placement point?
        /// </summary>
        public bool ValidPlacement { get => _validPlacement; set => _validPlacement = value; }

        /// <summary>
        /// All PlaceablesGroups have finished loading
        /// </summary>
        public bool GroupsLoadingComplete { get => _loadComplete; }

        /// <summary>
        /// Raised when a Placeables position has been finalized
        /// </summary>
        [HideInInspector] public UnityEvent<GameObject> ObjectPlaced;

        /// <summary>
        /// Raised when a PlaceablesGroup has loaded
        /// </summary>
        [HideInInspector] public UnityEvent<PlaceablesGroup> GroupLoaded; /// Is null on first session

        /// <summary>
        /// Raised when all PlaceablesGroups have finished loading
        /// </summary>
        [HideInInspector] public UnityEvent<List<PlaceablesGroup>> AllGroupsLoaded; /// Is null on first session

        private List<PlaceablesGroup> _placeablesGroups = new List<PlaceablesGroup>();
        private int _groupIndex = 0;
        private bool _appStarted,
                     _loadComplete,
                     _validPlacement,
                     _placementBlocked,
                     _saveQueued;

        private GameObject _selectedPlaceable,
                           _placementReticle;

        private string _storageKey = "LocalStorageData",
                       _currentPrefabIdentifier = null;

        [SerializeField]
        private TMP_Dropdown typeOptions;
        
        [SerializeField] private Button DevButton;
        [SerializeField] private Button InfoButton;
        [SerializeField] private GameObject DevPanel;

        private void Start()
        {
            /// Wipe local storage?
            bool wipePrefs = true;

            if (PlayerPrefs.HasKey("BuildKey") && PlayerPrefs.GetString("BuildKey") == _buildKey)
                wipePrefs = false;

            if (wipePrefs)
            {
                ClearData();
            }
        }

        /// <summary>
        /// This is called once, on the first accuracy event of the session, so we know it's safe to recall all stored data
        /// </summary>
        private void OnGeoInitCompleted()
        {
            RestoreSavedPlaceablesGroups();
        }

        /// <summary>
        /// Loads all data and then checks distance to all PlaceablesGroups. Creates new Group if needed.
        /// </summary>
        private async void RestoreSavedPlaceablesGroups()
        {
            LoadAll();

            await Task.Delay(1000);

            /// Find closest group        
            PlaceablesGroup closestGroup = null;
            float distance = Mathf.Infinity;
            bool needNewGroup = false;

            _placeablesGroups.ForEach(group =>
            {
                if (Vector3.Distance(group.transform.position, Camera.main.transform.position) < distance)
                {
                    distance = Vector3.Distance(group.transform.position, Camera.main.transform.position);
                    //Debug.Log("Distance to Group" + _placeablesGroups.IndexOf(group) + ": " + distance);
                    closestGroup = group;
                }
            });

            /// Do we need to create a new Group Anchor?
            needNewGroup = closestGroup == null || distance > _maxGroupDistance;

            if (!needNewGroup) Debug.Log("Using Group @ distance: " + distance);

            if (needNewGroup)
            {
                Debug.Log("Creating new PlaceablesGroup");
                CreateNewPlaceablesGroup().Init();
            }

            _appStarted = true;
        }

        /// <summary>
        /// Instantiates a new PlaceablesGroup from prefab
        /// </summary>
        /// <returns></returns>
        private PlaceablesGroup CreateNewPlaceablesGroup()
        {
            PlaceablesGroup newGroup = Instantiate(_placeablesGroupPrefab).GetComponent<PlaceablesGroup>();
            if (!_placeablesGroups.Contains(newGroup))
                _placeablesGroups.Add(newGroup);

            _groupIndex = _placeablesGroups.IndexOf(newGroup);

            return newGroup;
        }

        /// <summary>
        /// Public call to serialize and save all current session data
        /// Operation will be queued for next frame
        /// </summary>
        public void Save()
        {
            if (_saveQueued || !_appStarted) return;

            QueueSave();
        }

        /// <summary>
        /// Delay save operation until next frame
        /// </summary>
        private async void QueueSave()
        {
            _saveQueued = true;

            await Task.Yield();

            SaveImmediate();
        }

        /// <summary>
        /// Clear all data in Player Prefs
        /// </summary>
        public void ClearData()
        {
            DestroyAll();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetString("BuildKey", _buildKey);
            PlayerPrefs.Save();
            Debug.Log("Internal Storage Reset");

            if (_appStarted)
            {
                Debug.Log("Creating new PlaceablesGroup");
                CreateNewPlaceablesGroup().Init();
            }
        }

        /// <summary>
        /// Serialize and save all current session data to Player Prefs
        /// </summary>
        private void SaveImmediate()
        {
            int count = 0;

            /// Find out if there's anything to save?
            _placeablesGroups.ForEach(group =>
            {
                group.Placeables.ForEach(p =>
                {
                    if (p.State == PlaceableState.Finalized)
                        count++;
                });
            });

            if (count == 0)
            {
                _saveQueued = false;
                return;
            }

            List<GroupData> groupDataList = new List<GroupData>();

            _placeablesGroups.ForEach(group =>
            {
                groupDataList.Add(group.GroupData);

                group.GroupData.PlaceableDataList = new List<PlaceableObjectData>();

                group.Placeables.ForEach(placeable =>
                {
                    if (placeable.State == PlaceableState.Finalized)
                        group.GroupData.PlaceableDataList.Add(placeable.GetData());
                });

                Debug.Log("Saved Group + " + group.GroupData.PlaceableDataList.Count + " Placeables @ " +
                                             group.GroupData.Latitude.ToString("F2") + " | " +
                                             group.GroupData.Longitude.ToString("F2") + " | " +
                                             group.GroupData.Altitude.ToString("F2"));
            });

            LocalStorageData storageData = new LocalStorageData(groupDataList);
            PlayerPrefs.SetString(_storageKey, JsonUtility.ToJson(storageData));

            PlayerPrefs.Save();
            _saveQueued = false;
        }

        /// <summary>
        /// Load and deserialize previous session data from Player Prefs
        /// </summary>
        public void LoadAll()
        {
            if (!PlayerPrefs.HasKey(_storageKey))
            {
                Debug.Log("Nothing to load");
                GroupLoaded?.Invoke(null);
                _loadComplete = true;
                AllGroupsLoaded?.Invoke(null);
                return;
            }

            LocalStorageData storageData = JsonUtility.FromJson<LocalStorageData>(PlayerPrefs.GetString(_storageKey));

            storageData.Groups.ForEach(groupData =>
            {
                PlaceablesGroup group = CreateNewPlaceablesGroup();
                group.Restore(groupData);

                Transform groupTransform = group.transform;

                groupData.PlaceableDataList.ForEach(placeableData =>
                {
                    PlaceableObject placeable = applicationConfig.GetPlacable(placeableData.PrefabIdentifier, groupTransform);
                    placeable.Restore(placeableData, group);
                });

                Debug.Log("Loaded Group + " + _placeablesGroups[_groupIndex].Placeables.Count + " Placeables @ " +
                                              groupData.Latitude.ToString("F2") + " | " +
                                              groupData.Longitude.ToString("F2") + " | " +
                                              groupData.Altitude.ToString("F2"));

                GroupLoaded?.Invoke(_placeablesGroups[_groupIndex]);
            });

            _loadComplete = true;
            AllGroupsLoaded?.Invoke(_placeablesGroups);
        }

        /// <summary>
        /// Destroy all PlaceablesGroups in the scene
        /// </summary>
        public void DestroyAll()
        {
            for (int i = _placeablesGroups.Count - 1; i >= 0; i--)
            {
                Destroy(_placeablesGroups[i].gameObject);
                _placeablesGroups.Remove(_placeablesGroups[i]);
            }

            _groupIndex = 0;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
                SaveImmediate();
        }

        private void OnApplicationQuit()
        {
            if (_appStarted)
                SaveImmediate();
        }

        private void DevWebsite()
        {
            DevPanel.SetActive(true);
        }

        private void InfoWebsite()
        {
            Application.OpenURL("https://github.com/hcilab-uofm/Roadmap/blob/main/README.md");
            //Application.OpenURL("https://lyonscrawl.github.io/");
        }

    }
}

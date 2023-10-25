using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ubc.ok.ovilab.roadmap
{
    /// <summary>
    /// Singleton Class
    /// Manages PlaceablesGroups and world placement of PlaceableObjects, as well as saving and restoring session data.
    /// Expects a component that implements the `PopupManager` class.
    /// </summary>
    [RequireComponent(typeof(PopupManager))]
    public class PlaceablesManager : Singleton<PlaceablesManager>
    {
        [SerializeField] private GroupManager groupManager;
        
        private const string _playerPrefsStorageKey = "RoadMapStorage";
        private const string _playerPrefsBranchNameKey = "RoadMapBranchName";
        private bool modifyable = false;
        private bool deleting = false;
        private PopupManager popupManager;

        public string BranchName { get; private set; }

        public bool Modifyable
        {
            get => modifyable;
            set => SetPlaceablesModifiable(value);
        }

        public PlaceableObject ActivePlaceableObject { get; set; }

        #region Unity functions
        private void Start()
        {
            popupManager = GetComponent<PopupManager>();
            modifyable = false;
            // TODO: remove the dependency on build key
            if (!(PlayerPrefs.HasKey("BuildKey") && PlayerPrefs.GetString("BuildKey") == RoadmapApplicationConfig.activeApplicationConfig.buildKey))
            {
                groupManager.RunAfterInitialized(ClearData);
            }
            if (PlayerPrefs.HasKey(_playerPrefsBranchNameKey))
            {
                BranchName = PlayerPrefs.GetString(_playerPrefsBranchNameKey);
            }
            else
            {
                BranchName = $"tempMaster_{System.DateTime.Now.Ticks}";
            }
            groupManager.RunAfterInitialized(LoadAll);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if(pauseStatus)
            {
                SaveImmediate();
            }
        }

        private void OnApplicationQuit()
        {
            SaveImmediate();
        }
        #endregion

        #region load & save
        /// <summary>
        /// Serialize and save all current session data to Player Prefs & also a copy to the application persistent path.
        /// </summary>
        private void SaveImmediate()
        {
            // int count = 0;

            /// TODO:Find out if there's anything to save?
            // groups.ForEach(group =>
            // {
            //     group.Placeables.ForEach(p =>
            //     {
            //         if (p.State == PlaceableState.Finalized)
            //             count++;
            //     });
            // });

            // if (count == 0)
            // {
            //     return;
            // }

            StorageData storageData = GetStorageData();
            string jsonString = JsonUtility.ToJson(storageData);
            PlayerPrefs.SetString(_playerPrefsStorageKey, jsonString);
            PlayerPrefs.SetString(_playerPrefsBranchNameKey, BranchName);

            System.IO.File.WriteAllText(GetSaveFileLocation(), jsonString);
            PlayerPrefs.Save();
            Debug.Log($"Saving data\n{storageData}");
        }

        /// <summary>
        /// Returns instance of StorageData representing current state of applications.
        /// </summary>
        public StorageData GetStorageData()
        {
            return new StorageData(groupManager.GetPlaceablesGroupData(), PlatformManager.Instance.currentPlatform.ToString(), RoadmapApplicationConfig.activeApplicationConfig.buildKey, BranchName);
        }

        /// <summary>
        /// Clear state. Would also clear the PlayerPrefs associated with the applications.
        /// </summary>
        public void ClearData()
        {
            groupManager.ClearAllPlaceableObjects();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetString("BuildKey", RoadmapApplicationConfig.activeApplicationConfig.buildKey);
            PlayerPrefs.Save();
            Debug.Log("Internal Storage Reset");
        }

        // FIXME: Make this async?
        /// <summary>
        /// Load data from the PlayerPrefs and populate application state.
        /// </summary>
        public void LoadAll()
        {
            if (!PlayerPrefs.HasKey(_playerPrefsStorageKey))
            {
                Debug.Log("Nothing to load");
                return;
            }

            StorageData storageData = JsonUtility.FromJson<StorageData>(PlayerPrefs.GetString(_playerPrefsStorageKey));

            LoadFromStorageData(storageData);
        }

        /// <summary>
        /// Populate application state from a `StorageData`.
        /// If suceeded will return true.
        /// </summary>
        public bool LoadFromStorageData(StorageData storageData, bool force=false)
        {
            Debug.Log($"Loading data\n{storageData}");
            if (!force)
            {
                List<string> placeables = RoadmapApplicationConfig.activeApplicationConfig.PlacableIdentifierList().ToList();
                foreach (PlaceableObjectData data in storageData.groups.SelectMany(g => g.PlaceableDataList))
                {
                    if (!placeables.Contains(data.prefabIdentifier))
                    {
                        return false;
                    }
                }
            }
            storageData.groups.ForEach(groupData => SetupGroup(groupData));
            return true;
        }

        /// <summary>
        /// Setup new group.
        /// The groupData passed to this also can be null (See PlaceablesGroup.Init for more information)
        /// </summary>
        private void SetupGroup(GroupData groupData)
        {
            PlaceablesGroup thisGroup = groupManager.GetPlaceablesGroup(groupData.identifier);
            foreach(PlaceableObjectData data in groupData.PlaceableDataList)
            {
                AddPlaceableObject(data.prefabIdentifier, data.identifier, thisGroup, OnPlaceableClicked, data.localPosition, data.localRotation, data.localScale, data.lastUpdate);
            }
        }

        /// <summary>
        /// Get location to save application state as a json file.
        /// </summary>
        private string GetSaveFileLocation()
        {
            return System.IO.Path.Combine(Application.persistentDataPath, $"{RoadmapApplicationConfig.activeApplicationConfig.buildKey}_{_playerPrefsStorageKey}.json");
        }
        #endregion

        #region Create & manage placeables
        /// <summary>
        /// Callback for the add UI button when adding new objects from the List Menu.
        /// </summary>
        public void SpawnObject(string identifier)
        {
            AddPlaceableObject(identifier, "", OnPlaceableClicked);
        }

        /// <summary>
        /// Initilize and add a PlaceableObject to this group.
        /// </summary>
        public PlaceableObject AddPlaceableObject(string prefabIdentifier, string identifier, System.Action<PlaceableObject> onClickedCallback)
        {
            Transform t = Camera.main.transform;
            Vector3 newPosition = t.position + t.forward.normalized * 1.5f;
            PlaceablesGroup placeablesGroup = groupManager.GetClosestGroup(newPosition);
            Vector3 localPosition = placeablesGroup.transform.InverseTransformPoint(newPosition);
            return AddPlaceableObject(prefabIdentifier, identifier, placeablesGroup, onClickedCallback, localPosition, Quaternion.identity, Vector3.one);
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
        public PlaceableObject AddPlaceableObject(string prefabIdentifier, string identifier, PlaceablesGroup placeablesGroup, System.Action<PlaceableObject> onClickedCallback, Vector3 position, Quaternion rotation, Vector3 scale, long lastUpdate=-1)
        {
            PlaceableObject placeableObject = PlaceableObject.SetupPlaceableObject(prefabIdentifier, identifier, placeablesGroup, lastUpdate);
            if (placeableObject ==  null)
            {
                return null;
            }

            placeableObject.onClickedCallback += onClickedCallback;
            placeableObject.SetLocalPose(position, rotation, scale);
            placeableObject.SetObjectManipulationEnabled(modifyable);
            return placeableObject;
        }
        #endregion

        #region UI related functions
        /// <summary>
        /// Empty callback function.
        /// </summary>
        public void EmptyCallback() { }

        /// <summary>
        /// Callback for the delete button.
        /// </summary>
        public void StartDelete()
        {
            deleting = true;
            // NOTE: Passing empty action to cancel so that the button
            // shows up.  Since the dissmissCallback is expected to be
            // called always, resetting `deleting` there.
            popupManager.OpenDialogWithMessage("Select object to delete", "", null, "Cancel", EmptyCallback, () => deleting = false);
        }

        /// <summary>
        /// Delete the active placeable. Also stops the delete mode.
        /// </summary>
        public void DeleteActivePlaceable()
        {
            if (ActivePlaceableObject != null)
            {
                ActivePlaceableObject.DeleteThySelf();
                ActivePlaceableObject = null;
            }
        }

        /// <summary>
        /// Callback for when the placeable object is clicked
        /// </summary>
        private void OnPlaceableClicked(PlaceableObject placeableObject)
        {
            ActivePlaceableObject = placeableObject;
            if (deleting)
            {
                popupManager.DismissPopup();
                popupManager.OpenDialogWithMessage("Deleting selected object. Are you sure?", "Yes", DeleteActivePlaceable, "No (Cancel delete)", EmptyCallback, null);
                deleting = false;
            }
        }

        /// <summary>
        /// Make all placeable objects modifiable or not.
        /// Also used as the callback function for the modify UI button.
        /// </summary>
        public void SetPlaceablesModifiable(bool modifyable)
        {
            this.modifyable = modifyable;
            groupManager.GetAllPlaceableObjects().ForEach(obj => { obj.SetObjectManipulationEnabled(modifyable); });

            // NOTE: Any popup would have to gracefully get dismissed
            popupManager.DismissPopup();
            deleting = deleting && modifyable;
        }

        /// <summary>
        /// Get branch name
        /// </summary>
        public void SetBranchName(string branchName)
        {
            this.BranchName = branchName;
        }
        #endregion
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using TMPro;
using System;

namespace ubc.ok.ovilab.roadmap
{
    /// <summary>
    /// Singleton Class
    /// Manages PlaceablesGroups and world placement of PlaceableObjects, as well as saving and restoring session data.
    /// </summary>
    [RequireComponent(typeof(PopupManager))]
    public class PlaceablesManager : Singleton<PlaceablesManager>
    {
        [SerializeField] public RoadmapApplicationConfig applicationConfig;
        
        private string _storageKey = "RoadMapStorage";
        private List<PlaceablesGroup> groups = new List<PlaceablesGroup>();
        private PlaceablesGroup currentGroup;
        private bool modifyable = false;
        private bool deleting = false;
        private PopupManager popupManager;

        public bool Modifyable
        {
            get => modifyable;
            set => SetPlaceablesModifiable(value);
        }

        public PlaceableObject ActivePlaceableObject { get; set; }

        private void Start()
        {
            popupManager = GetComponent<PopupManager>();
            modifyable = false;
            if (!(PlayerPrefs.HasKey("BuildKey") && PlayerPrefs.GetString("BuildKey") == applicationConfig.buildKey))
            {
                ClearData();
            }
            LoadAll();
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

        #region load & save
        /// <summary>
        /// Serialize and save all current session data to Player Prefs
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

            LocalStorageData storageData = GetLocalStorageData();
            string jsonString = JsonUtility.ToJson(storageData);
            PlayerPrefs.SetString(_storageKey, jsonString);

            System.IO.File.WriteAllText(GetSaveFileLocation(), jsonString);
            PlayerPrefs.Save();
            Debug.Log($"Saving data");
        }

        public LocalStorageData GetLocalStorageData()
        {
            return new LocalStorageData(groups.Select(g => g.GetGroupData()).ToList(), PlatformManager.Instance.currentPlatform.ToString());
        }

        public void ClearData()
        {
            DestroyAll();
            groups.Clear();
            SetupGroup(null);
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetString("BuildKey", applicationConfig.buildKey);
            PlayerPrefs.Save();
            Debug.Log("Internal Storage Reset");
        }

        private async void RestoreSavedData()
        {
            LoadAll();

            await Task.Delay(1000);
        }

        public void LoadAll()
        {
            if (!PlayerPrefs.HasKey(_storageKey))
            {
                Debug.Log("Nothing to load");
                SetupGroup(null);
                return;
            }

            LocalStorageData storageData = JsonUtility.FromJson<LocalStorageData>(PlayerPrefs.GetString(_storageKey));

            LoadFromLocalStorageData(storageData);
        }

        public void LoadFromLocalStorageData(LocalStorageData storageData)
        {
            storageData.groups.ForEach(groupData => SetupGroup(groupData));
            if (currentGroup == null)
            {
                SetupGroup(null);
            }
        }

        // TODO DestroyAll
        private void DestroyAll()
        {
            foreach(PlaceablesGroup group in groups)
            {
                Destroy(group.gameObject);
            }
        }

        private void SetupGroup(GroupData groupData)
        {
            GameObject groupObject = new GameObject("Group");
            PlaceablesGroup placeablesGroup = groupObject.AddComponent<PlaceablesGroup>();
            placeablesGroup.Init(groupData, OnPlaceableClicked);
            groups.Add(placeablesGroup);

            // TODO: group selection
            currentGroup = placeablesGroup;
        }

        private string GetSaveFileLocation()
        {
            return System.IO.Path.Combine(Application.persistentDataPath, $"{applicationConfig.buildKey}_{_storageKey}.json");
        }
        #endregion

        #region UI related functions
        /// <summary>
        /// Empty callback
        /// </summary>
        public void EmptyCallback() { }

        /// <summary>
        /// Callback for the add UI button when adding new objects from the List Menu.
        /// </summary>
        public void SpawnObject(string identifier)
        {
            PlaceableObject placeableObject = currentGroup.AddPlaceableObject(identifier, "", OnPlaceableClicked);
        }

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
            foreach(PlaceablesGroup g in groups)
            {
                g.SetPlaceablesModifiable(modifyable);
            }

            // NOTE: Any popup would have to gracefully get dismissed
            popupManager.DismissPopup();
            deleting = deleting && modifyable;
        }

        #endregion
    }
}

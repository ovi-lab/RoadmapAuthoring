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
    public class PlaceablesManager : Singleton<PlaceablesManager>
    {
        [SerializeField] public RoadmapApplicationConfig applicationConfig;
        
        [Tooltip("Changing this key will wipe all saved data first time a new build is run")]
        [SerializeField] private string _buildKey = "00001";
        private string _storageKey = "RoadMapStorage";
        private List<PlaceablesGroup> groups = new List<PlaceablesGroup>();

        private void Start()
        {
            if (PlayerPrefs.HasKey("BuildKey") && PlayerPrefs.GetString("BuildKey") == _buildKey)
            {
                ClearData();
            }
        }

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

            LocalStorageData storageData = new LocalStorageData(groups.Select(g => g.GetGroupData()).ToList());
            string jsonString = JsonUtility.ToJson(storageData);
            PlayerPrefs.SetString(_storageKey, jsonString);

            System.IO.File.WriteAllText(GetSaveFileLocation(), jsonString);
            PlayerPrefs.Save();
            Debug.Log($"Saving data");
        }

        public void ClearData()
        {
            DestroyAll();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetString("BuildKey", _buildKey);
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
                return;
            }

            LocalStorageData storageData = JsonUtility.FromJson<LocalStorageData>(PlayerPrefs.GetString(_storageKey));

            storageData.Groups.ForEach(groupData => SetupGroup(groupData));
        }

        // TODO DestroyAll
        private void DestroyAll()
        {
        }

        private void SetupGroup(GroupData groupData)
        {
            GameObject groupObject = new GameObject("Group");
            PlaceablesGroup placeablesGroup = groupObject.AddComponent<PlaceablesGroup>();
            placeablesGroup.Init(groupData);
        }

        private string GetSaveFileLocation()
        {
            return System.IO.Path.Combine(Application.persistentDataPath, $"{_buildKey}_{_storageKey}.json");
        }
    }
}

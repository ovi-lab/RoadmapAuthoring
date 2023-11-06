using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Collections;
using System;

namespace ubc.ok.ovilab.roadmap
{
    [RequireComponent(typeof(PopupManager))]
    public class RemoteDataSynchronization : Singleton<RemoteDataSynchronization>
    {
        private const string SERVER_URL = "https://roadmap-ubco-default-rtdb.firebaseio.com/";
        private const string DB_SCENE_DATA = "scene_data";
        private const string DB_SCENES = "scenes";
        private const string DB_GROUPS = "groups";
        private const string DB_BRANCH = "branch";
        private const string _playerPrefsStorageKey = "RoadMapStorageSyncLastPushed";
        private const string _playerPrefsBranchesList = "RoadMapStorageSyncBranchesList";
        private const string _playerPrefsBranchesCachedTime = "RoadMapStorageSyncBranchesCachedTime";

        private string lastPushedSceneDataId;
        private string LastPushedSceneDataId
        {
            get => lastPushedSceneDataId;
            set
            {
                PlayerPrefs.SetString(_playerPrefsStorageKey, value);
                lastPushedSceneDataId = value;
            }
        }

        private PopupManager popupManager;
        private Dictionary<string, string> branchListCache;
        private long branchesListCachedTime;

        #region Unity functions
        private void Start()
        {
            if (PlayerPrefs.HasKey(_playerPrefsStorageKey))
            {
                lastPushedSceneDataId = PlayerPrefs.GetString(_playerPrefsStorageKey);
            }

            if (PlayerPrefs.HasKey(_playerPrefsBranchesList) && PlayerPrefs.HasKey(_playerPrefsBranchesCachedTime))
            {
                branchListCache = JsonConvert.DeserializeObject<Dictionary<string, string>>(PlayerPrefs.GetString(_playerPrefsBranchesList));
                branchesListCachedTime = long.Parse(PlayerPrefs.GetString(_playerPrefsBranchesCachedTime));
            }
            else
            {
                UpdateBranchesList();
            }

            popupManager = GetComponent<PopupManager>();
        }
        #endregion

        #region Sync functions
        /// <summary>
        /// Get the active scene ID to use
        /// </summary>
        private string SceneID()
        {
            return $"{GroupID()}_{RoadmapApplicationConfig.activeApplicationConfig.buildKey}";
        }

        /// <summary>
        /// Get the group id to use.
        /// </summary>
        private string GroupID()
        {
            if (string.IsNullOrEmpty(RoadmapApplicationConfig.activeApplicationConfig.groupID))
            {
                throw new UnityException($"GroupID not set");
            }
            return $"{RoadmapApplicationConfig.activeApplicationConfig.groupID}";
        }

        /// <summary>
        /// Get the active scene ID to use
        /// </summary>
        private string ActiveBranchName()
        {
            return $"{PlaceablesManager.Instance.BranchName}";
        }

        /// <summary>
        /// Run callable after verifying the scene exists in "scenes"
        /// </summary>
        private void CheckSceneInScenes(System.Action callable)
        {
            ProcessRequest($"/{DB_SCENES}/{SceneID()}", HTTPMethod.GET, (dataString) =>
            {
                if (string.IsNullOrEmpty(dataString) || dataString == "null")
                {
                    ProcessRequest($"/{DB_SCENES}/{SceneID()}/{DB_GROUPS}/{GroupID()}", HTTPMethod.PUT, data: JsonConvert.SerializeObject(true), action: (_) =>
                    {
                        ProcessRequest($"/{DB_GROUPS}/{GroupID()}/{DB_SCENES}/{SceneID()}", HTTPMethod.PUT, data: JsonConvert.SerializeObject(true), action: (_) =>
                        {
                            callable();
                        });
                    });
                }
                else
                {
                    callable();
                }
            });
        }

        /// <summary>
        /// Save the scene data to the remote.
        /// </summary>
        private void SaveSceneData(StorageData data, string branchName=null)
        {
            if (string.IsNullOrEmpty(branchName))
            {
                branchName = ActiveBranchName();
            }
            CheckSceneInScenes(() =>
            {
                RemoteStorageData sceneData = new RemoteStorageData(System.DateTime.Now.Ticks, data, GroupID(), branchName);

                ProcessRequest($"/{DB_SCENE_DATA}", HTTPMethod.POST, (nameString) =>
                {
                    string name = JsonConvert.DeserializeAnonymousType(nameString, new { name = "" }).name;
                    ProcessRequest($"/{DB_SCENE_DATA}/{name}", HTTPMethod.PUT, (dataString) =>
                    {
                        ProcessRequest($"/{DB_SCENES}/{SceneID()}/{DB_SCENE_DATA}/{name}", HTTPMethod.PUT, (_) =>
                        {
                            ProcessRequest($"/{DB_BRANCH}/{GroupID()}/{branchName}", HTTPMethod.PUT, (_) =>
                            {
                                LastPushedSceneDataId = name; // Set only if everything went smooth!
                                UpdateBranchesList();
                            }, JsonConvert.SerializeObject(name));
                        }, JsonConvert.SerializeObject(sceneData.commit_time));
                    }, JsonConvert.SerializeObject(sceneData, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));

                }, JsonConvert.SerializeObject(true));
            });
        }

        /// <summary>
        /// Exectue callback with the latest scene data on remote.
        /// </summary>
        private void ProcessRemoteStorageData(System.Action<RemoteStorageData> callback, string branchName=null)
        {
            if (string.IsNullOrEmpty(branchName))
            {
                branchName = ActiveBranchName();
            }
            ProcessRequest($"/{DB_BRANCH}/{GroupID()}/{branchName}", HTTPMethod.GET, (idString) =>
            {
                if (string.IsNullOrEmpty(idString))
                {
                    StorageData data = PlaceablesManager.Instance.GetStorageData();
                    SaveSceneData(data, branchName);
                    callback(new RemoteStorageData(System.DateTime.Now.Ticks, data, GroupID(), ActiveBranchName()));
                }
                else
                {
                    idString = JsonConvert.DeserializeObject<string>(idString);
                    ProcessRequest($"/{DB_SCENE_DATA}/{idString}", HTTPMethod.GET, (dataString) =>
                    {
                        RemoteStorageData remoteData = JsonUtility.FromJson<RemoteStorageData>(dataString);
                        callback(remoteData);
                    });
                }
            });
        }

        /// <summary>
        /// Update list of branches from remote cache
        /// </summary>
        public void UpdateBranchesList()
        {
            ProcessRequest($"/{DB_BRANCH}/{GroupID()}", HTTPMethod.GET, (branchesString) =>
            {
                branchListCache = JsonConvert.DeserializeObject<Dictionary<string, string>>(branchesString);
                if (branchListCache == null)
                {
                    branchListCache = new Dictionary<string, string>();
                }
                branchesListCachedTime = System.DateTime.Now.Ticks;
                PlayerPrefs.SetString(_playerPrefsStorageKey, JsonConvert.SerializeObject(branchListCache));
                PlayerPrefs.SetString(_playerPrefsBranchesCachedTime, branchesListCachedTime.ToString());
            });
        }

        /// <summary>
        /// Safely update the lcoal storage data
        /// </summary>
        private void SafeLoadFromStorageData(StorageData result, StorageData fallback, System.Action sucessCallback=null, bool saveScene=true)
        {
            /// Clear and write local data
            PlaceablesManager.Instance.ClearData();

            if(PlaceablesManager.Instance.LoadFromStorageData(result))
            {
                sucessCallback?.Invoke();
                if (saveScene)
                {
                    /// Write remote data
                    SaveSceneData(result);
                }
            }
            else
            {
                popupManager.OpenDialogWithMessage("There are placeables not recognized by currently built app config. If continued, missing prefabs will be discarded. Continue?", "yes", () =>
                {
                    if (PlaceablesManager.Instance.LoadFromStorageData(result, force:true))
                    {
                        sucessCallback?.Invoke();
                        if (saveScene)
                        {
                            /// Write remote data
                            SaveSceneData(result);
                        }
                    }
                }, () =>
                {
                    PlaceablesManager.Instance.LoadFromStorageData(fallback, force:true);
                });
            }
        }

        /// <summary>
        /// Sync with given branch.
        /// </summary>
        public void MergeWithRemoteBranch(string branchName)
        {
            StorageData localData = PlaceablesManager.Instance.GetStorageData();

            ProcessRemoteStorageData((remoteDataStorage) =>
            {
                /// localData has the current platform set as LastWrittenPlatform
                StorageData result = StorageData.MergeData(remoteDataStorage.GetData(), localData, localData.lastWrittenPlatform, localData.buildKey, localData.branchName);

                SafeLoadFromStorageData(result, localData, saveScene:false);
            }, branchName);
        }

        /// <summary>
        /// Change to given branch. Unsynced changes will be lost.
        /// </summary>
        public void ChangeToRemoteBranch(string branchName)
        {
            StorageData localData = PlaceablesManager.Instance.GetStorageData();

            ProcessRemoteStorageData((remoteDataStorage) =>
            {
                StorageData result = remoteDataStorage.GetData();
                SafeLoadFromStorageData(result, localData, () => PlaceablesManager.Instance.SetBranchName(branchName), false);
            }, branchName);
        }

        /// <summary>
        /// Combine data from scene and local. Overwrites both local and remote.
        /// </summary>
        public void SyncWithRemote()
        {
            StorageData localData = PlaceablesManager.Instance.GetStorageData();

            ProcessRemoteStorageData((remoteDataStorage) =>
            {
                /// localData has the current platform set as LastWrittenPlatform
                StorageData result = StorageData.MergeData(remoteDataStorage.GetData(), localData, localData.lastWrittenPlatform, localData.buildKey, localData.branchName);
                SafeLoadFromStorageData(result, localData);
            });
        }

        /// <summary>
        /// Make the current scene the latest version on the remote.
        /// </summary>
        public void OverwriteRemote()
        {
            SaveSceneData(PlaceablesManager.Instance.GetStorageData());
        }

        /// <summary>
        /// Make the latest version on the remote the current scene.
        /// </summary>
        public void OverwriteLocal()
        {
            string activeBranchName = ActiveBranchName();
            if (branchListCache.ContainsKey(activeBranchName))
            {
                ProcessRemoteStorageData((remoteData) =>
                {
                    StorageData data = remoteData.GetData();
                    StorageData localData = PlaceablesManager.Instance.GetStorageData();

                    PlaceablesManager.Instance.ClearData();
                    SafeLoadFromStorageData(data, localData, saveScene:false);
                });
            }
            else
            {
                popupManager.OpenDialogWithMessage($"No branch named {activeBranchName} in remote.", () => { });
            }
        }

        /// <summary>
        /// Delete the last pushed scene data
        /// </summary>
        public void RemoveLastRemoteStorageData()
        {
            if (!string.IsNullOrEmpty(LastPushedSceneDataId))
            {
                ProcessRequest($"/{DB_SCENE_DATA}/{LastPushedSceneDataId}", HTTPMethod.DELETE);
                ProcessRequest($"/{DB_SCENES}/{SceneID()}/{DB_SCENE_DATA}/{LastPushedSceneDataId}", HTTPMethod.DELETE);
                LastPushedSceneDataId = null;
            }
        }
        #endregion

        #region Helper functions
        /// <summary>
        /// Helper method to execute web request asynchronously.
        /// </summary>
        protected void ProcessRequest(string endpoint, HTTPMethod method, System.Action<string> action = null, string data = "")
        {
            StartCoroutine(GetJsonUrl(endpoint, method, action, data));
        }

        /// <summary>
        /// Helper method for a get http request.
        /// </summary>
        protected UnityWebRequest GetMethod(HTTPMethod method, string url, string data)
        {
            switch (method)
            {
                case HTTPMethod.GET:
                    return UnityWebRequest.Get(url);
                case HTTPMethod.POST:
                    return UnityWebRequest.PostWwwForm(url, data);
                case HTTPMethod.PUT:
                    return UnityWebRequest.Put(url, data);
                case HTTPMethod.DELETE:
                    return UnityWebRequest.Delete(url);
                default:
                    throw new System.NotImplementedException();
            }
        }

        /// <summary>
        /// Helper method to get a json result from a http request.
        /// </summary>
        protected IEnumerator GetJsonUrl(string endpoint, HTTPMethod method, System.Action<string> action = null, string data = "")
        {
            string url = $"{SERVER_URL}{endpoint}.json";
            using (UnityWebRequest webRequest = GetMethod(method, url, data))
            {
                webRequest.timeout = 5;
                yield return webRequest.SendWebRequest();

                bool error;
#if UNITY_2020_OR_NEWER
                error = webRequest.result != UnityWebRequest.Result.Success;
#else
#pragma warning disable
                error = webRequest.isHttpError || webRequest.isNetworkError;
#pragma warning restore
#endif

                if (error)
                {
                    Debug.LogError($"Request for {url} failed with: {webRequest.error}");
                    yield break;
                }

                Debug.Log($"{method} request to {url} and got `{webRequest.downloadHandler.data}`");
                if (action != null)
                {
                    action(System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data));
                }
            }
        }
        #endregion

        #region UI functions
        public void OverwriteRemoteWithPrompt()
        {
            popupManager.OpenDialogWithMessage("Overwrite the remote?", "Yes", OverwriteRemote, () => { });
        }

        public void OverwriteLocalWithPrompt()
        {
            popupManager.OpenDialogWithMessage("Overwrite the local?", "Yes", OverwriteLocal, () => { });
        }

        public void SyncWithRemoteWithPrompt()
        {
            popupManager.OpenDialogWithMessage("Synchronize remote and local?", "Yes", () =>
            {
                StorageData localDataBeforeSync = PlaceablesManager.Instance.GetStorageData();
                SyncWithRemote();
                popupManager.OpenDialogWithMessage("Sync was a success?", "Yes", () => { }, "No(revert)", () =>
                {
                    RemoveLastRemoteStorageData();
                    PlaceablesManager.Instance.LoadFromStorageData(localDataBeforeSync, force:true);
                }, () => { });
            }, () => { });
        }

        public void MergeWithRemoteBranchWithPrompt(string branchName)
        {
            if (branchName != ActiveBranchName())
            {
                popupManager.OpenDialogWithMessage("Do you want to continue?", "Yes", () => MergeWithRemoteBranch(branchName), () => { });
            }
            else
            {
                popupManager.OpenDialogWithMessage("Selected active branch. Nothing happens", () => { });
            }
        }

        public void ChangeToRemoteBranchWithPrompt(string branchName)
        {
            if (branchName != ActiveBranchName())
            {
                string message;
                if (branchListCache.ContainsKey(PlaceablesManager.Instance.BranchName))
                {
                    message = $"Active branch `{PlaceablesManager.Instance.BranchName}` not seen in remote. Try updating branch list or pushing. If this branch is not in the remote, all data for this branch will be lost.";
                }
                else
                {
                    message = $"Changing to branch {branchName} from {PlaceablesManager.Instance.BranchName}";
                }
                popupManager.OpenDialogWithMessage(message, "yes",
                                                   () => ChangeToRemoteBranch(branchName), () => { });
            }
            else
            {
                popupManager.OpenDialogWithMessage("Selected active branch. Nothing happens", () => { });
            }
        }

        public void CreateNewBranchWithPrompt(string branchName)
        {
            popupManager.OpenDialogWithMessage("Do you want to continue?", "Yes", () => PlaceablesManager.Instance.SetBranchName(branchName), () => { });
        }

        /// <summary>
        /// Returns the list names of branches. Uses the cachaed values.
        /// </summary>
        public List<string> GetBranches()
        {
            if (branchListCache == null)
            {
                UpdateBranchesList();
                return null;
            }
            return branchListCache.Keys.ToList();
        }

        /// <summary>
        /// Get the last time the branche list caches was updated in human readable form.
        /// </summary>
        public string GetBranchesCacheTime()
        {
            if (branchesListCachedTime == 0)
            {
                return "Uninitialzed";
            }
            DateTime dateTime = new DateTime(branchesListCachedTime);
            return dateTime.ToString("MM/dd/yyyy h:mm tt");
        }
        #endregion
    }

    [System.Serializable]
    public class RemoteStorageData
    {
        public long commit_time;
        public string platform;
        public StorageData data;// StorageData
        public string dataHash;
        public string groupID;
        public string branchName;

        public RemoteStorageData(long commit_time, string platform, StorageData data, string groupID, string branchName)
        {
            this.commit_time = commit_time;
            this.data = data;
            this.platform = platform;
            this.groupID = groupID;
            this.branchName = branchName;
            FillFields();
        }

        public RemoteStorageData(long commit_time, StorageData data, string groupID, string branchName)
        {
            this.commit_time = commit_time;
            this.data = data;
            this.platform = data.lastWrittenPlatform;
            this.groupID = groupID;
            this.branchName = branchName;
            FillFields();
        }

        public void FillFields()
        {
            Hash128 hash = new Hash128();
            HashUtilities.ComputeHash128(ref this.data, ref hash);
            this.dataHash = hash.ToString();
        }

        public StorageData GetData()
        {
            return data;
        }
    }

    public enum HTTPMethod {
        GET, POST, PUT, DELETE
    }
}

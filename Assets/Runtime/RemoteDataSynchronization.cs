using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Collections;

namespace ubc.ok.ovilab.roadmap
{
    [RequireComponent(typeof(PopupManager))]
    public class RemoteDataSynchronization : MonoBehaviour
    {
        private const string SERVER_URL = "https://roadmap-ubco-default-rtdb.firebaseio.com/";
        private const string DB_SCENE_DATA = "scene_data";
        private const string DB_SCENES = "scenes";
        private const string DB_GROUPS = "groups";
        private const string _playerPrefsStorageKey = "RoadMapStorageSyncLastPushed";

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

        #region Unity functions
        private void Start()
        {
            if (PlayerPrefs.HasKey(_playerPrefsStorageKey))
            {
                lastPushedSceneDataId = PlayerPrefs.GetString(_playerPrefsStorageKey);
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
            return $"{PlaceablesManager.Instance.applicationConfig.buildKey}";
        }

        /// <summary>
        /// Get the group id to use.
        /// </summary>
        private string GroupID()
        {
            if (string.IsNullOrEmpty(PlaceablesManager.Instance.applicationConfig.groupID))
            {
                throw new UnityException($"GroupID not set");
            }
            return $"{PlaceablesManager.Instance.applicationConfig.groupID}";
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
        public void SaveSceneData(StorageData data)
        {
            CheckSceneInScenes(() =>
            {
                RemoteStorageData sceneData = new RemoteStorageData(System.DateTime.Now.Ticks, data);

                ProcessRequest($"/{DB_SCENE_DATA}", HTTPMethod.POST, (nameString) =>
                {
                    string name = JsonConvert.DeserializeAnonymousType(nameString, new { name = "" }).name;
                    ProcessRequest($"/{DB_SCENE_DATA}/{name}", HTTPMethod.PUT, (dataString) =>
                    {
                        ProcessRequest($"/{DB_SCENES}/{SceneID()}/{DB_SCENE_DATA}/{name}", HTTPMethod.PUT, (_) =>
                        {
                            LastPushedSceneDataId = name; // Set only if everything went smooth!
                        }, JsonConvert.SerializeObject(sceneData.commit_time));
                    }, JsonConvert.SerializeObject(sceneData, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));

                }, JsonConvert.SerializeObject(true));
            });
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

                /// Clear and write local data
                PlaceablesManager.Instance.ClearData();
                PlaceablesManager.Instance.LoadFromStorageData(result);
                /// Write remote data
                SaveSceneData(result);
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
            ProcessRemoteStorageData((remoteData) =>
            {
                StorageData data = remoteData.GetData();
                // Platform lastWrittenPlatform = System.Enum.Parse<Platform>(data.lastWrittenPlatform);
                // // FIXME: This if check is not needed?
                // if (lastWrittenPlatform != PlatformManager.Instance.currentPlatform)
                // {
                //     foreach (var _group in data.groups)
                //     {
                //         PlatformManager.Instance.ConvertGroupData(_group, lastWrittenPlatform);
                //     }
                // }

                PlaceablesManager.Instance.ClearData();
                PlaceablesManager.Instance.LoadFromStorageData(data);
            });
        }

        /// <summary>
        /// Exectue callback with the latest scene data on remote.
        /// </summary>
        public void ProcessRemoteStorageData(System.Action<RemoteStorageData> callback)
        {
            ProcessRequest($"/{DB_SCENES}/{SceneID()}/{DB_SCENE_DATA}", HTTPMethod.GET, (idStrings) =>
            {
                Dictionary<string, long> sceneData = JsonConvert.DeserializeObject<Dictionary<string, long>>(idStrings);
                string sceneDataId = sceneData.OrderByDescending(kvp => kvp.Value).First().Key;
                ProcessRequest($"/{DB_SCENE_DATA}/{sceneDataId}", HTTPMethod.GET, (dataString) =>
                {
                    RemoteStorageData remoteData = JsonUtility.FromJson<RemoteStorageData>(dataString);
                    callback(remoteData);
                });
            });
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
                    PlaceablesManager.Instance.LoadFromStorageData(localDataBeforeSync);
                }, () => { });
            }, () => { });
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

        public RemoteStorageData(long commit_time, string platform, StorageData data)
        {
            this.commit_time = commit_time;
            this.data = data;
            this.platform = platform;
        }

        public RemoteStorageData(long commit_time, StorageData data)
        {
            this.commit_time = commit_time;
            this.data = data;
            this.platform = data.lastWrittenPlatform;
        }

        public void ComputeHash()
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
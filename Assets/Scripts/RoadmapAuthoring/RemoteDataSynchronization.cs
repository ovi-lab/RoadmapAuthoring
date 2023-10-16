using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Collections;

namespace ubc.ok.ovilab.roadmap
{
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

        private void Start()
        {
            if (PlayerPrefs.HasKey(_playerPrefsStorageKey))
            {
                lastPushedSceneDataId = PlayerPrefs.GetString(_playerPrefsStorageKey);
            }
        }

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
                        ProcessRequest($"/{DB_GROUPS}/{GroupID()}/{DB_SCENES}/{SceneID()}", HTTPMethod.PUT, data:  JsonConvert.SerializeObject(true), action: (_) =>
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
        public void SaveSceneData(LocalStorageData data)
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
            LocalStorageData localData = PlaceablesManager.Instance.GetLocalStorageData();

            ProcessRemoteStorageData((remoteDataStorage) =>
            {
                LocalStorageData remoteData = remoteDataStorage.GetData();

                Dictionary<string, GroupData> remoteDataDict = remoteData.groups.ToDictionary(item => item.identifier);
                Dictionary<string, GroupData> localDataDict = localData.groups.ToDictionary(item => item.identifier);

                Dictionary<string, PlaceableObjectData> remotePlaceables, localPlaceables;
                Dictionary<string, byte> groupStates = new Dictionary<string, byte>(); // 1 - remote, 2 - local, 3 - both

                Dictionary<string, GroupData> groupData = new Dictionary<string, GroupData>();

                localData.groups.ForEach(_group =>
                {
                    /// Combine data that has common keys in both
                    if (remoteDataDict.ContainsKey(_group.identifier))
                    {
                        localPlaceables = _group.PlaceableDataList.ToDictionary(item => item.identifier);
                        remotePlaceables = remoteDataDict[_group.identifier].PlaceableDataList.ToDictionary(item => item.identifier);

                        /// Also have to decide which data of the group to use.
                        /// Setting up to select based on the one with the placeable with the latest update.
                        long remoteLatestUpdate = remotePlaceables.Select(x => x.Value.lastUpdate).Max();
                        bool useRemote = true;

                        foreach(KeyValuePair<string, PlaceableObjectData> localPlaceableKVP in localPlaceables)
                        {
                            /// When a plceable is in both, pick the one that has the largest timestamp
                            if (remotePlaceables.ContainsKey(localPlaceableKVP.Key))
                            {
                                if (localPlaceableKVP.Value.lastUpdate > remotePlaceables[localPlaceableKVP.Key].lastUpdate)
                                {
                                    remotePlaceables[localPlaceableKVP.Key] = localPlaceableKVP.Value;
                                }
                            }
                            /// Add placeables only in local
                            else
                            {
                                remotePlaceables.Add(localPlaceableKVP.Key, localPlaceableKVP.Value);
                            }

                            /// Checking if there is a local updated later than remoteLatestUpdate
                            if (localPlaceableKVP.Value.lastUpdate > remoteLatestUpdate)
                            {
                                useRemote = false;
                            }
                        }

                        /// Selecting based on the one with the placeable with the latest update.
                        /// NOTE: The placeable is relative to the group origin, hence,
                        /// the group origin is the one that has to have the coordinate transformation
                        /// placeable data should be usable as is
                        if (useRemote)
                        {
                            groupData[_group.identifier] = remoteDataDict[_group.identifier];
                            groupStates[_group.identifier] = 1;
                        }
                        else
                        {
                            groupData[_group.identifier] = _group;
                            groupStates[_group.identifier] = 2;
                        }

                        /// Making sure the PlaceableDataList is the combined one
                        groupData[_group.identifier].PlaceableDataList = remotePlaceables.Values.ToList();
                    }
                    /// _group is only in localData, add as is
                    else
                    {
                        groupData[_group.identifier] = _group;
                        groupStates[_group.identifier] = 2;
                    }
                });

                /// Adding groups only in remote
                remoteData.groups.ForEach(_group =>
                {
                    if (!groupData.ContainsKey(_group.identifier))
                    {
                        groupData[_group.identifier] = _group;
                        groupStates[_group.identifier] = 1;
                    }
                });

                // FIXME: This if check is not needed?
                /// if the platforms are different transfrom data
                if (remoteData.lastWrittenPlatform != localData.lastWrittenPlatform)
                {
                    foreach (var _group in groupData)
                    {
                        if (groupStates[_group.Key] == 1)
                        {
                            PlatformManager.Instance.ConvertGroupData(_group.Value, System.Enum.Parse<Platform>(localData.lastWrittenPlatform));
                        }
                    }
                }

                /// localData has the current platform set as LastWrittenPlatform
                LocalStorageData result = new LocalStorageData(groupData.Values.ToList(), localData.lastWrittenPlatform);

                /// Clear and write local data
                PlaceablesManager.Instance.ClearData();
                PlaceablesManager.Instance.LoadFromLocalStorageData(result);
                /// Write remote data
                SaveSceneData(result);
            });
        }

        /// <summary>
        /// Make the current scene the latest version on the remote.
        /// </summary>
        public void OverwriteRemote()
        {
            SaveSceneData(PlaceablesManager.Instance.GetLocalStorageData());
        }

        /// <summary>
        /// Make the latest version on the remote the current scene.
        /// </summary>
        public void OverwriteLocal()
        {
            ProcessRemoteStorageData((remoteData) =>
            {
                LocalStorageData data = remoteData.GetData();
                Platform lastWrittenPlatform = System.Enum.Parse<Platform>(data.lastWrittenPlatform);
                // FIXME: This if check is not needed?
                if (lastWrittenPlatform != PlatformManager.Instance.currentPlatform)
                {
                    foreach (var _group in data.groups)
                    {
                        PlatformManager.Instance.ConvertGroupData(_group, lastWrittenPlatform);
                    }
                }

                PlaceablesManager.Instance.ClearData();
                PlaceablesManager.Instance.LoadFromLocalStorageData(data);
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

        /// <summary>
        /// Helper method to execute web request asynchronously.
        /// </summary>
        protected void ProcessRequest(string endpoint, HTTPMethod method, System.Action<string> action=null, string data="")
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
        protected IEnumerator GetJsonUrl(string endpoint, HTTPMethod method, System.Action<string> action=null, string data="")
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
    }

    [System.Serializable]
    public class RemoteStorageData
    {
        public long commit_time;
        public string platform;
        public LocalStorageData data;// LocalStorageData

        public RemoteStorageData(long commit_time, string platform, LocalStorageData data)
        {
            this.commit_time = commit_time;
            this.data = data;
            this.platform = platform;
        }

        public RemoteStorageData(long commit_time, LocalStorageData data)
        {
            this.commit_time = commit_time;
            this.data = data;
            this.platform = data.lastWrittenPlatform;
        }

        public LocalStorageData GetData()
        {
            return data;
        }
    }

    public enum HTTPMethod {
        GET, POST, PUT, DELETE
    }
}

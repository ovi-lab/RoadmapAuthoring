using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Collections;
using System;

namespace ubc.ok.ovilab.roadmap
{
    public class RemoteDataSynchronization : MonoBehaviour
    {
        private const string SERVER_URL = "https://roadmap-ubco-default-rtdb.firebaseio.com/";
        private const string DB_SCENE_DATA = "scene_data";
        private const string DB_SCENES = "scenes";
        private const string DB_GROUPS = "groups";

        // See `docs/_calculating_translation.org` for how this is calculated
        private float zLatOffset = -119.3963448f;
        private float xLonOffset = 49.93952982f;
        private float zLatFactor = 71755.33313297f;
        private float xLonFactor = 111273.39342956f;

        private string SceneID()
        {
            return $"{PlaceablesManager.Instance.applicationConfig.buildKey}";
        }

        private string GroupID()
        {
            if (string.IsNullOrEmpty(PlaceablesManager.Instance.applicationConfig.groupID))
            {
                throw new UnityException($"GroupID not set");
            }
            return $"{PlaceablesManager.Instance.applicationConfig.groupID}";
        }

        /// Run callable after verifying the scene exists in "scenes"
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
                        ProcessRequest($"/{DB_SCENES}/{SceneID()}/{DB_SCENE_DATA}/{name}", HTTPMethod.PUT, null, JsonConvert.SerializeObject(sceneData.commit_time));
                    }, JsonConvert.SerializeObject(sceneData, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
                }, JsonConvert.SerializeObject(true));

            });
        }

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
                                remotePlaceables[localPlaceableKVP.Key] = localPlaceableKVP.Value;
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
                    groupData[_group.identifier] = _group;
                    groupStates[_group.identifier] = 1;
                });

                /// if the platforms are different transfrom data
                if (remoteData.lastWrittenPlatform != localData.lastWrittenPlatform)
                {
                    switch (System.Enum.Parse<Platform>(localData.lastWrittenPlatform))
                    {
                        case Platform.Oculus:
                            foreach (var _group in groupData)
                            {
                                if (groupStates[_group.Key] == 1)
                                {
                                    ArToVr(_group.Value);
                                }
                            }
                            break;
                        case Platform.ARCore:
                            foreach (var _group in groupData)
                            {
                                if (groupStates[_group.Key] == 1)
                                {
                                    VrtoAr(_group.Value);
                                }
                            }
                            break;
                        default:
                            throw new System.NotImplementedException();
                    }
                }

                /// localData has the current platform set as LastWrittenPlatform
                LocalStorageData result = new LocalStorageData(groupData.Values.ToList(), localData.lastWrittenPlatform);

                /// Write local data
                PlaceablesManager.Instance.LoadFromLocalStorageData(result);
                /// Write remote data
                SaveSceneData(result);
            });
        }

        public void OverwriteRemote()
        {
            SaveSceneData(PlaceablesManager.Instance.GetLocalStorageData());
        }

        public void OverwriteLocal()
        {
            ProcessRemoteStorageData((remoteData) =>
            {
                LocalStorageData data = remoteData.GetData();
                Platform lastWrittenPlatform = System.Enum.Parse<Platform>(data.lastWrittenPlatform);
                if (lastWrittenPlatform != PlatformManager.Instance.currentPlatform)
                {
                    foreach (var _group in data.groups)
                    {
                        switch (lastWrittenPlatform)
                        {
                            case Platform.Oculus:
                                VrtoAr(_group);
                                break;
                            case Platform.ARCore:
                                ArToVr(_group);
                                break;
                            default:
                                throw new System.NotImplementedException();
                        }
                    }
                }

                PlaceablesManager.Instance.LoadFromLocalStorageData(data);
            });
        }

        public void ProcessRemoteStorageData(System.Action<RemoteStorageData> callback)
        {
            ProcessRequest($"/{DB_SCENES}/{SceneID()}/{DB_SCENE_DATA}", HTTPMethod.GET, (idStrings) =>
            {
                Dictionary<string, long> sceneData = JsonConvert.DeserializeObject<Dictionary<string, long>>(idStrings);
                string sceneDataId = sceneData.OrderByDescending(kvp => kvp.Key).First().Key;
                ProcessRequest($"/{DB_SCENE_DATA}/{sceneDataId}", HTTPMethod.GET, (dataString) =>
                {
                    RemoteStorageData remoteData = JsonUtility.FromJson<RemoteStorageData>(dataString);
                    callback(remoteData);
                });
            });
        }

        protected GroupData VrtoAr(GroupData data)
        {
            data.Latitude = data.Latitude / zLatFactor + zLatOffset;
            data.Longitude = data.Longitude / xLonFactor + xLonOffset;
            data.Altitude = 0; // Using the terrain coordinates, sets to zero on ground
            return data;
        }

        protected GroupData ArToVr(GroupData data)
        {
            data.Latitude = (data.Latitude - zLatOffset)  * zLatFactor;
            data.Longitude = (data.Longitude - xLonOffset) * xLonFactor;

            Vector3 rayOrigin = new Vector3((float)data.Longitude, (float)data.Latitude, 20);

            RaycastHit hit;
            // Does the ray intersect terrain TODO: terrain layer?
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity, 8))
            {
                data.Altitude = hit.point.y;
            }
            else
            {
                throw new Exception("Tarrain missed by transformed coordinates for ArToVr.");
            }
            return data;
        }

        protected void ProcessRequest(string endpoint, HTTPMethod method, System.Action<string> action=null, string data="")
        {
            StartCoroutine(GetJsonUrl(endpoint, method, action, data));
        }

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
                default:
                    throw new System.NotImplementedException();
            }
        }

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
        GET, POST, PUT
    }
}

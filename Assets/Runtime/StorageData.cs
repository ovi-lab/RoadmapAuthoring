using System.Collections.Generic;
using System.Linq;

namespace ubc.ok.ovilab.roadmap
{
    [System.Serializable]
    public struct StorageData
    {
        public List<GroupData> groups;
        public string lastWrittenPlatform;
        public string buildKey;
        public string branchName;

        public StorageData(List<GroupData> groups, string lastWrittenPlatform, string buildKey, string branchName)
        {
            this.groups = groups;
            this.lastWrittenPlatform = lastWrittenPlatform;
            this.buildKey = buildKey;
            this.branchName = branchName;
        }

        public override string ToString()
        {
            return $"plaform: {lastWrittenPlatform}/{branchName} with {buildKey}\ngroups[{groups.Count}]:\n " + string.Join("\n ", groups);
        }

        /// <summary>
        /// Merge the data from two StorageData items. The
        /// lastWrittenPlatform and buildKey will set as the values in
        /// the returned merged StorageData.
        /// </summary>
        public static StorageData MergeData(StorageData dataA, StorageData dataB, string lastWrittenPlatform, string buildKey, string branchName)
        {
            Dictionary<string, GroupData> dataADict = dataA.groups.ToDictionary(item => item.identifier);
            Dictionary<string, GroupData> dataBDict = dataB.groups.ToDictionary(item => item.identifier);

            Dictionary<string, PlaceableObjectData> dataAPlaceables, dataBPlaceables;
            Dictionary<string, byte> groupStates = new Dictionary<string, byte>(); // 1 - A, 2 - B, 3 - both

            Dictionary<string, GroupData> groupData = new Dictionary<string, GroupData>();

            dataB.groups.ForEach(_group =>
            {
                /// Combine data that has common keys in both
                if (dataADict.ContainsKey(_group.identifier))
                {
                    dataBPlaceables = _group.PlaceableDataList.ToDictionary(item => item.identifier);
                    dataAPlaceables = dataADict[_group.identifier].PlaceableDataList.ToDictionary(item => item.identifier);

                    /// Also have to decide which data of the group to use.
                    /// Setting up to select based on the one with the placeable with the latest update.
                    long dataALatestUpdate = dataAPlaceables.Select(x => x.Value.lastUpdate).Max();
                    bool useDataA = true;

                    foreach (KeyValuePair<string, PlaceableObjectData> localPlaceableKVP in dataBPlaceables)
                    {
                        /// When a plceable is in both, pick the one that has the largest timestamp
                        if (dataAPlaceables.ContainsKey(localPlaceableKVP.Key))
                        {
                            if (localPlaceableKVP.Value.lastUpdate > dataAPlaceables[localPlaceableKVP.Key].lastUpdate)
                            {
                                dataAPlaceables[localPlaceableKVP.Key] = localPlaceableKVP.Value;
                            }
                        }
                        /// Add placeables only in local
                        else
                        {
                            dataAPlaceables.Add(localPlaceableKVP.Key, localPlaceableKVP.Value);
                        }

                        /// Checking if there is a update in B later than dataALatestUpdate
                        if (localPlaceableKVP.Value.lastUpdate > dataALatestUpdate)
                        {
                            useDataA = false;
                        }
                    }

                    /// Selecting based on the one with the placeable with the latest update.
                    /// NOTE: The placeable is relative to the group origin, hence,
                    /// the group origin is the one that has to have the coordinate transformation
                    /// placeable data should be usable as is
                    if (useDataA)
                    {
                        groupData[_group.identifier] = dataADict[_group.identifier];
                        groupStates[_group.identifier] = 1;
                    }
                    else
                    {
                        groupData[_group.identifier] = _group;
                        groupStates[_group.identifier] = 2;
                    }

                    /// Making sure the PlaceableDataList is the combined one
                    groupData[_group.identifier].PlaceableDataList = dataAPlaceables.Values.ToList();
                }
                /// _group is only in localData, add as is
                else
                {
                    groupData[_group.identifier] = _group;
                    groupStates[_group.identifier] = 2;
                }
            });

            /// Adding groups only in A
            dataA.groups.ForEach(_group =>
            {
                if (!groupData.ContainsKey(_group.identifier))
                {
                    groupData[_group.identifier] = _group;
                    groupStates[_group.identifier] = 1;
                }
            });

            return new StorageData(groupData.Values.ToList(), lastWrittenPlatform, buildKey, branchName);
        }
    }
}

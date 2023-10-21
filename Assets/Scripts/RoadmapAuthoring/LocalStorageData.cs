using System.Collections.Generic;

namespace ubc.ok.ovilab.roadmap
{
    [System.Serializable]
    public class LocalStorageData
    {
        public List<GroupData> groups;
        public string lastWrittenPlatform;
        public string buildKey;

        public LocalStorageData(List<GroupData> groups, string lastWrittenPlatform, string buildKey)
        {
            this.groups = groups;
            this.lastWrittenPlatform = lastWrittenPlatform;
            this.buildKey = buildKey;
        }

        public override string ToString()
        {
            return $"plaform: {lastWrittenPlatform} with {buildKey}\ngroups[{groups.Count}]:\n " + string.Join("\n ", groups);
        }
    }
}

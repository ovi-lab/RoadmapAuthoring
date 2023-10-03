using System.Collections.Generic;

namespace ubc.ok.ovilab.roadmap
{
    [System.Serializable]
    public class LocalStorageData
    {
        public List<GroupData> groups;
        public string lastWrittenPlatform;
        public LocalStorageData(List<GroupData> groups, string lastWrittenPlatform)
        {
            this.groups = groups;
            this.lastWrittenPlatform = lastWrittenPlatform;
        }
    }
}

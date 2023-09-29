using System.Collections.Generic;

namespace ubc.ok.ovilab.roadmap
{
    [System.Serializable]
    public class LocalStorageData
    {
        public List<GroupData> Groups;
        public LocalStorageData(List<GroupData> groups)
        {
            Groups = groups;
        }
    }
}

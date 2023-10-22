using System.Collections.Generic;

namespace ubc.ok.ovilab.roadmap
{
    [System.Serializable]
    public class GroupData
    {
        public string identifier;
        public List<PlaceableObjectData> PlaceableDataList;

        public GroupData(string identifier, List<PlaceableObjectData> placeableDataList)
        {
            this.identifier = identifier;
            PlaceableDataList = placeableDataList;
        }

        public override string ToString()
        {
            return $"Group ({identifier} with[{PlaceableDataList.Count}]:\n   " + string.Join("\n   ", PlaceableDataList);
        }
    }
}

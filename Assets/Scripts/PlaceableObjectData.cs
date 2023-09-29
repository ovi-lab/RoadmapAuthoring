using UnityEngine;

namespace ubc.ok.ovilab.roadmap
{
    [System.Serializable]
    public class PlaceableObjectData
    {
        public string PrefabIdentifier;
        public Pose LocalPose;
        public string AuxData;

        public PlaceableObjectData(string prefabIdentifier, Pose localPose, string auxData = null)
        {
            PrefabIdentifier = prefabIdentifier;
            LocalPose = localPose;
            AuxData = auxData;
        }
    }
}

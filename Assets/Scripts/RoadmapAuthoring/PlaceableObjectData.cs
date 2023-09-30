using UnityEngine;

namespace ubc.ok.ovilab.roadmap
{
    [System.Serializable]
    public class PlaceableObjectData
    {
        public string PrefabIdentifier;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public string AuxData;

        public PlaceableObjectData(string prefabIdentifier, Vector3 localPosition, Quaternion localRotation, string auxData = null)
        {
            PrefabIdentifier = prefabIdentifier;
            this.localPosition = localPosition;
            this.localRotation = localRotation;
            AuxData = auxData;
        }
    }
}

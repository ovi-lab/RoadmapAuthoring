using UnityEngine;

namespace ubc.ok.ovilab.roadmap
{
    [System.Serializable]
    public class PlaceableObjectData
    {
        public string PrefabIdentifier;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
        public string AuxData;

        public PlaceableObjectData(string prefabIdentifier, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, string auxData = null)
        {
            PrefabIdentifier = prefabIdentifier;
            this.localPosition = localPosition;
            this.localRotation = localRotation;
            this.localScale = localScale;
            AuxData = auxData;
        }
    }
}

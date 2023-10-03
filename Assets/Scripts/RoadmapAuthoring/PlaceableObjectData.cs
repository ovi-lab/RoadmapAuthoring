using UnityEngine;

namespace ubc.ok.ovilab.roadmap
{
    [System.Serializable]
    public class PlaceableObjectData
    {
        public string prefabIdentifier;
        public string identifier;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
        public string auxData;
        public long lastUpdate;

        public PlaceableObjectData(string prefabIdentifier, string identifier, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, long lastUpdate, string auxData = null)
        {
            this.prefabIdentifier = prefabIdentifier;
            this.identifier = identifier;
            this.localPosition = localPosition;
            this.localRotation = localRotation;
            this.localScale = localScale;
            this.auxData = auxData;
            this.lastUpdate = lastUpdate;
        }
    }
}

using UnityEngine;
using System;

namespace ubc.ok.ovilab.roadmap
{
    [Serializable]
    public struct GroupCoordinateData
    {
        public string identifier;

        // GPS coordinates
        public double latitude;
        public double longitude;
        public double altitude;

        // cartesian coordinates
        public Vector3 position;

        // Common for both
        public Quaternion rotation;

        public override string ToString()
        {
            return $"{identifier} {latitude},{longitude},{altitude} - {position} - {rotation}";
        }
    }
}

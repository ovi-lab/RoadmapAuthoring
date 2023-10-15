using System;
using UnityEngine;

namespace ubc.ok.ovilab.roadmap
{
    public enum Platform { Oculus, ARCore }

    /// <summary>
    /// Singleton Class
    /// Handles switching between different platforms. Depending on which paltform is detected
    /// the appropriate components will be configured and used.
    /// </summary>
    [DefaultExecutionOrder(-100)] // We want all the configs done before any related sripts are executed
    public class PlatformManager : Singleton<PlatformManager>
    {
        [SerializeField] public Platform currentPlatform;

        // See `docs/_calculating_translation.org` for how this is calculated
        private float zLatOffset = -119.3963448f;
        private float xLonOffset = 49.93952982f;
        private float zLatFactor = 71755.33313297f;
        private float xLonFactor = 111273.39342956f;

        /// <summary>
        /// Convert group date between platforms.
        /// If the `convertingFrom` platform is the same as the
        /// current, it woul make not changes to the groupd data
        /// passed.
        /// </summary>
        public void ConvertGroupData(GroupData data, Platform convertingFrom)
        {
            if (currentPlatform == convertingFrom)
            {
                return;
            }

            switch(convertingFrom)
            {
                case Platform.Oculus:
                    VrtoAr(data);
                    break;
                case Platform.ARCore:
                    ArToVr(data);
                    break;
            }
        }

        /// <summary>
        /// Convert group data from vr to ar
        /// </summary>
        protected GroupData VrtoAr(GroupData data)
        {
            data.Latitude = data.Latitude / zLatFactor + zLatOffset;
            data.Longitude = data.Longitude / xLonFactor + xLonOffset;
            data.Altitude = 0; // Using the terrain coordinates, sets to zero on ground
            return data;
        }

        /// <summary>
        /// Convert group data from ar to vr
        /// </summary>
        protected GroupData ArToVr(GroupData data)
        {
            data.Latitude = (data.Latitude - zLatOffset)  * zLatFactor;
            data.Longitude = (data.Longitude - xLonOffset) * xLonFactor;

            Vector3 rayOrigin = new Vector3((float)data.Longitude, (float)data.Latitude, 20);

            RaycastHit hit;
            // Does the ray intersect terrain
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity, LayerMask.NameToLayer("Terrain")))
            {
                data.Altitude = hit.point.y;
            }
            else
            {
                throw new Exception("Tarrain missed by transformed coordinates for ArToVr.");
            }
            return data;
        }

    }
}

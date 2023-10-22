using System;
using System.Collections.Generic;
using UnityEngine;

namespace ubc.ok.ovilab.roadmap
{
    [CreateAssetMenu(fileName = "Data", menuName = "Roadmap/SceneGroupData", order = 1)]
    public class SceneGroupsData : ScriptableObject
    {
        // // See `docs/_calculating_translation.org` for how this is calculated
        // [SerializeField] private float zLatOffset = 49.93952982f;
        // [SerializeField] private float xLonOffset = -119.3963448f;
        // [SerializeField] private float zLatFactor = 111273.39342956f;
        // [SerializeField] private float xLonFactor = 71755.33313297f;
        public List<GroupCoordinateData> groups;

        // /// <summary>
        // /// Compute the cartesian coordinates from the GPS coordinates
        // /// </summary>
        // public void UpdateCartesianCoords(GroupCoordinateData coordinateData)
        // {
        //     // x - longitude; z - latitude
        //     coordinateData.position = new Vector3((float)((coordinateData.longitude - xLonOffset) * xLonFactor), 0, (float)((coordinateData.latitude - zLatOffset) * zLatFactor));

        //     Vector3 rayOrigin = new Vector3(coordinateData.position.x, 20, coordinateData.position.z);

        //     RaycastHit hit;
        //     // Does the ray intersect terrain
        //     if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity, LayerMask.NameToLayer("Terrain")))
        //     {
        //         coordinateData.position.z = hit.point.y;
        //     }
        //     else
        //     {
        //         throw new Exception("Tarrain missed by transformed coordinates for ArToVr.");
        //     }
        // }

        // /// <summary>
        // /// Compute the GPS coordinates from the cartesian coordinates
        // /// </summary>
        // public void UpdateGPSCoordinates(GroupCoordinateData coordinateData)
        // {
        //     coordinateData.latitude = coordinateData.position.z / zLatFactor + zLatOffset;
        //     coordinateData.longitude = coordinateData.position.x / xLonFactor + xLonOffset;
        //     coordinateData.altitude = 0; // Using the terrain coordinates, sets to zero on ground
        // }
    }
}

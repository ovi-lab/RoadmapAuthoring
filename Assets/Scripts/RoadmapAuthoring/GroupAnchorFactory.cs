using UnityEngine;

namespace ubc.ok.ovilab.roadmap
{
    /// <summary>
    /// Factory to generate groups.
    /// </summary>
    public class GroupAnchorFactory : MonoBehaviour
    {
        /// <summary>
        /// Initializes a new instance of PlaceblesGroup and return it."
        /// </summary>
        public virtual PlaceablesGroup GetPlaceablesGroup(GroupData groupData, System.Action<PlaceableObject> onClickedCallback)
        {
            GameObject groupObject = new GameObject("Group");

            if (groupData != null)
            {
                transform.position = new Vector3((float)groupData.Longitude, (float)groupData.Altitude, (float)groupData.Latitude);
                // z is the north!
                transform.rotation = Quaternion.AngleAxis((float)groupData.Heading, Vector3.up);

            }
            else
            {
                /// z is north, x is east
                /// heading is the camera's forward vector projected on the xz plane (y is the plane normal vector)
                // Heading not needed in the VR scene
                // float heading = Vector3.Angle(-Vector3.forward,
                //                               Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up));
                transform.position = Camera.main.transform.position;
                transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
            }

            PlaceablesGroup placeablesGroup = groupObject.AddComponent<PlaceablesGroup>();
            placeablesGroup.Init(groupData, onClickedCallback);
            return placeablesGroup;
        }
    }
}

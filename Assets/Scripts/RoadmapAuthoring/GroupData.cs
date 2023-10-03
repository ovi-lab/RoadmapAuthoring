using System.Collections.Generic;

namespace ubc.ok.ovilab.roadmap
{
    [System.Serializable]
    public class GroupData
    {
        public string identifier;
        public double Latitude;
        public double Longitude;
        public double Altitude;
        public double Heading;
        public List<PlaceableObjectData> PlaceableDataList;

        public GroupData(string identifier, double latitude, double longitude, double altitude, double heading, List<PlaceableObjectData> placeableDataList)
        {
            this.identifier = identifier;
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
            Heading = heading;
            PlaceableDataList = placeableDataList;
        }
    }
}

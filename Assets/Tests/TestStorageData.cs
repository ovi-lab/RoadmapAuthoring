using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace ubc.ok.ovilab.roadmap.test
{
    public class TestStorageData
    {
        [Test]
        public void Test_StorageData_MergeData()
        {
            // Data for A
            PlaceableObjectData d1 = new PlaceableObjectData("1", "1", Vector3.down, Quaternion.identity, Vector3.one, 5);
            PlaceableObjectData d2 = new PlaceableObjectData("2", "2", Vector3.up, Quaternion.identity, Vector3.one, 10);
            // Data for B
            PlaceableObjectData d3 = new PlaceableObjectData("1", "1", Vector3.up, Quaternion.identity, Vector3.one, 10);
            PlaceableObjectData d4 = new PlaceableObjectData("3", "3", Vector3.up, Quaternion.identity, Vector3.one, 10);

            GroupData a1 = new GroupData("g1", new List<PlaceableObjectData>() { d1 });
            GroupData a2 = new GroupData("g2", new List<PlaceableObjectData>() { d2 });

            GroupData b1 = new GroupData("g1", new List<PlaceableObjectData>() { d3 });
            GroupData b2 = new GroupData("g3", new List<PlaceableObjectData>() { d4 });

            StorageData A = new StorageData(new List<GroupData>() { a1, a2 }, "-", "-");
            StorageData B = new StorageData(new List<GroupData>() { b1, b2 }, "-", "-");

            StorageData result = StorageData.MergeData(A, B, "-", "-");

            Dictionary<string, GroupData> groupData = result.groups.ToDictionary(g => g.identifier, g => g);

            Assert.AreEqual(groupData.Count, 3, "There are 3 groups");
            Assert.IsTrue(groupData.All(g => g.Value.PlaceableDataList.Count == 1),  "One object in each");
            Assert.IsTrue(groupData.All(g => g.Value.PlaceableDataList[0].localPosition == Vector3.up), "All objects are as expected (localPosition=Vector3.up)");
            Assert.IsTrue(groupData.All(g => g.Value.PlaceableDataList[0].lastUpdate == 10), "All objects are as expected (lastUpdate=10)");
        }
    }
}

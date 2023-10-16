using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ubc.ok.ovilab.roadmap
{
    /// <summary>
    /// Used to populate the items for the add button on the UI
    /// </summary>
    public class DropdownController : MonoBehaviour
    {
        [Tooltip("The prefab to use to generate items in the dropdown.")]
        public GameObject itemPrefab;
        [Tooltip("The root object of the dropdown menu.")]
        public GameObject menuRoot;

        private void Start()
        {
            RoadmapApplicationConfig config = PlaceablesManager.Instance.applicationConfig;
            foreach(string placeableIdentifer in config.PlacableIdentifierList())
            {
                GameObject go = Instantiate(itemPrefab, this.transform);
                go.transform.name = placeableIdentifer;
                go.GetComponentInChildren<TextMeshProUGUI>().text = placeableIdentifer;
                go.GetComponent<Button>().onClick.AddListener(() => {
                    PlaceablesManager.Instance.SpawnObject(placeableIdentifer);
                    menuRoot.SetActive(false);
                });
            }
        }
    }
}

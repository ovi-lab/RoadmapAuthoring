using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ubc.ok.ovilab.roadmap.UI
{
    /// <summary>
    /// Used to populate the items for the add button on the UI
    /// </summary>
    public class DropdownController : ScrollListManager
    {
        [Tooltip("The prefab to use to generate items in the dropdown.")]
        public GameObject itemPrefab;

        private List<GameObject> scrollItems = new List<GameObject>();

        public override void SetupScrollList(List<ScrollListItem> items)
        {
            foreach(GameObject go in scrollItems)
            {
                Destroy(go);
            }

            scrollItems.Clear();

            foreach(ScrollListItem item in items)
            {
                GameObject go = Instantiate(itemPrefab, this.transform);
                go.transform.name = item.identifier;
                go.GetComponentInChildren<TextMeshProUGUI>().text = item.identifier;
                go.GetComponent<Button>().onClick.AddListener(() => {
                    item.callback.Invoke();
                    menuRoot.SetActive(false);
                });
                scrollItems.Add(go);
            }
        }
    }
}

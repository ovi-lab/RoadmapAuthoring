// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for samples. While nice to have, this XML documentation is not required for samples.
#pragma warning disable CS1591

using TMPro;
using UnityEngine;
using MixedReality.Toolkit.UX.Experimental;
using MixedReality.Toolkit.UX;

namespace ubc.ok.ovilab.roadmap.UI
{
    // NOTE: Copied from the MRTK3 VirtualizedScrollRectList example
    public class PlaceablesVirtualizedScrollList : MonoBehaviour
    {
        [SerializeField] private GameObject menuRoot;
        private VirtualizedScrollRectList virtualizedList;
        private float destScroll;
        private bool animate;
        
        /// <summary>
        /// A Unity event function that is called on the frame when a script is enabled just before any of the update methods are called the first time.
        /// </summary> 
        private void Start()
        {
            RoadmapApplicationConfig config = PlaceablesManager.Instance.applicationConfig;
            virtualizedList = GetComponent<VirtualizedScrollRectList>();
            virtualizedList.SetItemCount(config.NumberOfPlaceables());
            virtualizedList.OnVisible = (go, i) =>
            {
                string identifier = config.PlaceableIndexToName(i);
                foreach (var text in go.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    if (text.gameObject.name == "Text")
                    {
                        text.text = identifier;
                    }
                }
                go.GetComponent<PressableButton>().OnClicked.AddListener(() => {
                    PlaceablesManager.Instance.SpawnObject(identifier);
                    menuRoot.SetActive(false);
                });
            };
        }

        /// <summary>
        /// A Unity event function that is called every frame, if this object is enabled.
        /// </summary>
        private void Update()
        {
            if (animate)
            {
                float newScroll = Mathf.Lerp(virtualizedList.Scroll, destScroll, 8 * Time.deltaTime);
                virtualizedList.Scroll = newScroll;
                if (Mathf.Abs(virtualizedList.Scroll - destScroll) < 0.02f)
                {
                    virtualizedList.Scroll = destScroll;
                    animate     = false;
                }
            }
        }

        /// <summary>
        /// Scrolls the VirtualizedScrollRect to the next page.
        /// </summary>
        public void Next()
        {
            animate    = true;
            destScroll = Mathf.Min(virtualizedList.MaxScroll, Mathf.Floor(virtualizedList.Scroll / virtualizedList.RowsOrColumns) * virtualizedList.RowsOrColumns + virtualizedList.TotallyVisibleCount);
        }
        /// <summary>
        /// Scrolls the VirtualizedScrollRect to the previous page.
        /// </summary>
        public void Prev()
        {
            animate    = true;
            destScroll = Mathf.Max(0, Mathf.Floor(virtualizedList.Scroll / virtualizedList.RowsOrColumns) * virtualizedList.RowsOrColumns - virtualizedList.TotallyVisibleCount);
        }
    }
}
#pragma warning restore CS1591

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ubc.ok.ovilab.roadmap.UI
{
    public abstract class ScrollListManager : MonoBehaviour
    {
        [Tooltip("The root object of the dropdown menu.")]
        public GameObject menuRoot;

        public abstract void SetupScrollList(List<ScrollListItem> items);

        /// <summary>
        /// Show the list of placeable objects to spawn
        /// </summary>
        public void ShowPlaceablesList()
        {
            RoadmapApplicationConfig config = RoadmapApplicationConfig.activeApplicationConfig;
            SetupScrollList(config.PlacableIdentifierList()
                            .Select(i => new ScrollListItem(i,
                                                             () => PlaceablesManager.Instance.SpawnObject(i)))
                            .ToList());
            menuRoot.SetActive(true);
        }

        /// <summary>
        /// Change the active branch name
        /// </summary>
        public void ChangeToDifferentBranch()
        {
            SetupScrollList(RemoteDataSynchronization.Instance.GetBranches()
                            .Select(i => new ScrollListItem(i,
                                                            () =>
                                                            {
                                                                RemoteDataSynchronization.Instance.ChangeToRemoteBranchWithPrompt(i);
                                                                BranchDataManager.Instance.Refresh();
                                                            }))
                            .ToList());
            menuRoot.SetActive(true);
        }
    }

    public struct ScrollListItem
    {
        public string identifier;
        public System.Action callback;

        public ScrollListItem(string identifier, Action callback)
        {
            this.callback = callback;
            this.identifier = identifier;
        }
    }
}

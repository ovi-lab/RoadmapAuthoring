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
            List<string> items = RemoteDataSynchronization.Instance.GetBranches();
            if (items != null)
            {
                SetupScrollList(items
                            .Select(i => new ScrollListItem(i,
                                                            () =>
                                                            {
                                                                RemoteDataSynchronization.Instance.ChangeToRemoteBranchWithPrompt(i);
                                                                BranchDataManager.Instance.Refresh();
                                                            }))
                            .ToList());
            }
            else
            {
                // Setting up empty scroll list
                SetupScrollList(new List<ScrollListItem>());
            }
            menuRoot.SetActive(true);
        }

        /// <summary>
        /// Merge with a remote branch
        /// </summary>
        public void MergeWithBranch()
        {
            List<string> items = RemoteDataSynchronization.Instance.GetBranches();
            if (items != null)
            {
                SetupScrollList(items
                            .Select(i => new ScrollListItem(i,
                                                            () =>
                                                            {
                                                                RemoteDataSynchronization.Instance.MergeWithRemoteBranchWithPrompt(i);
                                                                BranchDataManager.Instance.Refresh();
                                                            }))
                            .ToList());
            }
            else
            {
                // Setting up empty scroll list
                SetupScrollList(new List<ScrollListItem>());
            }
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

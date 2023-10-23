using UnityEngine;
using TMPro;
using System;

namespace ubc.ok.ovilab.roadmap.UI
{
    public class BranchDataManager : Singleton<BranchDataManager>
    {

        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private GameObject dialogRoot;
        [SerializeField] private TMP_Text branchNameDisplay;
        [SerializeField] private TMP_Text branchUpdateTimeDisplay;
        private bool refreshSet;
        private DateTime refreshSetAt;
        private TimeSpan refreshTimspan = new TimeSpan(0, 0, 10); // 10 seconds

        public void OnDone()
        {
            string newBranch = inputField.text;
            if (!string.IsNullOrEmpty(newBranch))
            {
                dialogRoot.SetActive(false);
                RemoteDataSynchronization.Instance.CreateNewBranchWithPrompt(newBranch);
                Refresh();
            }
        }

        public void Refresh()
        {
            refreshSet = true;
            refreshSetAt = DateTime.Now;
        }

        private void RefreshInternal()
        {
            branchNameDisplay.text = PlaceablesManager.Instance.BranchName;
            branchUpdateTimeDisplay.text = RemoteDataSynchronization.Instance.GetBranchesCacheTime();
        }

        private void Start()
        {
            Refresh();
        }

        private void Update()
        {
            // NOTE: Because of the async nature of some related calls, continue to refresh for 10 seconds after a refresh call was made
            if (refreshSet)
            {
                if ((DateTime.Now - refreshSetAt) < refreshTimspan)
                {
                    RefreshInternal();
                }
                else
                {
                    refreshSet = false;
                }
            }
        }
    }
}

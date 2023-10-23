using UnityEngine;
using TMPro;

namespace ubc.ok.ovilab.roadmap.UI
{
    public class BranchDataManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private GameObject dialogRoot;
        [SerializeField] private TMP_Text branchNameDisplay;

        public void OnDone()
        {
            string newBranch = inputField.text;
            if (!string.IsNullOrEmpty(newBranch))
            {
                dialogRoot.SetActive(false);
                RemoteDataSynchronization.Instance.CreateNewBranchWithPrompt(newBranch);
            }
        }

        private void Update()
        {
            if (branchNameDisplay.gameObject.activeInHierarchy)
            {
                branchNameDisplay.text = PlaceablesManager.Instance.BranchName;
            }
        }
    }
}

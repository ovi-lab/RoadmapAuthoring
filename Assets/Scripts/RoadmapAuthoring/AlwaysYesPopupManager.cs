using UnityEngine;

namespace ubc.ok.ovilab.roadmap
{
    /// <summary>
    /// Simple popup manager that always immediately runs the yeaCallback.
    /// </summary>
    public class AlwaysYesPopupManager :PopupManager
    {
        public override void OpenDialogWithMessage(string text, string yeaString, System.Action yeaCallback, string neaString, System.Action neaCallback, System.Action dismissCallback)
        {
            Debug.Log($"[AlwaysYesPopup] {text}: calling {yeaString}");
            yeaCallback.Invoke();
            dismissCallback.Invoke();
        }

        // Nothing to do here
        public override void DismissPopup() {}
    }
}

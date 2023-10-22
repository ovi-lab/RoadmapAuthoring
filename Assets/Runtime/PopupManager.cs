using UnityEngine;

namespace ubc.ok.ovilab.roadmap
{
    public abstract class PopupManager: MonoBehaviour
    {
        /// <summary>
        /// Show a dialog. This can be dimissed with the DismissPopup method.
        /// </summary>
        public abstract void OpenDialogWithMessage(string text, string yeaString, System.Action yeaCallback, string neaString, System.Action neaCallback, System.Action dismissCallback);

        public virtual void OpenDialogWithMessage(string text, string yeaString, System.Action yeaCallback, System.Action dismissCallback)
        {
            OpenDialogWithMessage(text, yeaString, yeaCallback, "Cancel", () => {}, dismissCallback);
        }

        public virtual void OpenDialogWithMessage(string text, System.Action dismissCallback)
        {
            OpenDialogWithMessage(text, "", null, "Cancel", () => {}, dismissCallback);
        }

        /// <summary>
        /// Dismisses the active dialog
        /// </summary>
        public abstract void DismissPopup();
    }
}

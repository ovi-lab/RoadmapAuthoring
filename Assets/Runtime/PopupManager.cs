// Copyright (c) UBCO OVILab
// This code is licensed under MIT license (see LICENSE.md for details)

using UnityEngine;

namespace ubc.ok.ovilab.roadmap
{
    /// <summary>
    /// Abstract class to manage popups.
    /// </summary>
    public abstract class PopupManager: MonoBehaviour
    {
        /// <summary>
        /// Show a dialog. The active shown dialog should get
        /// dismissed when the <see cref="DismissPopup"/> is called.
        /// <param name="text">The message to be displayed.</param>
        /// <param name="yeaString">The string on the positive
        /// button.</param>
        /// <param name="yeaCallback">The callback when the positive
        /// button is selected. If this is null, the positive button
        /// will not be shown.</param>
        /// <param name="neaString">The string on the negative
        /// button.</param>
        /// <param name="neaCallback">The callback when the negative
        /// button is selected. If this is null, the positive button
        /// will not be shown.</param>
        /// <param name="dismissCallback">The callback when the popup
        /// is dismissed. This will be called when any of the buuttons
        /// are pressed, or the popup is dismissed in a different
        /// way.</param>
        /// </summary>
        public abstract void OpenDialogWithMessage(string text, string yeaString, System.Action yeaCallback, string neaString, System.Action neaCallback, System.Action dismissCallback);

        /// <summary>
        /// Show a dialog with only positive button. The active shown
        /// dialog should get dismissed when the <see cref="DismissPopup"/>
        /// is called.   <see cref="OpenDialogWithMessage"/> for
        /// information on parameters.
        /// </summary>
        public virtual void OpenDialogWithMessage(string text, string yeaString, System.Action yeaCallback, System.Action dismissCallback)
        {
            OpenDialogWithMessage(text, yeaString, yeaCallback, "Cancel", () => {}, dismissCallback);
        }

        /// <summary>
        /// Show a dialog with only negative button with "cancel",
        /// which only dismisses the dialog. The active shown dialog
        /// should get dismissed when the <see cref="DismissPopup"/>
        /// is called.  <see cref="OpenDialogWithMessage"/> for
        /// information on parameters.
        /// </summary>
        public virtual void OpenDialogWithMessage(string text, System.Action dismissCallback)
        {
            OpenDialogWithMessage(text, "", null, "Cancel", () => {}, dismissCallback);
        }

        /// <summary>
        /// Dismisses the active dialog.
        /// </summary>
        public abstract void DismissPopup();
    }
}

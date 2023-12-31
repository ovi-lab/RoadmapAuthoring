// Copyright (c) UBCO OVILab
// This code is licensed under MIT license (see LICENSE.md for details)

using MixedReality.Toolkit.UX;
using UnityEngine;

namespace ubc.ok.ovilab.roadmap
{
    /// <summary>
    /// <see cref="PopupManager"/> that manages an <see cref="MixedReality.Toolkit.UX.Dialog"/>.
    /// <seealso cref="CanvasPopupManager"/>
    /// </summary>
    [RequireComponent(typeof(DialogPool))]
    public class MRTKPopupManager :PopupManager
    {
        private DialogPool dialogPool;
        private IDialog activeDialog;

        private void Start()
        {
            dialogPool = GetComponent<DialogPool>();
        }

        public override void OpenDialogWithMessage(string text, string yeaString, System.Action yeaCallback, string neaString, System.Action neaCallback, System.Action dismissCallback)
        {
            IDialog dialog = dialogPool.Get();
            dialog.Reset();
            dialog = dialog.SetHeader(text);

            if(yeaCallback != null)
            {
                dialog = dialog.SetPositive(yeaString, (_) => yeaCallback());
            }

            if(neaCallback != null)
            {
                dialog = dialog.SetNegative(neaString, (_) => neaCallback());
            }

            if(dismissCallback != null)
            {
                dialog.OnDismissed += (_) => dismissCallback();
            }

            activeDialog = dialog;

            dialog.Show();
        }

        public override void DismissPopup()
        {
            if (activeDialog != null)
            {
                activeDialog.Dismiss();
            }
            activeDialog = null;
        }
    }
}

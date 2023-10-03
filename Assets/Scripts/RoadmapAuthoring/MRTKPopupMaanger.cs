using MixedReality.Toolkit.UX;
using UnityEngine;

namespace ubc.ok.ovilab.roadmap
{
    [RequireComponent(typeof(DialogPool))]
    public class MRTKPopupMaanger :PopupManager
    {
        private DialogPool dialogPool;
        private IDialog activeDialog;

        private void Start()
        {
            dialogPool = GetComponent<DialogPool>();
        }

        public override void OpenDialogWithMessage(string text, string yeaString, System.Action yeaCallback, string neaString, System.Action neaCallback, System.Action dismissCallback)
        {
            IDialog dialog = dialogPool.Get().SetHeader(text);

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
            activeDialog.Dismiss();
        }
    }
}

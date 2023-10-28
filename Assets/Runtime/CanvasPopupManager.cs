// Copyright (c) UBCO OVILab
// This code is licensed under MIT license (see LICENSE.md for details)

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ubc.ok.ovilab.roadmap
{
    /// <summary>
    /// <see cref="PopupManager"/> that works with canvas elements.
    /// <seealso cref="MRTKPopupManager"/>
    /// </summary>
    public class CanvasPopupManager :PopupManager
    {
        [Tooltip("The root GameObject in the scene of the dialog being managed.")]
        [SerializeField] private GameObject popupDialogBase;
        [Tooltip("TMPro text object where the message will be displayed.")]
        [SerializeField] private TextMeshProUGUI message;
        [Tooltip("The TMPro text positive button.")]
        [SerializeField] private Button yeaButton;
        [Tooltip("The TMPro text negtive button.")]
        [SerializeField] private Button neaButton;

        private TextMeshProUGUI yeaText;
        private TextMeshProUGUI neaText;
        private bool previousDialogActiveState = false;

        private System.Action activeDismissCallback;

        #region Unity functions
        private void Start()
        {
            yeaText = yeaButton.GetComponentInChildren<TextMeshProUGUI>();
            neaText = neaButton.GetComponentInChildren<TextMeshProUGUI>();
            previousDialogActiveState = popupDialogBase.activeInHierarchy;
        }

        // Keep track of when the popup is dismissed
        private void Update()
        {
            bool dialogActiveState = popupDialogBase.activeInHierarchy;

            // If the state changed and new state is false
            if (dialogActiveState != previousDialogActiveState && !dialogActiveState)
            {
                activeDismissCallback?.Invoke();
                DismissPopup();
            }
        }
        #endregion

        #region Overrides
        /// <inheritdoc />
        public override void OpenDialogWithMessage(string text, string yeaString, System.Action yeaCallback, string neaString, System.Action neaCallback, System.Action dismissCallback)
        {
            message.text = text;

            if(yeaCallback != null)
            {
                yeaButton.gameObject.SetActive(true);
                UnityAction yeaCallbackAction = new UnityAction(yeaCallback);
                yeaButton.onClick.AddListener(yeaCallbackAction);
                yeaButton.onClick.AddListener(DismissPopup);
                yeaText.text = yeaString;
            }
            else
            {
                yeaButton.gameObject.SetActive(false);
            }


            if(neaCallback != null)
            {
                neaButton.gameObject.SetActive(true);
                UnityAction neaCallbackAction = new UnityAction(neaCallback);
                neaButton.onClick.AddListener(neaCallbackAction);
                neaButton.onClick.AddListener(DismissPopup);
                neaText.text = neaString;
            }
            else
            {
                neaButton.gameObject.SetActive(false);
            }

            if(dismissCallback != null)
            {
                UnityAction dismissCallbackAction = new UnityAction(dismissCallback);
                neaButton.onClick.AddListener(dismissCallbackAction);
                activeDismissCallback = dismissCallback;
            }

            popupDialogBase.SetActive(true);
        }

        /// <inheritdoc />
        public override void DismissPopup()
        {
            popupDialogBase.SetActive(false);
            yeaButton.onClick.RemoveAllListeners();
            neaButton.onClick.RemoveAllListeners();
            activeDismissCallback = null;
        }
        #endregion
    }
}

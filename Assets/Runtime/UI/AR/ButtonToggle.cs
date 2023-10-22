using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ubc.ok.ovilab.roadmap
{
    /// <summary>
    /// Toggle behaviour to canvas buttons
    /// </summary>
    public class ButtonToggle : MonoBehaviour
    {
        public UnityEvent onToggled;
        public UnityEvent onUntoggled;

        private bool toggleOn = false;

        // Unity method
        private void Start()
        {
            // initilize toggle states
            if (toggleOn)
            {
                onToggled?.Invoke();
            }
            else
            {
                onUntoggled?.Invoke();
            }

            // Add a click event listener to the Modify button
            GetComponent<Button>().onClick.AddListener(ToggleButtonsVisibility);
        }

        /// <summary>
        /// Triggers the appropriate event based on the toggle status
        /// </summary>
        private void ToggleButtonsVisibility()
        {
            // Toggle the visibility of the other buttons
            toggleOn = !toggleOn;

            if (toggleOn)
            {
                onToggled?.Invoke();
            }
            else
            {
                onUntoggled?.Invoke();
            }
        }
    }
}

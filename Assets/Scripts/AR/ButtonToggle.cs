using UnityEngine;
using UnityEngine.UI;

public class ButtonToggle : MonoBehaviour
{
    public Button modifyButton;
    public Button addButton;
    public Button deleteButton;
    public Button syncButton;

    private bool isButtonsVisible = false;

    private void Start()
    {
        // Initialize button visibility
        ToggleButtonsVisibility();

        // Add a click event listener to the Modify button
        modifyButton.onClick.AddListener(ToggleButtonsVisibility);
    }

    private void ToggleButtonsVisibility()
    {
        // Toggle the visibility of the other buttons
        isButtonsVisible = !isButtonsVisible;

        addButton.gameObject.SetActive(isButtonsVisible);
        deleteButton.gameObject.SetActive(isButtonsVisible);
        syncButton.gameObject.SetActive(isButtonsVisible);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class DropdownController : MonoBehaviour
{
    public GameObject[] objectDropdown;
    public GameObject[] objectsToToggle;

    private bool objectsVisible = false;

    public void ToggleObjectsVisibility()
    {
        objectsVisible = !objectsVisible;

        foreach (GameObject obj in objectsToToggle)
        {
            obj.SetActive(objectsVisible);
        }
    }
}

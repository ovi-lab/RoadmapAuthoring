using UnityEngine;
using UnityEngine.UI;

public class ToggleOBJ : MonoBehaviour
{
    public GameObject[] objectsToShowHide;

    private void Start()
    {
        // Set all objects to be initially hidden
        foreach (var obj in objectsToShowHide)
        {
            obj.SetActive(false);
        }
    }

    public void ToggleObjectVisibility(int index)
    {
        if (index >= 0 && index < objectsToShowHide.Length)
        {
            bool isOn = objectsToShowHide[index].activeSelf;
            objectsToShowHide[index].SetActive(!isOn);
        }
    }
}

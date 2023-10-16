using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class activate : MonoBehaviour
{
    private bool isVisible = true;
    
   
    public void ToggleObjectVisibility()
    {
        isVisible = !isVisible;
        gameObject.SetActive(isVisible);
    }
}

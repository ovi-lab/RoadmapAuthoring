using UnityEngine;

public class MoveObject : MonoBehaviour
{
    private bool isObjectSelected = false;
    private Vector3 touchOffset;

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    // Check if the touch hits this object
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
                    {
                        // Object is selected, store the touch offset
                        isObjectSelected = true;
                        touchOffset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 10f));
                    }
                    break;

                case TouchPhase.Moved:
                    if (isObjectSelected)
                    {
                        // Move the object with the touch
                        Vector3 newPosition = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 10f)) + touchOffset;
                        transform.position = newPosition;
                    }
                    break;

                case TouchPhase.Ended:
                    isObjectSelected = false;
                    break;
            }
        }
    }
}

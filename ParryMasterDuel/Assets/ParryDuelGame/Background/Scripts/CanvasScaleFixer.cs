using UnityEngine;

public class CanvasScaleFixer : MonoBehaviour
{
    private Vector3 initialLocalScale;

    void Start()
    {
        // Store the scale you set in the inspector
        initialLocalScale = transform.localScale;
    }

    void LateUpdate()
    {
        if (transform.parent == null) return;

        // Get the current local scale to modify it
        Vector3 newScale = initialLocalScale;

        // If the parent's scale is negative (flipped), 
        // we flip the child's local scale to counteract it.
        if (transform.parent.localScale.x < 0)
        {
            newScale.x = -initialLocalScale.x;
        }
        else
        {
            newScale.x = initialLocalScale.x;
        }

        transform.localScale = newScale;
    }
}
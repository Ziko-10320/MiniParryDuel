using UnityEngine;

public class CanvasScaleFixer : MonoBehaviour
{
    private Vector3 fixedScale;

    void Start()
    {
        fixedScale = new Vector3(Mathf.Abs(transform.localScale.x),
                                  transform.localScale.y,
                                  transform.localScale.z);
    }

    void LateUpdate()
    {
        // Always keep X scale positive regardless of parent flipping
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );
    }
}
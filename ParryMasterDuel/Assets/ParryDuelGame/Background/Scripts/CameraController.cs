using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Targets")]
    public Transform player1;
    public Transform player2;

    [Header("Zoom")]
    [Tooltip("Orthographic size when players are very close")]
    public float minZoom = 4f;
    [Tooltip("Maximum orthographic size (how wide the camera can get)")]
    public float maxZoom = 9f;
    [Tooltip("How much of the distance between players maps to zoom")]
    public float zoomLimiter = 10f;
    [Tooltip("Extra padding added to the zoom so players aren't at screen edges")]
    public float zoomPadding = 2f;

    [Header("Y Follow")]
    [Tooltip("How high above the midpoint the camera offset sits")]
    public float yOffset = 1f;
    [Tooltip("How far up the camera will follow (above the baseline Y)")]
    public float maxYOffset = 2f;
    [Tooltip("Baseline Y the camera always tries to return toward")]
    public float baselineY = 0f;

    [Header("Smoothing")]
    public float positionSmoothTime = 0.15f;
    public float zoomSmoothTime = 0.2f;

    // ?? Private ??????????????????????????????????????????????
    private Camera cam;
    private Vector3 velocity = Vector3.zero;
    private float zoomVelocity = 0f;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (player1 == null || player2 == null) return;

        MoveCamera();
        ZoomCamera();
    }

    void MoveCamera()
    {
        // Midpoint between both players
        Vector3 midpoint = (player1.position + player2.position) / 2f;

        // Y: clamp between baseline and baseline + maxYOffset
        // This means the camera gently follows jumps but never drops below baselineY
        float targetY = Mathf.Clamp(midpoint.y + yOffset, baselineY, baselineY + maxYOffset);

        Vector3 targetPosition = new Vector3(midpoint.x, targetY, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, positionSmoothTime);
    }

    void ZoomCamera()
    {
        // Distance between players drives zoom
        float distance = Vector3.Distance(player1.position, player2.position);

        // Map distance ? orthographic size
        float targetZoom = (distance / zoomLimiter) * (maxZoom - minZoom) + minZoom + zoomPadding;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetZoom, ref zoomVelocity, zoomSmoothTime);
    }

    // Assign players from outside (called by game manager after characters are spawned)
    public void SetTargets(Transform p1, Transform p2)
    {
        player1 = p1;
        player2 = p2;
    }
}
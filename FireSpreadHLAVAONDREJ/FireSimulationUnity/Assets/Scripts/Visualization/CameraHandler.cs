using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [SerializeField] Camera targetCamera; // Allow setting a custom camera

    private Vector3 worldCenter = new Vector3(0f, 0f, 0f);

    // Properties for zoom
    public float MinZoom = 5f;
    public float MaxZoom = 50f;
    public float ZoomChangeSpeed = 0.05f;

    // Properties for rotation
    public float UpDownSpeed = 50f;
    public float RotationSpeed = 50f;
    public Vector3 DefaultRotation = new Vector3(0, 0, 0);

    void Awake()
    {
        // If no camera is set, use the main camera
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    // Camera rotates around this point.
    public void SetWorldCenter(Vector3 center)
    {
        worldCenter = center;
    }

    // Handles changes in camera zoom level.
    public void ZoomCamera(float zoomChange)
    {
        targetCamera.orthographicSize = Mathf.Clamp(targetCamera.orthographicSize + zoomChange * ZoomChangeSpeed, MinZoom, MaxZoom);
    }

    // Adjusts the camera's rotation based on user input. Behaves as set to default position if zero arguments are provided.
    public void RotateCamera(Vector3? rotationChange = null)
    {

        // Use the provided rotationChange or defaultRotation if null which is to set 
        Vector3 effectiveRotationChange = rotationChange ?? DefaultRotation;

        // Calculate new rotation angles
        Vector3 angles = Camera.main.transform.eulerAngles + new Vector3(-1 * effectiveRotationChange.x * UpDownSpeed * Time.deltaTime, -1 * effectiveRotationChange.y * RotationSpeed * Time.deltaTime, 0);
        angles.x = Mathf.Clamp(angles.x, 10f, 89f); // Min/max angle range

        targetCamera.transform.eulerAngles = angles;
        float CameraDistance = 100f; // Being to close to with camera can make tiles to no render some parts
        targetCamera.transform.position = worldCenter - (targetCamera.transform.forward * CameraDistance); // Move camera to new position
        targetCamera.transform.LookAt(worldCenter);
    }
}
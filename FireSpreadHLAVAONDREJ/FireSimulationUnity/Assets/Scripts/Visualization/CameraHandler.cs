using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraHandler : MonoBehaviour
{
    [SerializeField] Camera targetCamera; // Allow setting a custom camera

    private Vector3 worldCenter = new Vector3(0f, 0f, 0f);
    public Vector3 savedPosition;
    public Quaternion savedRotation;
    private bool isPositionSaved = false;

    // Properties for zoom
    float CameraDistance = 50f; // Being to close to with camera can make tiles to no render some parts
    public float MinZoom = 5f;
    public float MaxZoom = 50f;
    public float ZoomChangeSpeed = 0.15f;

    // Properties for rotation
    public float UpDownSpeed = 50f;
    public float RotationSpeed = 50f;

    void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main; // If no camera is set, use the main camera as default
        }
    }

    // Save the current camera position and rotation
    public void SaveCameraPosition()
    {
        try
        {
            savedPosition = targetCamera.transform.position;
            savedRotation = targetCamera.transform.rotation;
            isPositionSaved = true;
            Debug.Log("Camera position and rotation saved.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save camera position and rotation: " + e.Message);
        }
    }

    // Apply the saved camera position and rotation
    public void ApplySavedCameraPosition()
    {
        if (!isPositionSaved)
        {
            Debug.LogWarning("Camera position and rotation have not been saved yet!");
            return;
        }

        try
        {
            targetCamera.transform.position = savedPosition;
            targetCamera.transform.rotation = savedRotation;
            Debug.Log("Camera position and rotation applied.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to apply saved camera position and rotation: " + e.Message);
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

    // Sets initial zoom and position of the camera for new world based on the larger dimension size of the world
    public void SetCamera(int width, int depth)
    {
        RotateCamera(new Vector3(0, 0, 0)); // set to default position

        float requiredSize = Mathf.Max(width / 2f, depth / 2f / targetCamera.aspect);
        targetCamera.orthographicSize = Mathf.Clamp(requiredSize, MinZoom, MaxZoom);
    }

    // Adjusts the camera's rotation based on user input.
    public void RotateCamera(Vector3 rotationChange)
    {
        Vector3 effectiveRotationChange = rotationChange;

        // Calculate new rotation angles
        Vector3 angles = Camera.main.transform.eulerAngles + new Vector3(-1 * effectiveRotationChange.x * UpDownSpeed * Time.deltaTime, -1 * effectiveRotationChange.y * RotationSpeed * Time.deltaTime, 0);
        angles.x = Mathf.Clamp(angles.x, 10f, 89f); // Min/max angle range of the camera

        targetCamera.transform.eulerAngles = angles;
        targetCamera.transform.position = worldCenter - (targetCamera.transform.forward * CameraDistance); // Move camera to new position
        targetCamera.transform.LookAt(worldCenter);
    }

    // Captures a high-resolution screenshot
    public void CaptureHighResolutionScreenshot()
    {
        int ResolutionMultiplier = 2;

        int width = Screen.width * ResolutionMultiplier;
        int height = Screen.height * ResolutionMultiplier;

        RenderTexture rt = new RenderTexture(width, height, 24);
        targetCamera.targetTexture = rt; // Set the target texture of the camera
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);

        targetCamera.Render();

        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        targetCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        Destroy(screenShot);

        string saveDirectory = Path.Combine(Application.streamingAssetsPath, "ScreenShots");
        string filename = Path.Combine(saveDirectory, "Screenshot_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png");
        File.WriteAllBytes(filename, bytes);

        Debug.Log("Screenshot saved to: " + filename);
    }
}
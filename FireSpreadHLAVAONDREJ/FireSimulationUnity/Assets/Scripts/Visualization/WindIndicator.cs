using UnityEngine;
using TMPro;

public class WindIndicator : MonoBehaviour
{
    [SerializeField] Camera mainTargetCamera;
    [SerializeField] TextMeshProUGUI windSpeedText;
    [SerializeField] Camera windArrowCamera;
    [SerializeField] GameObject windArrow; // for wind indicator

    void Awake()
    {
        // If no camera is set, use the main camera
        if (mainTargetCamera == null)
        {
            mainTargetCamera = Camera.main;
        }
    }

    // Update the wind indicator to show the current wind direction and speed
    public void UpdateIndicator(int windDirection, float windSpeed)
    {
        // Update the arrow's rotation to match the wind direction for Unity
        windArrow.transform.eulerAngles = new Vector3(0, -windDirection + 90, 90);

        windSpeedText.text = $"Wind: \n {windSpeed.ToString("F1")} km/h";
    }

    // Ensures the wind arrow is correctly oriented (looked through the windArrowCamera with the mainTargetCamera) to appear for the main camera as it is pointing in the right direction.
    public void UpdateIndicatorCameraAngle()
    {
        // Calculate the direction vector from the main camera to its focal point (assuming it's the world's center)
        Vector3 mainCameraDirection = mainTargetCamera.transform.forward;

        // Determine the distance from the arrow to the windArrowCamera
        float distanceToArrow = (windArrowCamera.transform.position - windArrow.transform.position).magnitude;

        // Update the secondary camera's position to be at the same distance from the arrow but in the opposite direction of the main camera
        Vector3 windArrowCameraPosition = windArrow.transform.position - mainCameraDirection * distanceToArrow;

        windArrowCamera.transform.position = windArrowCameraPosition;

        if (mainCameraDirection == Vector3.down) // main camera looking straight down
        {
            windArrowCamera.transform.rotation = mainTargetCamera.transform.rotation;
        }
        else
        {
            windArrowCamera.transform.LookAt(windArrow.transform.position);
        }

        return;
    }

    // Method to show/activate the wind indicator objects
    public void ActivateIndicator()
    {
        windSpeedText.gameObject.SetActive(true);
        windArrow.SetActive(true);
    }

    // Method to hide/deactivate the wind indicator objects
    public void DeactivateIndicator()
    {
        windSpeedText.gameObject.SetActive(false);
        windArrow.SetActive(false);
    }

    // Set the indicator to a default apperance
    public void SetIndicatorToDefault()
    {
        windArrow.transform.eulerAngles = new Vector3(0, 90, 90);
        windSpeedText.text = "Wind Speed";
    }
}
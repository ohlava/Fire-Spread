using UnityEngine;
using TMPro;

public class WindIndicator : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI windSpeedText;
    [SerializeField] Camera windArrowCamera;
    [SerializeField] GameObject windArrow; // for wind indicator

    // Update the wind indicator to show the current wind direction and speed
    public void UpdateIndicator(int windDirection, float windSpeed)
    {
        // Update the arrow's rotation to match the wind direction
        windArrow.transform.eulerAngles = new Vector3(0, -windDirection + 90, 90);

        // Update the wind speed text
        windSpeedText.text = $"Wind: \n {windSpeed.ToString("F1")} km/h";
    }

    public void UpdateCamera()
    {
        // Calculate the direction vector from the main camera to its focal point (assuming it's the world's center)
        Vector3 mainCameraDirection = Camera.main.transform.forward;

        // Determine the distance from the arrow to the windArrowCamera
        float distanceToArrow = (windArrowCamera.transform.position - windArrow.transform.position).magnitude;

        // Update the secondary camera's position to be at the same distance from the arrow but in the opposite direction of the main camera
        Vector3 windArrowCameraPosition = windArrow.transform.position - mainCameraDirection * distanceToArrow;

        windArrowCamera.transform.position = windArrowCameraPosition;

        if (mainCameraDirection == Vector3.down) // main camera is looking straight down
        {
            // Copy the rotation of the main camera to the windArrowCamera
            windArrowCamera.transform.rotation = Camera.main.transform.rotation;
        }
        else
        {
            // Make the windArrowCamera LookAt the arrow center
            windArrowCamera.transform.LookAt(windArrow.transform.position);
        }

        return;
    }

    // Method to activate the wind indicator objects
    public void ActivateIndicator()
    {
        windSpeedText.gameObject.SetActive(true);
        windArrow.SetActive(true);
    }

    // Method to deactivate the wind indicator objects
    public void DeactivateIndicator()
    {
        windSpeedText.gameObject.SetActive(false);
        windArrow.SetActive(false);
    }
}
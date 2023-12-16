using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEditor;

public class WindIndicator : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI windSpeedText;
    [SerializeField] Camera windArrowCamera;
    [SerializeField] GameObject windArrow; // for wind indicator


    // Update the wind indicator to show the current wind direction and speed
    public void UpdateIndicator(Weather weather)
    {
        // Update the arrow's rotation to match the wind direction
        // Rotate the arrow so that it points in the direction from which the wind is coming
        // Unity uses a left-handed coordinate system, so rotating around the Y axis in a positive direction
        // will make the arrow point counter-clockwise. This means we need to negate the wind direction
        // to make the arrow point along the wind direction.
        windArrow.transform.eulerAngles = new Vector3(0, -weather.WindDirection + 90, 90);

        // Update the wind speed text
        // The "F1" format specifier tells the ToString method to format the float to one decimal place
        windSpeedText.text = $"Wind: \n {weather.WindSpeed.ToString("F1")} km/h";
    }

    public void UpdateCamera()
    {
        // Calculate the direction vector from the main camera to its focal point (assuming it's the world's center)
        Vector3 mainCameraDirection = Camera.main.transform.forward;

        // Determine the distance from the arrow to the windArrowCamera
        float distanceToArrow = (windArrowCamera.transform.position - windArrow.transform.position).magnitude;

        // Update the secondary camera's position to be at the same distance from the arrow but in the opposite direction of the main camera
        // Here we invert the direction by using -mainCameraDirection
        Vector3 windArrowCameraPosition = windArrow.transform.position - mainCameraDirection * distanceToArrow;

        windArrowCamera.transform.position = windArrowCameraPosition;

        // Check if the main camera is looking straight down
        if (mainCameraDirection == Vector3.down)
        {
            // If yes, copy the rotation of the main camera to the windArrowCamera
            windArrowCamera.transform.rotation = Camera.main.transform.rotation;
        }
        else
        {
            // Otherwise, make the windArrowCamera LookAt the arrow center
            windArrowCamera.transform.LookAt(windArrow.transform.position);
        }

        return;
    }
}
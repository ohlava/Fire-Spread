using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class WindIndicator : MonoBehaviour
{
    public Transform arrowTransform;
    public TextMeshProUGUI windSpeedText;

    // Update the wind indicator to show the current wind direction and speed
    public void UpdateIndicator(Weather weather)
    {
        // Update the arrow's rotation to match the wind direction
        if (arrowTransform != null)
        {
            // Rotate the arrow so that it points in the direction from which the wind is coming
            // Unity uses a left-handed coordinate system, so rotating around the Y axis in a positive direction
            // will make the arrow point counter-clockwise. This means we need to negate the wind direction
            // to make the arrow point along the wind direction.
            arrowTransform.eulerAngles = new Vector3(0, -weather.WindDirection + 90, 90);
        }
        else
        {
            Debug.LogError("WindIndicator: Arrow Transform is not assigned!");
        }

        // Update the wind speed text
        if (windSpeedText != null)
        {
            // The "F1" format specifier tells the ToString method to format the float to one decimal place
            windSpeedText.text = $"Wind: \n {weather.WindSpeed.ToString("F1")} km/h"; 
        }
        else
        {
            Debug.LogError("WindIndicator: Wind Speed Text is not assigned!");
        }

    }
}
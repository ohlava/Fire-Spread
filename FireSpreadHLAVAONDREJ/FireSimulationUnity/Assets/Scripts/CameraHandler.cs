using System;
using UnityEngine;

public static class CameraHandler
{
    public static void SetCameraPositionAndOrientation(int worldWidth, int worldDepth)
    {
        // The diagonal of the world in Unity units
        float diagonal = Mathf.Sqrt(worldWidth * worldWidth + worldDepth * worldDepth);
        float zoom = 0.7f;

        // Set the camera's position to the center of the world, and above it at the calculated distance.
        Camera.main.transform.position = new Vector3(worldWidth / 2f, diagonal * zoom, 0);

        // Adjust the camera's orientation to look towards the center of the world.
        Camera.main.transform.LookAt(new Vector3(worldWidth / 2f, 0, worldDepth / 2f));
    }
}
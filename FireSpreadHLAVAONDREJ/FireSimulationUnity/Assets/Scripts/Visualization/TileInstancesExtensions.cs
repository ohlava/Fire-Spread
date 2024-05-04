using UnityEngine;

// Extension methods for GameObject instances - mainly used for tiles instances in the visualizer.
public static class TileInstancesExtensions
{
    // Sets the material of the GameObject instance.
    public static void SetMaterialTo(this GameObject instance, Material material)
    {
        Renderer renderer = instance.transform.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }
    }

    // Sets the color of the GameObject instance's material.
    public static void SetColorTo(this GameObject instance, Color color)
    {
        Renderer renderer = instance.transform.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }

    // Gets the position of the GameObject instance in world space.
    public static Vector3 GetPosition(this GameObject instance)
    {
        return instance.transform.position;
    }

    // Gets the height of the GameObject instance.
    public static float GetHeight(this GameObject instance)
    {
        return instance.transform.localScale.y;
    }
}
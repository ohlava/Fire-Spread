using System;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System.IO;
using UnityEngine.UI;

public class MapImporter : MonoBehaviour
{
    private float[,] heightMap;
    public int maxMapHeight = 100;
    public int maxMapWidth = 100;

    // Adjust this to change how the height values will be scaled
    public float HeightMultiplier = 2f;
    // widthDepthScale parameter is the number of pixels to skip between each sample of the texture map
    public int widthDepthScale = 1;

    public float[,] GetMap()
    {
        if (ImportHeightMap() == true)
        {
            return heightMap;
        }
        else
        {
            Debug.Log("Something went wrong with map Import");

            // return plain map
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    heightMap[i, j] = 1f;
                }
            }
            return heightMap;
        }
    }

    private bool ImportHeightMap()
    {
        string path = EditorUtility.OpenFilePanel("Overwrite with png", "", "png");
        if (path.Length != 0)
        {
            var fileContent = File.ReadAllBytes(path);

            // Create a texture and load the image file into it
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileContent);

            // Convert the image into a heightmap
            heightMap = ConvertToHeightmap(tex);
            return true;
        }
        return false;
    }

    private float[,] ConvertToHeightmap(Texture2D tex)
    {
        
        if(widthDepthScale < 1)
            widthDepthScale = 1;

        int width = Mathf.Min(tex.width / widthDepthScale, maxMapWidth);
        int height = Mathf.Min(tex.height / widthDepthScale, maxMapHeight);

        float[,] heights = new float[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Get the color value (0.0 - 1.0) for the pixel
                // We use the grayscale value of the image as the height
                // Make sure to multiply indices by the scale to get the correct pixel from the texture
                float heightValue = tex.GetPixel(i * widthDepthScale, j * widthDepthScale).grayscale;

                // Scale the height value and assign it to the heightmap
                heights[i, j] = heightValue * HeightMultiplier;
            }
        }

        return heights;
    }
}
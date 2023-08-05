using System;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System.IO;
using UnityEngine.UI;

public interface IMapImporter
{
    float[,] GetMap(int width, int depth);
}

public class HeightMapImporter : IMapImporter
{
    private float[,] heightMap;

    // Adjust this to change how the height values will be scaled
    public float HeightMultiplier = 20f;

    public float[,] GetMap(int requiredWidth, int requiredDepth)
    {
        if (ImportHeightMap(requiredWidth, requiredDepth) == true)
        {
            return heightMap;
        }
        else
        {
            Debug.Log("Something went wrong with map Import");

            // return plain map
            float[,] plainMap = new float[requiredWidth, requiredDepth];
            for (int i = 0; i < requiredWidth; i++)
            {
                for (int j = 0; j < requiredDepth; j++)
                {
                    plainMap[i, j] = 1f;
                }
            }
            return plainMap;
        }
    }

    private bool ImportHeightMap(int requiredWidth, int requiredDepth)
    {
        string path = EditorUtility.OpenFilePanel("Overwrite with png", "", "png"); // CONNOT BE used outside of unity editor
        if (path.Length != 0)
        {
            var fileContent = File.ReadAllBytes(path);

            // Create a texture and load the image file into it
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileContent);

            if (tex.width < requiredWidth || tex.height < requiredDepth)
            {
                return false;
            }

            // Convert the image into a heightmap
            heightMap = ConvertToHeightmap(tex, requiredWidth, requiredDepth);
            return true;
        }
        return false;
    }

    private float[,] ConvertToHeightmap(Texture2D tex, int requiredWidth, int requiredDepth)
    {
        float[,] heights = new float[requiredWidth, requiredDepth];

        int widthSampleRatio = (int)((float)tex.width / requiredWidth);
        int depthSampleRatio = (int)((float)tex.height / requiredDepth);

        int sampleRatio = Mathf.Min(widthSampleRatio, depthSampleRatio, tex.width - 1, tex.height - 1);

        for (int i = 0; i < requiredWidth; i++)
        {
            for (int j = 0; j < requiredDepth; j++)
            {
                float heightValue = 1f;
                if (i < tex.width && j < tex.height)
                {
                    heightValue = tex.GetPixel(i * sampleRatio, j * sampleRatio).grayscale;
                }
                else
                {
                    Debug.LogError("Outside of the range");
                }

                // Scale the height value and assign it to the heightmap
                heights[i, j] = heightValue * HeightMultiplier;
            }
        }
        return heights;
    }
}
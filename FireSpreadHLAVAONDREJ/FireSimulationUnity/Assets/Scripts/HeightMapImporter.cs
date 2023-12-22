using System;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System.IO;
using UnityEngine.UI;


public interface IMapImporter
{
    Map<float> GetMap(int width, int depth, string fullFilePath);
}

public class HeightMapImporter : IMapImporter
{
    private Map<float> heightMap;

    // Adjust this to change how the height values will be scaled
    public float HeightMultiplier = 10f;

    public Map<float> GetMap(int requiredWidth, int requiredDepth, string fullFilePath)
    {
        if (ImportHeightMap(requiredWidth, requiredDepth, fullFilePath) == true)
        {
            return heightMap;
        }
        Debug.Log("Something went wrong with map Import, check the file path:");
        return null;
    }

    private bool ImportHeightMap(int requiredWidth, int requiredDepth, string fullFilePath)
    {
        if (File.Exists(fullFilePath))
        {
            var fileContent = File.ReadAllBytes(fullFilePath);

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


    private Map<float> ConvertToHeightmap(Texture2D tex, int requiredWidth, int requiredDepth)
    {
        Map<float> heights = new Map<float>(requiredWidth, requiredDepth);

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
                heights.Data[i, j] = heightValue * HeightMultiplier;
            }
        }
        return heights;
    }
}
using UnityEngine;
using System.IO;

public interface IMapImporter<T>
{
    Map<T> GetMap(int width, int depth, string fullFilePath);
}

public class HeightMapImporter : IMapImporter<float>
{
    private Map<float> heightMap;
    public float HeightMultiplier = 5f; // Adjust this to change how the height values will be scaled

    // Retrieves a height map from a file or returns a default map if the file import fails.
    public Map<float> GetMap(int requiredWidth, int requiredDepth, string fullFilePath)
    {
        if (ImportHeightMap(requiredWidth, requiredDepth, fullFilePath))
        {
            // Do other stuff like smoothing etc. if needed
            return heightMap;
        }
        else
        {
            Debug.Log($"Something went wrong with map Import, using default map instead, check the file path: {fullFilePath}");
            Map<float> defaultMap = new Map<float>(requiredWidth, requiredDepth);
            defaultMap.FillWithDefault(1.0f);
            return defaultMap;
        }
    }

    private bool ImportHeightMap(int requiredWidth, int requiredDepth, string fullFilePath)
    {
        if (File.Exists(fullFilePath))
        {
            var fileContent = File.ReadAllBytes(fullFilePath);

            // Create a texture and load the image file into it
            Texture2D tex = new Texture2D(2, 2); // Temporary allocation before loading the actual image data
            tex.LoadImage(fileContent);

            if (tex.width < requiredWidth || tex.height < requiredDepth)
            {
                Debug.Log("Required world dimensions exceed for this image.");
                return false;
            }

            // Convert the image into a heightmap
            heightMap = ConvertToHeightmap(tex, requiredWidth, requiredDepth);
            return true;
        }
        return false;
    }

    // Converts a texture into a height map using a calculated sample ratio to fit the required dimensions.
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
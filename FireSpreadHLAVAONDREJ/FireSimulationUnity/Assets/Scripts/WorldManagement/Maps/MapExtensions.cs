using System;
using System.Collections.Generic;

public static class MapExtensions
{
    // Normalizes all heights to be between 0 and 1
    public static Map<float> Normalize(this Map<float> map)
    {
        // Find min and max values
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Depth; y++)
            {
                if (map.Data[x, y] < minValue)
                {
                    minValue = map.Data[x, y];
                }

                if (map.Data[x, y] > maxValue)
                {
                    maxValue = map.Data[x, y];
                }
            }
        }

        // Scale the values between 0 and 1
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Depth; y++)
            {
                map.Data[x, y] = (map.Data[x, y] - minValue) / (maxValue - minValue);
            }
        }

        return map;
    }

    // Reduce the height of the neighbors of waterMap by the beach factor
    public static Map<float> ReduceByBeachFactor(this Map<float> map, Map<int> waterMap, float beachFactor)
    {
        for (int x = 3; x < map.Width - 3; x++)
        {
            for (int y = 3; y < map.Depth - 3; y++)
            {
                if (waterMap.Data[x, y] == 1)
                {
                    // Experiment with -2/2 -3/3, random etc. 
                    for (int dx = -3; dx <= 3; dx++)
                    {
                        for (int dy = -3; dy <= 3; dy++)
                        {
                            map.Data[x + dx, y + dy] *= (1 - beachFactor);
                        }
                    }
                }
            }
        }

        return map.Normalize();
    }

    // Blur or Smooth Map: This could be used to make transitions between heights or other features more gradual.
    public static Map<float> Smooth(this Map<float> map, int iterations = 1)
    {
        for (int iter = 0; iter < iterations; iter++)
        {
            for (int x = 1; x < map.Width - 1; x++)
            {
                for (int y = 1; y < map.Depth - 1; y++)
                {
                    float avg = (
                        map.Data[x - 1, y - 1] + map.Data[x, y - 1] + map.Data[x + 1, y - 1] +
                        map.Data[x - 1, y] + map.Data[x, y] + map.Data[x + 1, y] +
                        map.Data[x - 1, y + 1] + map.Data[x, y + 1] + map.Data[x + 1, y + 1]
                    ) / 9f;

                    map.Data[x, y] = avg;
                }
            }
        }
        return map;
    }

    // Amplify(or Reduce) Map: Modify the values in the map by multiplying them by a factor.
    public static Map<float> Amplify(this Map<float> map, float factor)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Depth; y++)
            {
                map.Data[x, y] *= factor;
            }
        }

        return map;
    }

    public static Map<float> RaiseElevation(this Map<float> map, float amount)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Depth; y++)
            {
                map.Data[x, y] += amount;
            }
        }

        return map;
    }

    // Set Border: Set a fixed value for a border of a given width around the map. This can be useful if you want a consistent edge, like a sea level around islands.
    public static Map<float> SetBorder(this Map<float> map, int borderWidth, float value)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < borderWidth; y++) map.Data[x, y] = value;
            for (int y = map.Depth - borderWidth; y < map.Depth; y++) map.Data[x, y] = value;
        }
        for (int y = 0; y < map.Depth; y++)
        {
            for (int x = 0; x < borderWidth; x++) map.Data[x, y] = value;
            for (int x = map.Width - borderWidth; x < map.Width; x++) map.Data[x, y] = value;
        }

        return map;
    }

    public static Map<float> GaussianBlur(this Map<float> map)
    {
        float[,] blurredData = new float[map.Width, map.Depth];
        int blurSize = 1;  // this is a simple 3x3 kernel

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Depth; y++)
            {
                float sum = 0;
                int count = 0;

                for (int i = -blurSize; i <= blurSize; i++)
                {
                    for (int j = -blurSize; j <= blurSize; j++)
                    {
                        int xi = x + i;
                        int yj = y + j;

                        if (xi >= 0 && xi < map.Width && yj >= 0 && yj < map.Depth)
                        {
                            sum += map.Data[xi, yj];
                            count++;
                        }
                    }
                }

                blurredData[x, y] = sum / count;
            }
        }

        return blurredData;  // Converted automatically due to the implicit conversion we defined earlier
    }
}

public static class Array2DExtensions
{
    public static IEnumerable<(int, int)> GetNeighbours(this Array array, int x, int y)
    {
        int lowerBoundX = array.GetLowerBound(0);
        int upperBoundX = array.GetUpperBound(0);

        int lowerBoundY = array.GetLowerBound(1);
        int upperBoundY = array.GetUpperBound(1);

        for (int i = Math.Max(lowerBoundX, x - 1); i <= Math.Min(upperBoundX, x + 1); i++)
        {
            for (int j = Math.Max(lowerBoundY, y - 1); j <= Math.Min(upperBoundY, y + 1); j++)
            {
                if (i != x || j != y)
                    yield return (i, j);
            }
        }
    }
}
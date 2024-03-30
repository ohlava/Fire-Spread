using System.Threading;

public static class RandomUtility
{
    // Each thread gets its own instance of System.Random, thus avoiding concurrent access issues.
    private static readonly ThreadLocal<System.Random> threadLocalRandom =
        new ThreadLocal<System.Random>(() => new System.Random());

    // System.Random class is not thread-safe on it's own. 
    public static System.Random Instance => threadLocalRandom.Value;

    // Generates a random float number within the specified range.
    public static float Range(float min, float max)
    {
        return (float)(Instance.NextDouble() * (max - min) + min);
    }

    // Generates a random integer between min (inclusive) and the max (exclusive)
    public static int Range(int min, int max)
    {
        return Instance.Next(min, max);
    }

    // Generates a random float between 0 and 1
    public static float NextFloat()
    {
        return (float)Instance.NextDouble();
    }

    // Generates a random integer between 0 (inclusive) and the specified max (exclusive)
    public static int NextInt(int max)
    {
        return Instance.Next(max);
    }
}
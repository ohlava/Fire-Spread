using System;

// configuration object for simulation parameters
public class FireSimParameters
{
    public float BaseSpreadProbability { get; set; }
    public float VegetationSpreadFactor { get; set; }
    public float MoistureSpreadFactor { get; set; }
    public float WindSpreadFactor { get; set; }
    public float SlopeSpreadFactor { get; set; }

    // Default constructor with all factors enabled.
    public FireSimParameters()
    {
        BaseSpreadProbability = 0.3f; // Cumulative probability of catching on fire being between 0.20f-0.35f seems to be nice
        VegetationSpreadFactor = 1.0f;
        MoistureSpreadFactor = 1.0f;
        WindSpreadFactor = 1.0f;
        SlopeSpreadFactor = 1.0f;
    }

    // Flexible constructor with optional parameters for each factor.
    // Each parameter can be either a bool (for enabled/disabled) or a float (for custom factors), defaulting to null.
    // This uses object? so it can accept either a bool or a float.
    public FireSimParameters(object vegetationSpread = null, object moistureSpread = null, object windSpread = null, object slopeSpread = null, object baseSpreadProbability = null)
    {
        BaseSpreadProbability = ParseFactor(baseSpreadProbability, 0.3f);
        VegetationSpreadFactor = ParseFactor(vegetationSpread, 1.0f);
        MoistureSpreadFactor = ParseFactor(moistureSpread, 1.0f);
        WindSpreadFactor = ParseFactor(windSpread, 1.0f);
        SlopeSpreadFactor = ParseFactor(slopeSpread, 1.0f);
    }

    // Helper method to parse the input (bool or float) and return an appropriate float value.
    private float ParseFactor(object factor, float defaultValue)
    {
        if (factor == null)
        {
            return defaultValue; // Use default if no value is provided.
        }
        else if (factor is bool)
        {
            return (bool)factor ? 1.0f : 0.0f; // Convert bool to float (1.0 or 0.0).
        }
        else if (factor is float)
        {
            return (float)factor; // Use the float value directly.
        }
        else
        {
            throw new ArgumentException("Factor must be either a bool or a float.");
        }
    }
}
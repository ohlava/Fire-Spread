using System;
using System.Collections.Generic;
using UnityEngine;

public class FireSimulation : SimulationBase
{
    private FireSimParameters _parameters;
    private List<Tile> _burningTiles;
    private FireLogger _fireLogger;

    public FireSimulation(FireSimParameters parameters, World world, List<Tile> initBurningTiles) : base(world)
    {
        _parameters = parameters;
        _fireLogger = new FireLogger();
        _calendar = new SimulationCalendar();

        _burningTiles = initBurningTiles; // When creating the simulation we have to tell what we set on fire.
        foreach (Tile tile in initBurningTiles)
        {
            _fireLogger.LogTileStartedBurning(_calendar.CurrentTime, tile);
        }

        SetWorldProperties();
    }

    protected override void SetWorldProperties()
    {
        foreach (Tile tile in _world.Grid)
        {
            tile.IsBurning = false;
            tile.BurningFor = 0;
            tile.IsBurned = false;

            switch (tile.Vegetation)
            {
                case VegetationType.Grass:
                    tile.BurnTime = 1;
                    break;
                case VegetationType.Sparse:
                    tile.BurnTime = 2;
                    break;
                case VegetationType.Swamp:
                    tile.BurnTime = 3;
                    break;
                case VegetationType.Forest:
                    tile.BurnTime = 4;
                    break;
                default:
                    Debug.Log("Some vegetation type is not handled.");
                    break;
            }
            if (tile.Moisture >= 60)
            {
                tile.BurnTime += 2;
            }
            else if (tile.Moisture >= 40 )
            {
                tile.BurnTime += 1;
            }
        }
    }

    public override bool Finished()
    {
        if (_burningTiles.Count == 0)
        {
            return true;
        }
        return false;
    }

    public override void Update()
    {
        // Advance the simulation calendar.
        _calendar.AdvanceTime();

        // All that are currently burning, but some will in this update stop burning, some will keep burning.
        List<Tile> nextBurningTiles = new List<Tile>(_burningTiles);

        // If this tile is burning, it may spread to neighbors.
        foreach (Tile tile in _burningTiles)
        {
            // Process neighbors at distance 1 and 2 - tiles further away can also catch on fire, but with much smaller probability
            for (int distance = 1; distance <= 2; distance++)
            {
                // Iterate over each neighboring tile.
                foreach (Tile neighborTile in _world.GetCircularEdgeNeighborTiles(tile, distance))
                {
                    // Adjust the spread probability based on the distance
                    float distanceModifier = distance == 1 ? 1f : 0.1f;

                    // Calculate the fire spread probability.
                    float spreadProbability = CalculateFireSpreadProbability(_world, tile, neighborTile, _parameters);
                    spreadProbability *= distanceModifier;

                    // Check if fire spread.
                    if (UnityEngine.Random.value < spreadProbability)
                    {
                        bool ignited = neighborTile.Ignite();
                        if (ignited && !nextBurningTiles.Contains(neighborTile))
                        {
                            nextBurningTiles.Add(neighborTile);
                            _fireLogger.LogTileStartedBurning(_calendar.CurrentTime, neighborTile);
                        }
                    }
                }
            }

            // Increment the burn time and extinguish the tile if it has been burning too long
            tile.BurningFor++;
            if (tile.BurningFor >= tile.BurnTime)
            {
                tile.Extinguish();
                nextBurningTiles.Remove(tile);
                _fireLogger.LogTileStoppedBurning(_calendar.CurrentTime, tile);
            }
        }

        // This allows us to iterate through the list of currently burning tiles without modification, while preparing the list of tiles that will be burning in the next update.
        _burningTiles = nextBurningTiles;
    }



    public List<FireEvent> GetLastUpdateEvents()
    {
        return _fireLogger.GetLastUpdateEvents(_calendar.CurrentTime);
    }

    public Dictionary<int, int> GetBurningTilesOverTime()
    {
        return _fireLogger.GetBurningTilesOverTime();
    }

    private float CalculateFireSpreadProbability(World world, Tile source, Tile target, FireSimParameters parameters)
    {
        float vegetationFactor = parameters.VegetationSpreadFactor > 0 ? GetVegetationFactor(target.Vegetation, parameters.VegetationSpreadFactor) : _parameters.BaseSpreadProbability;
        float slopeFactor = parameters.SlopeSpreadFactor > 0 ? GetSlopeFactor(source, target, parameters.SlopeSpreadFactor) : _parameters.BaseSpreadProbability;

        // First, average the vegetation and slope factors
        float combined = (vegetationFactor + slopeFactor) / 2;

        // Now adjust the probability based on the moisture and wind
        float moistureFactor = parameters.MoistureSpreadFactor > 0 ? GetMoistureFactor(target.Moisture, parameters.MoistureSpreadFactor) : 1.0f;
        float windFactor = parameters.WindSpreadFactor > 0 ? GetWindFactor(world, source, target, parameters.WindSpreadFactor) : 1.0f;

        // Apply the moisture and wind effects as percentage changes.
        float adjustedProbability = combined * moistureFactor * windFactor;
        adjustedProbability = Math.Max(0.0f, Math.Min(1.0f, adjustedProbability));

        if (adjustedProbability == 0.0f || adjustedProbability == 1.0f) return adjustedProbability; // No need to call GetStepFireProbability

        return GetStepFireProbability(adjustedProbability, source.BurnTime);
    }

    // Calculates the per-step probability of a tile catching fire, such that over the tile's BurnTime,
    // the total cumulative probability of catching fire is equal to the specified total probability. - uses binary search
    private float GetStepFireProbability(float totalProbability, int BurnTime)
    {
        float lowerBound = 0;
        float upperBound = 1;
        float p;

        for (int i = 0; i < 100; i++) // 100 should be enough iterations to get a good approximation
        {
            p = (lowerBound + upperBound) / 2;
            float calcTotalProbability = p * (1 - (float)Math.Pow(1 - p, BurnTime)) / p;

            if (calcTotalProbability > totalProbability)
            {
                upperBound = p;
            }
            else
            {
                lowerBound = p;
            }
        }

        return (lowerBound + upperBound) / 2;
    }



    private float GetVegetationFactor(VegetationType vegetation, float spreadFactor)
    {
        float factor = _parameters.BaseSpreadProbability;

        switch (vegetation)
        {
            case VegetationType.Grass:
                factor -= 0.12f;
                break;
            case VegetationType.Forest:
                factor += 0.1f;
                break;
            case VegetationType.Sparse:
                factor -= 0.05f;
                break;
            case VegetationType.Swamp:
                factor -= 0.08f;
                break;
            default:
                break;
        }

        return factor * spreadFactor;
    }

    private float GetMoistureFactor(float moisture, float spreadFactor)
    {
        float factor;

        // Two linear functions for moisture factor
        if (moisture < 50) // Moisture value is between 0 (dry) and 100 (water)
        {
            float x1 = 0f, y1 = 1f;
            float x2 = 50f, y2 = 0.8f;

            factor = y1 + ((moisture - x1) * (y2 - y1) / (x2 - x1));
        }
        else
        {
            float x1 = 50f, y1 = 0.8f;
            float x2 = 100f, y2 = 0f;

            factor = y1 + ((moisture - x1) * (y2 - y1) / (x2 - x1));
        }

        factor = Mathf.Clamp(factor, 0, 1);

        return factor * spreadFactor;
    }

    private float GetWindFactor(World world, Tile source, Tile target, float spreadFactor)
    {
        // Determine direction from source to target
        Vector2 dirSourceToTarget = new Vector2(target.WidthPosition - source.WidthPosition, target.DepthPosition - source.DepthPosition).normalized;
        Vector2 windDirectionVector = new Vector2(Mathf.Cos(world.Wind.WindDirection * Mathf.Deg2Rad), Mathf.Sin(world.Wind.WindDirection * Mathf.Deg2Rad));

        // Calculate the dot product between the wind direction and the direction to the target
        float dotProduct = Vector2.Dot(windDirectionVector, dirSourceToTarget);

        // Calculate wind speed factor (linearly from 0 to x% increase)
        float windSpeed = world.Wind.WindSpeed;
        float windSpeedFactor = Mathf.Min(windSpeed / 60.0f, 1.0f) * 0.8f; // Caps at 0.8 for 60+ wind speed

        // Combine the windSpeedFactor with the adjustmentFactor
        float factor = 1.0f + windSpeedFactor * dotProduct; // dotProduct is ranging from -1 to 0 (perpendicular) to 1

        // Ensure totalFactor is within bounds
        factor = Mathf.Clamp(factor, 0.4f, 1.8f); // Caps at 1.8 for 60+ wind speed in the same direction

        return factor * spreadFactor;
    }

    private float GetSlopeFactor(Tile source, Tile target, float spreadFactor)
    {
        // The spread factor will be higher for uphill slopes.
        float factor;

        // Points for linear interpolation based on the given slope and spread factor relationship
        float x1 = 10f, y1 = 1.5f;
        float x2 = 40f, y2 = 3f;

        float heightDifference = target.Height - source.Height;

        // Calculate the slope in degrees
        float slopeInDegrees = Mathf.Atan(heightDifference / 1) * (180f / Mathf.PI); // distance is considered to be 1

        // Calculate factor using linear interpolation formula
        factor = slopeInDegrees >= 0 ? y1 + ((slopeInDegrees - x1) * (y2 - y1) / (x2 - x1)) : 1f; // no change for downhill

        factor = factor * _parameters.BaseSpreadProbability;
        factor = Math.Min(0.8f, factor); // Maximal probability value

        return factor * spreadFactor;
    }
}








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
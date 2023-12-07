using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class FireSpreadSimulation
{
    private FireSpreadParameters _parameters;
    private World _world;
    private List<Tile> _burningTiles;
    private SimulationCalendar _calendar;
    private FireEventsLogger _eventLogger;

    public FireSpreadSimulation(FireSpreadParameters parameters, World world, List<Tile> initBurningTiles)
    {
        _parameters = parameters;
        _world = world;
        _eventLogger = new FireEventsLogger();
        _calendar = new SimulationCalendar();

        _burningTiles = initBurningTiles; // When creating the simulation we have to tell what we set on fire.
        foreach (Tile tile in initBurningTiles)
        {
            _eventLogger.LogEvent(new FireEvent(_calendar.CurrentTime, EventType.TileStartedBurning, tile));
        }
    }
    public bool Finished()
    {
        if (_burningTiles.Count == 0)
        {
            return true;
        }
        return false;
    }

    public void Update()
    {
        // Advance the simulation calendar.
        _calendar.AdvanceTime();

        // All that are currently burning, but some will in this update stop burning, some will keep burning.
        List<Tile> nextBurningTiles = new List<Tile>(_burningTiles);

        // If this tile is burning, it may spread to neighbors.
        foreach (Tile tile in _burningTiles)
        {
            // Iterate over each neighboring tile.
            foreach (Tile neighborTile in _world.GetNeighborTiles(tile))
            {
                // Calculate the fire spread probability.
                float spreadProbability = CalculateFireSpreadProbability(_world, tile, neighborTile, _parameters);

                // Check if fire spread.
                if (UnityEngine.Random.value < spreadProbability)
                {
                    bool ignited = neighborTile.Ignite();
                    if (ignited && !nextBurningTiles.Contains(neighborTile))
                    {
                        nextBurningTiles.Add(neighborTile);
                        _eventLogger.LogEvent(new FireEvent(_calendar.CurrentTime, EventType.TileStartedBurning, neighborTile));
                    }
                }
            }

            // Increment the burn time and extinguish the tile if it has been burning too long
            tile.BurningFor++;
            if (tile.BurningFor >= tile.BurnTime)
            {
                tile.Extinguish();
                nextBurningTiles.Remove(tile);
                _eventLogger.LogEvent(new FireEvent(_calendar.CurrentTime, EventType.TileStoppedBurning, tile));
            }
        }

        // This allows us to iterate through the list of currently burning tiles without modification, while preparing the list of tiles that will be burning in the next update.
        _burningTiles = nextBurningTiles;
    }

    public List<FireEvent> GetLastUpdateEvents()
    {
        return _eventLogger.GetLastUpdateEvents(_calendar.CurrentTime);
    }

    public Dictionary<int, int> GetBurningTilesOverTime()
    {
        return _eventLogger.GetBurningTilesOverTime();
    }

    private float CalculateFireSpreadProbability(World world, Tile source, Tile target, FireSpreadParameters parameters)
    {
        // TODO implement more reminiscent of reality / something like cumulative probability being 0.20f-0.35f seems to be nice
        // precalculate?

        float vegetationFactor = GetVegetationFactor(target.Vegetation, parameters.VegetationSpreadFactor);
        float moistureFactor = GetMoistureFactor(target.Moisture, parameters.MoistureSpreadFactor);
        float windFactor = GetWindFactor(world, source, target, parameters.WindSpreadFactor);
        float slopeFactor = GetSlopeFactor(source, target, parameters.SlopeSpreadFactor);

        // First, average the vegetation and slope factors
        float combined = (vegetationFactor + slopeFactor) / 2;

        // Now adjust the probability based on the moisture and wind
        // The more moisture, the less likely the fire is to spread, hence we multiply by moistureFactor.
        // The stronger the wind in the direction of the target, the more likely the fire is to spread, hence we multiply by windFactor.
        // Apply the moisture and wind effects as percentage changes.
        float adjustedProbability = combined * moistureFactor * windFactor;

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

    // Implement helper methods for calculating factors based on vegetation, moisture, wind, and slope.
    private float GetVegetationFactor(VegetationType vegetation, float spreadFactor)
    {
        float factor = 1.0f;

        switch (vegetation)
        {
            case VegetationType.Grass:
                factor = 0.18f;
                break;
            case VegetationType.Forest:
                factor = 0.4f;
                break;
            case VegetationType.Sparse:
                factor = 0.25f;
                break;
            case VegetationType.Swamp:
                factor = 0.22f;
                break;
        }

        return factor * spreadFactor;
    }

    private float GetMoistureFactor(float moisture, float spreadFactor)
    {
        //(1 - moisture) * spreadFactor;

        /// The spread factor will be higher for drier tiles.
        // Moisture value is between 0 (dry) and 1 (water). The factor will range from 0 (no effect) to -1 (full effect).

        if (moisture > 85)
        {
            return 0.5f;
        }
        else if (moisture > 65)
        {
            return 0.7f;
        }
        else
        {
            return 0.88f;
        }
    }

    private float GetWindFactor(World world, Tile source, Tile target, float spreadFactor)
    {
        // It has been measured that with a wind speed of 10 km/h, the fire spreads through the Australian bush at a speed of about 0.5 km/h.
        // If the wind speed increases to 20 km/h, the speed of the fire will increase to 0.8 km/h. At a wind speed of 40 km/h, the speed of fire progress is already 1.8 km/h
        // use interpolation

        // Determine direction from source to target
        Vector2 dirSourceToTarget = new Vector2(target.WidthPosition - source.WidthPosition, target.DepthPosition - source.DepthPosition).normalized;
        Vector2 windDirectionVector = new Vector2(Mathf.Cos(world.Weather.WindDirection * Mathf.Deg2Rad), Mathf.Sin(world.Weather.WindDirection * Mathf.Deg2Rad));

        // Calculate the dot product between the wind direction and the direction to the target
        float dotProduct = Vector2.Dot(windDirectionVector, dirSourceToTarget);

        // If dotProduct is close to 1, it means the wind is roughly in the same direction.
        if (dotProduct > 0.7)
        {
            return 1.5f;  // Increase the spread factor slightly
        }
        else
        {
            return 1f;  // Default value when wind is not in the target direction
        }
    }

    private float GetSlopeFactor(Tile source, Tile target, float spreadFactor)
    {
        // The spread factor will be higher for uphill slopes.
        // Experimentally, it was found that at a slope of 10°, the speed of fire progress increases twice, and at a slope of 40° even four times.

        float slopeDifference = target.Height - source.Height;

        if (slopeDifference >= 0)
        {
            return 0.35f * spreadFactor;
        }
        else
        {
            return 0.25f * spreadFactor;
        }
    }
}











public class FireSpreadParameters
{
    // Use bool to indicate whether a parameter is counted or not.
    public bool IncludeVegetationSpread { get; set; }
    public bool IncludeMoistureSpread { get; set; }
    public bool IncludeWindSpread { get; set; }
    public bool IncludeSlopeSpread { get; set; }

    // Instead of using float for factors, you use the boolean properties to determine whether to count the factor (1) or not (0).
    public float VegetationSpreadFactor => IncludeVegetationSpread ? 1.0f : 0.0f;
    public float MoistureSpreadFactor => IncludeMoistureSpread ? 1.0f : 0.0f;
    public float WindSpreadFactor => IncludeWindSpread ? 1.0f : 0.0f;
    public float SlopeSpreadFactor => IncludeSlopeSpread ? 1.0f : 0.0f;

    public FireSpreadParameters()
    {
        // Set default values for including spread factors.
        IncludeVegetationSpread = true; // 1 when counted, 0 when not
        IncludeMoistureSpread = true;   // 1 when counted, 0 when not
        IncludeWindSpread = true;       // 1 when counted, 0 when not
        IncludeSlopeSpread = true;      // 1 when counted, 0 when not
    }
}
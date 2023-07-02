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
    private EventLogger _eventLogger;

    public FireSpreadSimulation(FireSpreadParameters parameters, World world, List<Tile> initBurningTiles)
    {
        _parameters = parameters;
        _world = world;
        _eventLogger = new EventLogger();
        _calendar = new SimulationCalendar();

        _burningTiles = initBurningTiles; // When creating the simulation we have to tell what we set on fire.
        foreach (Tile tile in initBurningTiles)
        {
            _eventLogger.LogEvent(new FireEvent(_calendar.CurrentTime, EventType.StartedBurning, tile));
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
                float spreadProbability = CalculateFireSpreadProbability(tile, neighborTile, _world.Weather, _parameters);

                // Check if fire spread.
                if (UnityEngine.Random.value < spreadProbability)
                {
                    bool ignited = neighborTile.Ignite();
                    if (ignited && !nextBurningTiles.Contains(neighborTile))
                    {
                        nextBurningTiles.Add(neighborTile);
                        _eventLogger.LogEvent(new FireEvent(_calendar.CurrentTime, EventType.StartedBurning, neighborTile));
                    }
                }
            }

            // Increment the burn time and extinguish the tile if it has been burning too long
            tile.BurningFor++;
            if (tile.BurningFor >= tile.BurnTime)
            {
                tile.Extinguish();
                nextBurningTiles.Remove(tile);
                _eventLogger.LogEvent(new FireEvent(_calendar.CurrentTime, EventType.StoppedBurning, tile));
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

    private float CalculateFireSpreadProbability(Tile source, Tile target, Weather weather, FireSpreadParameters parameters)
    {
        // TODO implement more reminiscent of reality

        // float vegetationFactor = GetVegetationFactor(target.Vegetation, parameters.VegetationSpreadFactor);
        // float moistureFactor = GetMoistureFactor(target.Moisture, parameters.MoistureSpreadFactor);
        // float windFactor = GetWindFactor(source, target, weather, parameters.WindSpreadFactor);
        // float slopeFactor = GetSlopeFactor(source, target, parameters.SlopeSpreadFactor);

        // float combined = GetStepFireProbability(vegetationFactor * moistureFactor * slopeFactor * windFactor, source.BurnTime)

        float combined = UnityEngine.Random.Range(0.25f, 0.35f);
        return GetStepFireProbability(combined, source.BurnTime);
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
                factor = 0.8f;
                break;
            case VegetationType.Forest:
                factor = 1.2f;
                break;
            case VegetationType.Sparse:
                factor = 0.5f;
                break;
            case VegetationType.Swamp:
                factor = 0.2f;
                break;
        }

        return factor * spreadFactor;
    }

    private float GetMoistureFactor(float moisture, float spreadFactor)
    {
        // The spread factor will be higher for drier tiles.
        return (1 - moisture) * spreadFactor;
    }

    private float GetWindFactor(Tile source, Tile target, Weather weather, float spreadFactor)
    {
        // Calculate the wind direction towards the target tile.
        float targetDirection = 1;

        // Calculate the angular difference between the wind direction and the target direction.
        float angularDifference = Mathf.DeltaAngle(weather.WindDirection, targetDirection);

        // Calculate the wind factor based on the angular difference and wind strength.
        float windFactor = Mathf.Cos(Mathf.Deg2Rad * angularDifference) * weather.WindStrength * spreadFactor;

        return Mathf.Max(0, windFactor);
    }

    private float GetSlopeFactor(Tile source, Tile target, float spreadFactor)
    {
        float slopeDifference = target.Height - source.Height;

        // Assuming slopeDifference is between -1 (downhill) and 1 (uphill).
        // The spread factor will be higher for uphill slopes.
        return (1 + slopeDifference) * spreadFactor;
    }
}

public class EventLogger
{
    // The key is the time, and the value is a list of events that happened at that time.
    private Dictionary<int, List<FireEvent>> _events;

    public EventLogger()
    {
        // Initialize the dictionary in the constructor
        _events = new Dictionary<int, List<FireEvent>>();
    }

    public void LogEvent(FireEvent evt)
    {
        // Extract the time from the event
        int time = evt.Time;

        // Check if there are already events at this time
        if (_events.ContainsKey(time))
        {
            // If there are, add the new event to the existing list
            _events[time].Add(evt);
        }
        else
        {
            // If there are no events at this time, create a new list and add the event
            _events[time] = new List<FireEvent>() { evt };
        }
    }

    public List<FireEvent> GetLastUpdateEvents(int time)
    {
        // First, check if there are events at this time
        if (!_events.ContainsKey(time))
        {
            // If there are no events, return an empty list
            return new List<FireEvent>();
        }

        // If there are events, return the list of events
        return _events[time];
    }

    public Dictionary<int, int> GetBurningTilesOverTime()
    {
        Dictionary<int, int> burningTilesOverTime = new Dictionary<int, int>();

        // Track current count of burning tiles
        int currentBurningCount = 0;

        // Iterate over events in chronological order
        foreach (var timeEvents in _events.OrderBy(e => e.Key))
        {
            // At each time step, calculate the count of burning tiles
            foreach (FireEvent evt in timeEvents.Value)
            {
                if (evt.Type == EventType.StartedBurning)
                {
                    currentBurningCount++;
                }
                else if (evt.Type == EventType.StoppedBurning)
                {
                    currentBurningCount--;
                }
            }

            burningTilesOverTime[timeEvents.Key] = currentBurningCount;
        }

        return burningTilesOverTime;
    }
}

public class SimulationCalendar
{
    public int CurrentTime { get; private set; }

    public SimulationCalendar()
    {
        CurrentTime = 0;
    }

    public void AdvanceTime()
    {
        CurrentTime++;
    }
}

public class FireEvent
{
    public int Time { get; private set; }
    public EventType Type { get; private set; }
    public Tile Tile { get; private set; }

    public FireEvent(int time, EventType type, Tile tile)
    {
        Time = time;
        Type = type;
        Tile = tile;
    }
}

public enum EventType
{
    StartedBurning,
    StoppedBurning
}

public class FireSpreadParameters
{
    public float VegetationSpreadFactor { get; set; }
    public float MoistureSpreadFactor { get; set; }
    // public float WindSpreadFactor { get; set; }
    public float SlopeSpreadFactor { get; set; }

    public FireSpreadParameters()
    {
        // Set default values for the spread factors.
        VegetationSpreadFactor = 1.0f;
        MoistureSpreadFactor = 1.0f;
        // WindSpreadFactor = 1.0f;
        SlopeSpreadFactor = 1.0f;
    }
}
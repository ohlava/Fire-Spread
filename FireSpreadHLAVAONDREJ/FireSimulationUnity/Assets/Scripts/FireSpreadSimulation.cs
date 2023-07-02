using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FireSpreadSimulation
{
    private FireSpreadParameters _parameters;
    private World _world;
    private EventLogger _eventLogger;
    private SimulationCalendar _calendar;

    private List<FireEvent> _lastUpdateEvents;

    public FireSpreadSimulation(FireSpreadParameters parameters, World world)
    {
        _parameters = parameters;
        _world = world;
        _eventLogger = new EventLogger();
        _calendar = new SimulationCalendar();
    }

    // TODO add list of burning tiles and iterate threw it now all tiles!

    public void Update()
    {
        // Advance the simulation calendar.
        _calendar.AdvanceTime();
        _lastUpdateEvents = new List<FireEvent>();

        // Iterate over each tile in the world.
        for (int x = 0; x < _world.Width; x++)
        {
            for (int y = 0; y < _world.Depth; y++)
            {
                Tile tile = _world.GetTileAt(x, y);


                // If this tile is burning, it may spread to neighbors.
                if (tile.IsBurning)
                {
                    // Iterate over each neighboring tile.
                    foreach (Tile neighborTile in GetNeighborTiles(x, y))
                    {

                        // Calculate the fire spread probability.
                        float spreadProbability = CalculateFireSpreadProbability(tile, neighborTile, _world.Weather, _parameters);

                        // Check if fire spread.
                        if (Random.value < spreadProbability)
                        {
                            if (neighborTile.Ignite())
                            {
                                FireEvent fireEvent = new FireEvent() { Tile = neighborTile, EventType = EventType.StartedBurning, Time = _calendar.CurrentTime };
                                _eventLogger.LogEvent(fireEvent);
                                _lastUpdateEvents.Add(fireEvent);
                            }
                        }
                    }

                    // Increment the burn time and extinguish the tile if it has been burning too long.
                    tile.BurningFor++;
                    if (tile.BurningFor > tile.BurnTime)
                    {
                        tile.Extinguish();
                        FireEvent fireEvent = new FireEvent() { Tile = tile, EventType = EventType.StoppedBurning, Time = _calendar.CurrentTime };
                        _eventLogger.LogEvent(fireEvent);
                        _lastUpdateEvents.Add(fireEvent);
                        continue;
                    }
                }
            }
        }
        _eventLogger.LogLastUpdateEvents(_lastUpdateEvents);
    }

    public List<FireEvent> GetLastUpdateEvents()
    {
        return _eventLogger.GetLastUpdateEvents();
    }

    // Returns a list of neighboring tiles given the coordinates of a tile.
    private List<Tile> GetNeighborTiles(int x, int y)
    {
        List<Tile> neighbours = new List<Tile>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int nx = x + i;
                int ny = y + j;

                if (nx != x || ny != y) // not the same
                {
                    if (nx >= 0 && nx < _world.Width && ny >= 0 && ny < _world.Depth)
                    {
                        neighbours.Add(_world.GetTileAt(nx, ny));
                    }
                }
            }
        }
        return neighbours;
    }

    private float CalculateFireSpreadProbability(Tile source, Tile target, Weather weather, FireSpreadParameters parameters)
    {
        float vegetationFactor = GetVegetationFactor(target.Vegetation, parameters.VegetationSpreadFactor);
        float moistureFactor = GetMoistureFactor(target.Moisture, parameters.MoistureSpreadFactor);
        // float windFactor = GetWindFactor(source, target, weather, parameters.WindSpreadFactor);
        float slopeFactor = GetSlopeFactor(source, target, parameters.SlopeSpreadFactor);

        return 100f;
        //return vegetationFactor * moistureFactor * slopeFactor; // * windFactor
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
        }

        return factor * spreadFactor;
    }

    private float GetMoistureFactor(float moisture, float spreadFactor)
    {
        // Assuming moisture is a value between 0 (dry) and 1 (wet).
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
    // TODO should it rather have list of all events _lastUpdateEvents / list of lists
    private List<FireEvent> _events;
    private List<List<FireEvent>> _updateEvents;

    public EventLogger()
    {
        _events = new List<FireEvent>();
        _updateEvents = new List<List<FireEvent>>();
    }

    public void LogEvent(FireEvent evt)
    {
        _events.Add(evt);
    }

    public void LogLastUpdateEvents(List<FireEvent> lue)
    {
        _updateEvents.Add(lue);
    }

    public List<FireEvent> GetEvents()
    {
        return _events;
    }

    public List<FireEvent> GetLastUpdateEvents()
    {
        // Check if _updateEvents is not null and contains at least one list
        if (_updateEvents != null && _updateEvents.Count > 0)
        {
            // Get the last list from _updateEvents
            List<FireEvent> lastList = _updateEvents[_updateEvents.Count - 1];
            return lastList;
        }
        else
        {
            // Return an empty list if _updateEvents is null or empty
            return new List<FireEvent>();
        }
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
    public Tile Tile { get; set; }
    public EventType EventType { get; set; }
    public int Time { get; set; }
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
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Unified generic Event Logger
public class EventLogger<T> where T : Event
{
    // Stores events based on the time they occurred.
    private protected Dictionary<int, List<T>> events = new Dictionary<int, List<T>>();

    // Logs an event, storing it in the dictionary under the time it occurred.
    public void LogEvent(T evt)
    {
        int time = evt.Time;

        if (events.ContainsKey(time))
        {
            events[time].Add(evt);
        }
        else
        {
            events[time] = new List<T>() { evt };
        }
    }

    // Retrieves a list of events that occurred at a specific time.
    public List<T> GetLastUpdateEvents(int time)
    {
        if (!events.ContainsKey(time))
        {
            return new List<T>();
        }

        return events[time];
    }
}

// Logger class for wind change events.
public class WindLogger : EventLogger<WindEvent>
{
    // Logs a wind direction change
    public void LogWindDirectionChange(int time, int oldDirection, int newDirection)
    {
        LogEvent(new WindEvent(time, EventType.WindDirectionChange, oldDirection, newDirection));
    }

    // Logs a wind speed change
    public void LogWindSpeedChange(int time, float oldSpeed, float newSpeed)
    {
        LogEvent(new WindEvent(time, EventType.WindSpeedChange, oldSpeed, newSpeed));
    }

    // Prints a summary of all logged weather change events.
    public void PrintWeatherEventsSummary()
    {
        // Calculate the total number of wind events
        int totalWindEvents = events.Values.Sum(events => events.Count(evt => evt.Type == EventType.WindDirectionChange || evt.Type == EventType.WindSpeedChange));

        Debug.Log($"Total Wind Events: {totalWindEvents}");

        foreach (var kvp in events)
        {
            int time = kvp.Key;
            foreach (var evt in kvp.Value) // Iterate through each event in the current time slot
            {
                switch (evt.Type)
                {
                    case EventType.WindDirectionChange:
                        Debug.Log($"Time: {time}, Event: Wind Direction Change, Old Direction: {evt.OldWindDirection}, New Direction: {evt.NewWindDirection}");
                        break;
                    case EventType.WindSpeedChange:
                        Debug.Log($"Time: {time}, Event: Wind Speed Change, Old Speed: {evt.OldWindSpeed}km/h, New Speed: {evt.NewWindSpeed}km/h");
                        break;
                    default:
                        Debug.Log($"Time: {time}, Event: Unhandled Event Type");
                        break;
                }
            }
        }
    }
}

// Logger class specific to fire events.
public class FireLogger : EventLogger<FireEvent>
{
    public void LogTileStartedBurning(int time, Tile tile)
    {
        LogEvent(new FireEvent(time, EventType.TileStartedBurning, tile));
    }

    public void LogTileStoppedBurning(int time, Tile tile)
    {
        LogEvent(new FireEvent(time, EventType.TileStoppedBurning, tile));
    }

    // Calculates and returns the number of burning tiles over time.
    public Dictionary<int, int> GetBurningTilesOverTime()
    {
        Dictionary<int, int> burningTilesOverTime = new Dictionary<int, int> {{ -1, 0 }};
        int currentBurningCount = 0;

        // Iterate over events in chronological order
        foreach (var timeEvents in events)
        {
            // At each time step, calculate the count of burning tiles
            foreach (FireEvent evt in timeEvents.Value)
            {
                if (evt.Type == EventType.TileStartedBurning)
                {
                    currentBurningCount++;
                }
                else if (evt.Type == EventType.TileStoppedBurning)
                {
                    currentBurningCount--;
                }
            }

            burningTilesOverTime[timeEvents.Key] = currentBurningCount;
        }

        return burningTilesOverTime;

    }

    // Method to get a summary of fire events
    public void PrintFireEventsSummary()
    {
        int totalEvents = events.Sum(e => e.Value.Count);
        Debug.Log($"Total Fire Events: {totalEvents}");

        foreach (var kvp in events)
        {
            int time = kvp.Key;
            foreach (var evt in kvp.Value)
            {
                string eventTypeDescription = evt.Type switch
                {
                    EventType.TileStartedBurning => "Tile Started Burning",
                    EventType.TileStoppedBurning => "Tile Stopped Burning",
                    _ => "Unknown Fire Event"
                };

                Debug.Log($"Time: {time}, Event: {eventTypeDescription}, Tile: {evt.Tile} at ({evt.Tile.WidthPosition}, {evt.Tile.DepthPosition})");
            }
        }
    }
}
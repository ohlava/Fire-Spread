using System;
using System.Collections.Generic;
using System.Linq;

// Unified Event Logger
public class EventLogger<T> where T : Event
{
    // Stores events based on the time they occurred.
    private protected Dictionary<int, List<T>> _events = new Dictionary<int, List<T>>();

    // Logs an event, storing it in the dictionary under the time it occurred.
    public void LogEvent(T evt)
    {
        // Extract the time from the event
        int time = evt.Time;

        if (_events.ContainsKey(time))
        {
            // If there are, add the new event to the existing list
            _events[time].Add(evt);
        }
        else
        {
            // If there are no events at this time, create a new list and add the event
            _events[time] = new List<T>() { evt };
        }
    }

    // Retrieves a list of events that occurred at a specific time.
    public List<T> GetLastUpdateEvents(int time)
    {
        if (!_events.ContainsKey(time))
        {
            // If there are no events, return an empty list
            return new List<T>();
        }

        // If there are events, return the list of events
        return _events[time];
    }
}

// Logger class for weather change events.
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
        int totalWindEvents = _events.Values.Sum(events => events.Count(evt => evt.Type == EventType.WindDirectionChange || evt.Type == EventType.WindSpeedChange));

        Console.WriteLine($"Total Wind Events: {totalWindEvents}");

        // Iterate through each time slot that contains events
        foreach (var kvp in _events)
        {
            int time = kvp.Key;
            // Iterate through each event in the current time slot
            foreach (var evt in kvp.Value)
            {
                // Determine the type of event and print details accordingly
                switch (evt.Type)
                {
                    case EventType.WindDirectionChange:
                        Console.WriteLine($"Time: {time}, Event: Wind Direction Change, Old Direction: {evt.OldWindDirection}, New Direction: {evt.NewWindDirection}");
                        break;
                    case EventType.WindSpeedChange:
                        Console.WriteLine($"Time: {time}, Event: Wind Speed Change, Old Speed: {evt.OldWindSpeed}km/h, New Speed: {evt.NewWindSpeed}km/h");
                        break;
                    default:
                        // Handle unexpected event types
                        Console.WriteLine($"Time: {time}, Event: Unhandled Event Type");
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
        Dictionary<int, int> burningTilesOverTime = new Dictionary<int, int>();
        // Track current count of burning tiles
        int currentBurningCount = 0;

        // Iterate over events in chronological order
        foreach (var timeEvents in _events)
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
        int totalEvents = _events.Sum(e => e.Value.Count);
        Console.WriteLine($"Total Fire Events: {totalEvents}");

        foreach (var kvp in _events)
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

                Console.WriteLine($"Time: {time}, Event: {eventTypeDescription}, Tile: {evt.Tile}");
            }
        }
    }
}
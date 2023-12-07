using System;
using System.Collections.Generic;
using System.Linq;



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

public enum EventType
{
    TileStartedBurning,
    TileStoppedBurning,
    WeatherChange
}

// Base Event class for generic event handling
public class Event
{
    public int Time { get; protected set; }
    public EventType Type { get; protected set; }
}

// Specific event for fire
public class FireEvent : Event
{
    public Tile Tile { get; private set; }

    public FireEvent(int time, EventType type, Tile tile)
    {
        Time = time;
        Type = type;
        Tile = tile;
    }
}

public class WeatherEvent : Event
{
    public string Property { get; private set; }
    public object OldValue { get; private set; }
    public object NewValue { get; private set; }

    public WeatherEvent(int time, string property, object oldValue, object newValue)
    {
        Time = time;
        Type = EventType.WeatherChange;
        Property = property;
        OldValue = oldValue;
        NewValue = newValue;
    }
}





// Unified Event Logger
public class EventLogger<T> where T : Event
{
    private protected Dictionary<int, List<T>> _events = new Dictionary<int, List<T>>();

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



public class WeatherChangeLogger : EventLogger<WeatherEvent>
{
    private List<WeatherEvent> logs = new List<WeatherEvent>();

    public void LogChange(string property, object oldValue, object newValue)
    {
        //TPDP add time / from simulation calendar
        logs.Add(new WeatherEvent(1, property, oldValue, newValue));
    }

    public void PrintWeatherChangeLogs()
    {
        foreach (var evt in logs)
        {
            Console.WriteLine($"Time: {evt.Time}, Property: {evt.Property}, Old Value: {evt.OldValue}, New Value: {evt.NewValue}");
        }
    }
}

public class FireEventsLogger : EventLogger<FireEvent>
{
    public Dictionary<int, int> GetBurningTilesOverTime()
    {
        Dictionary<int, int> burningTilesOverTime = new Dictionary<int, int>();
        int currentBurningCount = 0;

        foreach (var timeEvents in _events)
        {
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
}
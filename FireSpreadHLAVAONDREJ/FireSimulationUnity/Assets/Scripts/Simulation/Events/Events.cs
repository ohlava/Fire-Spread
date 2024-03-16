using System;
using System.Collections.Generic;
using System.Linq;

public enum EventType
{
    TileStartedBurning,
    TileStoppedBurning,
    WindDirectionChange,
    WindSpeedChange
}

// Base Event class for generic event handling.
public class Event
{
    public int Time { get; protected set; }
    public EventType Type { get; protected set; }
}

// Event class specific to fire-related events in the simulation.
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

// Event class specific for wind-related events in the simulation.
public class WindEvent : Event
{
    public int OldWindDirection { get; private set; }
    public int NewWindDirection { get; private set; }
    public float OldWindSpeed { get; private set; }
    public float NewWindSpeed { get; private set; }

    // Constructor for wind direction changes
    public WindEvent(int time, EventType type, int oldWindDirection, int newWindDirection)
    {
        Time = time;
        Type = type;
        OldWindDirection = oldWindDirection;
        NewWindDirection = newWindDirection;
    }

    // Constructor for wind speed changes
    public WindEvent(int time, EventType type, float oldWindSpeed, float newWindSpeed)
    {
        Time = time;
        Type = type;
        OldWindSpeed = oldWindSpeed;
        NewWindSpeed = newWindSpeed;
    }
}
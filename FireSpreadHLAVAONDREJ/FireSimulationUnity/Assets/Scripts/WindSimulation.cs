using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindSimulation
{
    private World _world;
    private WindLogger _windLogger;
    private SimulationCalendar _calendar;

    public WindSimulation(World world)
    {
        _world = world;
        _windLogger = new WindLogger();
        _calendar = new SimulationCalendar();
    }

    public void Update()
    {
        // Example logic to update weather, this needs to be fleshed out based on specific needs
        int windDirectionChange = UnityEngine.Random.Range(-15, 15);
        float windStrengthChange = UnityEngine.Random.Range(-3f, 3f);

        var oldDirection = _world.Wind.WindDirection;
        var oldSpeed = _world.Wind.WindSpeed;

        _world.Wind.WindDirection += windDirectionChange;
        _world.Wind.WindSpeed += windStrengthChange;

        // Log changes
        _windLogger.LogWindDirectionChange(_calendar.CurrentTime, oldDirection, _world.Wind.WindDirection);
        _windLogger.LogWindSpeedChange(_calendar.CurrentTime, oldSpeed, _world.Wind.WindSpeed);

        // Advance simulation time
        _calendar.AdvanceTime();
    }

    public List<WindEvent> GetLastUpdateEvents()
    {
        return _windLogger.GetLastUpdateEvents(_calendar.CurrentTime);
    }
}
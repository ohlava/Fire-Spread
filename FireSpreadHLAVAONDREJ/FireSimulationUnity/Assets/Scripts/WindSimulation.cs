using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindSimulation
{
    private World _world;
    private WeatherLogger _weatherLogger;
    private SimulationCalendar _calendar;

    public WindSimulation(World world)
    {
        _world = world;
        _weatherLogger = new WeatherLogger();
        _calendar = new SimulationCalendar();
    }

    public void Update()
    {
        // Example logic to update weather, this needs to be fleshed out based on specific needs
        int windDirectionChange = UnityEngine.Random.Range(-15, 15);
        float windStrengthChange = UnityEngine.Random.Range(-3f, 3f);

        var oldDirection = _world.Weather.WindDirection;
        var oldSpeed = _world.Weather.WindSpeed;

        _world.Weather.WindDirection += windDirectionChange;
        _world.Weather.WindSpeed += windStrengthChange;

        // Log changes
        _weatherLogger.LogWindDirectionChange(_calendar.CurrentTime, oldDirection, _world.Weather.WindDirection);
        _weatherLogger.LogWindSpeedChange(_calendar.CurrentTime, oldSpeed, _world.Weather.WindSpeed);

        // Advance simulation time
        _calendar.AdvanceTime();
    }

    public List<WeatherEvent> GetLastUpdateEvents()
    {
        return _weatherLogger.GetLastUpdateEvents(_calendar.CurrentTime);
    }
}
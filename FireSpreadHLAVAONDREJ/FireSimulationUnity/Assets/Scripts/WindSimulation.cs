using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindSimulation : SimulationBase
{
    private WindLogger _windLogger;

    public WindSimulation(World world) : base(world)
    {
        _world = world;
        _windLogger = new WindLogger();
        _calendar = new SimulationCalendar();

        SetWorldProperties();
    }

    public override void Update()
    {
        // Advance simulation time
        _calendar.AdvanceTime();

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
    }

    protected override void SetWorldProperties()
    {
        _world.Wind = new Wind(0, 15);
    }

    public override bool Finished()
    {
        return false;
    }

    public List<WindEvent> GetLastUpdateEvents()
    {
        return _windLogger.GetLastUpdateEvents(_calendar.CurrentTime);
    }
}
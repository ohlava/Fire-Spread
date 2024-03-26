using System.Collections.Generic;
using System.ComponentModel.Design;

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

    // Advances simulation time, also randomly adjusting wind direction and speed within defined limits, logs these changes.
    public override void Update()
    {
        _calendar.AdvanceTime();

        System.Random rand = new System.Random();
        int windDirectionChange = rand.Next(-15, 15);
        float windStrengthChange = (float)(rand.NextDouble() * (3.0 - (-3.0)) + (-3.0)); // between -3 and +3

        var oldDirection = _world.Wind.WindDirection;
        var oldSpeed = _world.Wind.WindSpeed;

        _world.Wind.WindDirection += windDirectionChange;
        _world.Wind.WindSpeed += windStrengthChange;

        _windLogger.LogWindDirectionChange(_calendar.CurrentTime, oldDirection, _world.Wind.WindDirection);
        _windLogger.LogWindSpeedChange(_calendar.CurrentTime, oldSpeed, _world.Wind.WindSpeed);
    }

    // Initializes the simulation's world with a starting wind state.
    protected override void SetWorldProperties()
    {
        _world.Wind = new Wind();
    }

    // Designed to run indefinitely.
    public override bool Finished()
    {
        return false;
    }

    // Retrieves logged wind events (direction and speed changes) for the most recent simulation update.
    public List<WindEvent> GetLastUpdateEvents()
    {
        return _windLogger.GetLastUpdateEvents(_calendar.CurrentTime);
    }
}
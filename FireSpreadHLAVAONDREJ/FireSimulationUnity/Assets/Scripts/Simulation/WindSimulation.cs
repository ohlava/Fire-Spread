using System.Collections.Generic;

public class WindSimulation : SimulationBase
{
    private WindLogger windLogger;

    public WindSimulation(World world) : base(world)
    {
        windLogger = new WindLogger();

        SetWorldProperties();
    }

    // Advances simulation time, also randomly adjusting wind direction and speed within defined limits, logs these changes.
    public override void Update()
    {
        calendar.AdvanceTime();

        int windDirectionChange = RandomUtility.Range(-15, 15);
        float windStrengthChange = RandomUtility.Range(-3.0f, 3.0f);

        var oldDirection = world.Wind.WindDirection;
        var oldSpeed = world.Wind.WindSpeed;

        world.Wind.WindDirection += windDirectionChange;
        world.Wind.WindSpeed += windStrengthChange;

        windLogger.LogWindDirectionChange(calendar.CurrentTime, oldDirection, world.Wind.WindDirection);
        windLogger.LogWindSpeedChange(calendar.CurrentTime, oldSpeed, world.Wind.WindSpeed);
    }

    // Initializes the simulation's world with a starting wind state.
    protected override void SetWorldProperties()
    {
        world.Wind = new Wind();
    }

    // Designed to run indefinitely.
    public override bool Finished()
    {
        return false;
    }

    // Retrieves logged wind events (direction and speed changes) for the most recent simulation update.
    public List<WindEvent> GetLastUpdateEvents()
    {
        return windLogger.GetLastUpdateEvents(calendar.CurrentTime);
    }
}
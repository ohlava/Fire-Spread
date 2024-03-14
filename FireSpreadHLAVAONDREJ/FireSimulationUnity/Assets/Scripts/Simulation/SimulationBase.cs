// Defines a common interface for all simulations.
public interface ISimulation
{
    void Update();
}

// Represents a calendar for managing time within the simulation.
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

public abstract class SimulationBase : ISimulation
{
    protected World _world;
    protected SimulationCalendar _calendar;

    public SimulationBase(World world)
    {
        _world = world;
        _calendar = new SimulationCalendar();
    }

    public abstract void Update();
    protected abstract void SetWorldProperties();
    public abstract bool Finished();
}
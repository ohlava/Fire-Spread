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

// Serves as the foundational class for simulations, integrating a world model and a time management system via SimulationCalendar.
// It requires derived classes to implement specific update behaviors, world and tiles state settings, and a termination condition.
public abstract class SimulationBase : ISimulation
{
    protected World world;
    protected SimulationCalendar calendar;

    public SimulationBase(World world)
    {
        this.world = world;
        calendar = new SimulationCalendar();
    }

    public abstract void Update();
    protected abstract void SetWorldProperties();
    public abstract bool Finished();
}
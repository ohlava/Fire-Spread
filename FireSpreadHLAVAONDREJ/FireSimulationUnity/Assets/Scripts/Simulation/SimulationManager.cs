using System.Collections.Generic;

// Manages the execution and coordination of different simulations.
public class SimulationManager
{
    private World world;
    private List<ISimulation> simulations;
    private FireSimulation mainFireSimulation; // Right now master simulation that will lead other simulations to stop

    public SimulationManager(World world)
    {
        this.world = world;
        simulations = new List<ISimulation>();
    }

    // Adds a new simulation to the manager, returns self for ability to chain this function
    public SimulationManager AddSimulation(ISimulation simulation)
    {
        simulations.Add(simulation);
        if (simulation is FireSimulation)
        {
            mainFireSimulation = simulation as FireSimulation;
        }

        return this;
    }

    // Updates all the simulations once in order of how they were added.
    public void UpdateAllSimulations()
    {
        foreach (var simulation in simulations)
        {
            if (simulation is not null)
            {
                simulation.Update();
            }
        }
    }

    // Runs all simulations to completion, or until the main/master fire simulation finishes.
    public void RunAllSimulations()
    {
        ResetWorld();

        while (!mainFireSimulation.Finished())
        {
            UpdateAllSimulations();
        }
    }

    // Resets the world to its initial state for a new simulations to run.
    private void ResetWorld()
    {
        world.Reset();
    }
}
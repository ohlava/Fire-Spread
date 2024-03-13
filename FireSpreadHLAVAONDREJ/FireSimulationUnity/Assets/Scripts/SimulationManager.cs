using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Defines a common interface for all simulations.
public interface ISimulation
{
    void Update();
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

// Manages the execution and coordination of different simulations.
public class SimulationManager
{
    private World _world;
    private List<ISimulation> _simulations;
    private FireSimulation _mainFireSimulation;

    public SimulationManager(World world)
    {
        _world = world;
        _simulations = new List<ISimulation>();
    }

    // Adds a new simulation to the manager, returns self for ability to chain this function
    public SimulationManager AddSimulation(ISimulation simulation)
    {
        _simulations.Add(simulation);
        if (simulation is FireSimulation)
        {
            _mainFireSimulation = simulation as FireSimulation;
        }

        return this;
    }

    // Updates all the simulations once in order of how they were added.
    public void UpdateAllSimulations()
    {
        foreach (var simulation in _simulations)
        {
            if (simulation is not null)
            {
                simulation.Update();
            }
        }
    }

    // Runs all simulations to completion, or until the main fire simulation finishes.
    public void RunAllSimulations()
    {
        ResetWorld();

        while (!_mainFireSimulation.Finished())
        {
            UpdateAllSimulations();
        }
    }

    // Resets the world to its initial state for a new simulations to run.
    private void ResetWorld()
    {
        _world.Reset();
    }
}
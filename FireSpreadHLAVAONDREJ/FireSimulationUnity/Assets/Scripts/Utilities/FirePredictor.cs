using System.Collections.Generic;

public class FirePredictor
{
    private FireSimParameters _fireParams;
    private List<Tile> _initBurningTiles;
    private World _world;

    public FirePredictor(FireSimParameters fireParams)
    {
        _fireParams = fireParams;
    }

    // Generates a heat map representing the intensity of fires across a given world running multiple simulations.
    public Map<float> GenerateHeatMap(int iterations, World world, List<Tile> initBurningTiles)
    {
        _world = world;
        _initBurningTiles = initBurningTiles;

        Map<float> heatMap = new Map<float>(world.Width, world.Depth);

        // Run the simulation for the specified number of iterations and collect the final state of the world for each run.
        List<World> runnedWorlds = GetSimulationRunnedWorlds(iterations);

        // Aggregate burn data from all simulation runs to calculate heat intensity.
        foreach (World runnedWorld in runnedWorlds)
        {
            for (int i = 0; i < world.Width; i++)
            {
                for (int j = 0; j < world.Depth; j++)
                {
                    if (runnedWorld.Grid[i, j].IsBurned)
                    {
                        // Increment heat map value for burned tiles in each run.
                        heatMap.Data[i, j] += 1.0f;
                    }
                }
            }
        }

        // Normalize the heat map by the number of iterations to get average heat intensity.
        for (int i = 0; i < world.Width; i++)
        {
            for (int j = 0; j < world.Depth; j++)
            {
                heatMap.Data[i, j] = heatMap.Data[i, j] / iterations;
            }
        }

        return heatMap;
    }

    // Runs the fire simulation multiple times and captures the state of the world after each run.
    private List<World> GetSimulationRunnedWorlds(int iterations)
    {
        // Convert initial burning tiles to a list of indices for reusability across simulation runs.
        List<(int, int)> initBurningTilesIndices = new List<(int, int)>();
        foreach (Tile tile in _initBurningTiles)
        {
            initBurningTilesIndices.Add((tile.WidthPosition, tile.DepthPosition));
        }

        List<World> runnedWorlds = new List<World>();
        for (int i = 0; i < iterations; i++)
        {
            World worldCopy = new World(_world);
            worldCopy = RunSimulation(worldCopy, initBurningTilesIndices); // Run the simulation on the copied world.
            runnedWorlds.Add(worldCopy);
        }

        return runnedWorlds;
    }

    // Function that runs the same simulation multiple times on one world and returns
    private World RunSimulation(World world, List<(int, int)> initBurningTiles)
    {
        SimulationManager manager = new SimulationManager(world);
        List<Tile> specificBurningTiles = new List<Tile>();

        // Convert initial burning tile indices back to Tile objects for that specific new world.
        foreach (var tuple in initBurningTiles)
        {
            Tile tile = world.GetTileAt(tuple.Item1, tuple.Item2);
            specificBurningTiles.Add(tile);
        }

        // Initialize the fire and wind simulations with specific parameters and conditions.
        FireSimulation fireSim = new FireSimulation(_fireParams, world, specificBurningTiles);
        WindSimulation windSim = new WindSimulation(world);
        manager.AddSimulation(fireSim).AddSimulation(windSim);

        // Run all added simulations, modifying the world's state.
        manager.RunAllSimulations();

        return world;
    }
}
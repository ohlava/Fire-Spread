using System.Collections.Generic;
using System.Threading.Tasks;

public class FirePredictor
{
    private FireSimParameters fireParams;
    private List<Tile> initBurningTiles;
    private World world;

    public FirePredictor(FireSimParameters fireParams)
    {
        this.fireParams = fireParams;
    }

    // Generates a heat map representing the intensity of fires across a given world running multiple simulations and collecting the final state of the world for each run.
    public Map<float> GenerateHeatMap(int iterations, World world, List<Tile> initBurningTiles)
    {
        this.world = world;
        this.initBurningTiles = initBurningTiles;

        Map<float> heatMap = new Map<float>(world.Width, world.Depth);

        // Task.Run execute simulations in parallel. Running simulation on worlds - each task, different runned world.
        var tasks = new Task<World>[iterations];
        for (int i = 0; i < iterations; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                var worldCopy = new World(this.world);
                return RunSimulation(worldCopy, ConvertToIndices(this.initBurningTiles)); // Run simulation and return the resulting World
            });
        }

        Task.WaitAll(tasks); // Wait for all tasks to complete.

        // Aggregate results from each completed task
        foreach (var task in tasks)
        {
            World runnedWorld = task.Result;
            for (int i = 0; i < world.Width; i++)
            {
                for (int j = 0; j < world.Depth; j++)
                {
                    if (runnedWorld.Grid[i, j].IsBurned)
                    {
                        heatMap.Data[i, j] += 1.0f;
                    }
                }
            }
        }

        // Normalize the heat map.
        for (int i = 0; i < world.Width; i++)
        {
            for (int j = 0; j < world.Depth; j++)
            {
                heatMap.Data[i, j] /= iterations;
            }
        }

        return heatMap;
    }

    private List<(int, int)> ConvertToIndices(List<Tile> tiles)
    {
        var indices = new List<(int, int)>();
        foreach (Tile tile in tiles)
        {
            indices.Add((tile.WidthPosition, tile.DepthPosition));
        }
        return indices;
    }

    // Runs the same simulation on the world and returns it
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

        FireSimulation fireSim = new FireSimulation(fireParams, world, specificBurningTiles);
        WindSimulation windSim = new WindSimulation(world);
        manager.AddSimulation(fireSim).AddSimulation(windSim);

        // Run all added simulations, modifying the world's state.
        manager.RunAllSimulations();

        return world;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;

public static class TileExtensions
{
    // Resets non static variables
    public static void Reset(this Tile tile)
    {
        tile.IsBurning = false;
        tile.IsBurned = false;
        tile.BurningFor = 0;
    }

    // Extinguish the fire on this tile and set its state to burned.
    public static void Extinguish(this Tile tile)
    {
        tile.IsBurning = false;
        tile.IsBurned = true;
        tile.BurningFor = 0;
    }

    // Start burning this tile just if it's not already burning or burned.
    public static bool Ignite(this Tile tile)
    {
        if (tile.IsBurning == true || tile.IsBurned == true || tile.IsWater)
        {
            return false;
        }
        tile.IsBurning = true;
        return true;
    }
}

public static class WorldExtensions
{
    // Gets a list of randomly selected non-water tiles
    public static List<Tile> GetRandomInitBurningTiles(this World world)
    {
        List<Tile> burningTiles = new List<Tile>();
        int attempts = 5; // Number of initial points to try to set on fire
        int spreadSteps = 4; // Maximum steps to spread from the initial point

        for (int attempt = 0; attempt < attempts; attempt++)
        {
            // Randomly select a starting tile that is not water
            Tile startTile;
            do
            {
                int x = RandomUtility.NextInt(world.Width);
                int y = RandomUtility.NextInt(world.Depth);
                startTile = world.Grid[x, y];
            }
            while (startTile.IsWater);

            // Try to spread fire from the starting point
            Tile currentTile = startTile;
            for (int step = 0; step < spreadSteps; step++)
            {
                if (!currentTile.IsWater)
                {
                    burningTiles.Add(currentTile);
                }

                // Randomly decide to move
                switch (RandomUtility.NextInt(2))
                {
                    case 0: // Move left
                        if (currentTile.WidthPosition > 0)
                        {
                            currentTile = world.Grid[currentTile.WidthPosition - 1, currentTile.DepthPosition];
                        }
                        break;
                    case 1: // Move up
                        if (currentTile.DepthPosition > 0)
                        {
                            currentTile = world.Grid[currentTile.WidthPosition, currentTile.DepthPosition - 1];
                        }
                        break;
                }
            }
        }

        return burningTiles;
    }

    // Return how many tiles you have to move from one tile to get to the second tile for x, y position.
    public static (int xDiff, int yDiff) GetTilesDistanceXY(Tile tile1, Tile tile2)
    {
        int xDiff = tile1.WidthPosition - tile2.WidthPosition;
        int yDiff = tile1.DepthPosition - tile2.DepthPosition;
        return (xDiff, yDiff);
    }

    // Return the tile at the specified position in the grid.
    public static Tile GetTileAt(this World world, int x, int y)
    {
        if (x < 0 || x >= world.Width || y < 0 || y >= world.Depth)
        {
            throw new ArgumentOutOfRangeException($"Coordinates ({x}, {y}) are out of the grid bounds.");
        }
        return world.Grid[x, y];
    }

    // Returns a list of all neighboring tiles given some tile.
    public static IEnumerable<Tile> GetNeighborTiles(this World world, Tile tile, int distance = 1)
    {
        int x = tile.WidthPosition;
        int y = tile.DepthPosition;

        for (int i = -distance; i <= distance; i++)
        {
            for (int j = -distance; j <= distance; j++)
            {
                int nx = x + i;
                int ny = y + j;

                if (nx != x || ny != y)
                {
                    // Check if the coordinates are valid (within the grid bounds)
                    if (nx >= 0 && nx < world.Width && ny >= 0 && ny < world.Depth)
                    {
                        yield return GetTileAt(world, nx, ny);
                    }
                }
            }
        }
    }

    // Returns a list of neighboring tiles given some tile but only ones that are neighbouring with some edge.
    public static IEnumerable<Tile> GetEdgeNeighborTiles(this World world, Tile tile)
    {
        int x = tile.WidthPosition;
        int y = tile.DepthPosition;

        if (x + 1 < world.Width) // check right boundary
            yield return GetTileAt(world, x + 1, y);

        if (x - 1 >= 0) // check left boundary
            yield return GetTileAt(world, x - 1, y);

        if (y + 1 < world.Depth) // check lower boundary
            yield return GetTileAt(world, x, y + 1);

        if (y - 1 >= 0) // check upper boundary
            yield return GetTileAt(world, x, y - 1);
    }

    // Returns the neighboring tiles of a given tile within a circular boundary defined by a specified radius.
    public static IEnumerable<Tile> GetCircularEdgeNeighborTiles(this World world, Tile tile, int radius)
    {
        int xCenter = tile.WidthPosition;
        int yCenter = tile.DepthPosition;

        double edgeThreshold = 0.5; // A threshold to determine if a tile is close enough to the edge of the circle.

        for (int x = xCenter - radius; x <= xCenter + radius; x++)
        {
            for (int y = yCenter - radius; y <= yCenter + radius; y++)
            {
                double distanceFromCenter = Math.Sqrt((x - xCenter) * (x - xCenter) + (y - yCenter) * (y - yCenter));

                if (Math.Abs(distanceFromCenter - radius) <= edgeThreshold) // tile is close to the edge of the circle
                {
                    // Check if the coordinates are valid (within the grid bounds)
                    if (x >= 0 && x < world.Width && y >= 0 && y < world.Depth)
                    {
                        yield return GetTileAt(world, x, y);
                    }
                }
            }
        }
    }

    // Reset the dynamic properties of simulation - world weather and non static atributes for all the tiles. 
    public static void Reset(this World world)
    {
        world.Wind.Reset();

        foreach (Tile tile in world.Grid)
        {
            tile.Reset();
        }
    }
}
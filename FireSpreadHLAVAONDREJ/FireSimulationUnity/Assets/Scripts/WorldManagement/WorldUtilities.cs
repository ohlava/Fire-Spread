using System;
using System.Collections.Generic;

public static class TileUtilities
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
        if (tile.IsBurning == true || tile.IsBurned == true || tile.Moisture == 100 || tile.IsWater)
        {
            return false;
        }
        tile.IsBurning = true;
        return true;
    }
}

public static class WorldUtilities
{
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

                if (nx != x || ny != y) // not the same position
                {
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

    public static IEnumerable<Tile> GetCircularEdgeNeighborTiles(this World world, Tile tile, int radius)
    {
        int xCenter = tile.WidthPosition;
        int yCenter = tile.DepthPosition;

        // A threshold to determine if a tile is close enough to the edge of the circle.
        // This accounts for the discrete nature of the grid.
        double edgeThreshold = 0.5;

        // Loop through a square grid that approximately covers the area of the circle
        for (int x = xCenter - radius; x <= xCenter + radius; x++)
        {
            for (int y = yCenter - radius; y <= yCenter + radius; y++)
            {
                double distanceFromCenter = Math.Sqrt((x - xCenter) * (x - xCenter) + (y - yCenter) * (y - yCenter));

                // Check if the tile is close to the edge of the circle
                if (Math.Abs(distanceFromCenter - radius) <= edgeThreshold)
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

    // Reset the dynamic properties of simulations - world weather and non static atributes for all the tiles. 
    public static void Reset(this World world)
    {
        // Reset weather to init state
        world.Wind.Reset();

        // Reset every Tile to init state
        foreach (Tile tile in world.Grid)
        {
            tile.Reset();
        }
    }
}
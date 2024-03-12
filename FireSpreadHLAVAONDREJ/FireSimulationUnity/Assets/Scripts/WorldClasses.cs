// Core data structures

using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class World
{
    #region Properties
    public int Width { get; }
    public int Depth { get; }
    public Tile[,] Grid;

    public Weather Weather;
    #endregion

    public World(int width, int depth)
    {
        if (width < 0 || depth < 0)
        {
            throw new ArgumentOutOfRangeException(width < 0 ? nameof(width) : nameof(depth),
                $"{(width < 0 ? "Width" : "Depth")} cannot be less than 0.");
        }
        Width = width;
        Depth = depth;

        Grid = new Tile[Width, Depth];
        Weather = new Weather(0, 15);

        InitializeTiles();
    }

    private void InitializeTiles()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Depth; y++)
            {
                Grid[x, y] = new Tile(0, 0, VegetationType.Grass, x, y);
            }
        }
    }
}

public enum VegetationType
{
    Grass,
    Sparse,
    Forest,
    Swamp
}

public class Tile
{
    #region Properties
    public int WidthPosition { get; private set; } // x position in the world
    public int DepthPosition { get; private set; } // y position in the world

    public float Height { get; set; }
    public int Moisture { get; set; } // number of percent 0-100, 100 is water
    public bool IsWater { get; private set; }
    public VegetationType Vegetation { get; set; }

    public bool IsBurning { get; set; }
    public bool IsBurned { get; set; }
    public int BurnTime { get; set; } // number of episodes required to burn this tile
    public int BurningFor { get; set; } // number of burning episodes - Non static during simulation
    #endregion

    public Tile(float height, int moisture, VegetationType vegetation, int positionX, int positionY)
    {
        WidthPosition = Math.Max(0, positionX);
        DepthPosition = Math.Max(0, positionY);

        Height = height;
        Vegetation = vegetation;
        Moisture = moisture;
        if (moisture == 100)
        {
            IsWater = true;
        }

        if (moisture == 100) // TODO allow not to have to be / lakes same height - 0 and rivers can flow down the hill
        {
            Height = 0.01f;
        }
    }
}


public class Weather
{
    public Weather(int windDirection, float windStrength)
    {
        WindDirection = windDirection;
        WindSpeed = windStrength;
        logger = new WeatherLogger();
    }

    public void Reset()
    {
        WindDirection = UnityEngine.Random.Range(0, 360);
        WindSpeed = UnityEngine.Random.Range(0f, 15f);
        logger = new WeatherLogger();
    }

    private int windDirection; // in degrees, 0-359 where 0 is Unity's +x axis, 90 is +z axis etc.
    private float windSpeed; // in km/h

    public int WindDirection
    {
        get { return windDirection; }
        set
        {
            windDirection = value % 360;
            if (windDirection < 0)
            {
                windDirection += 360;
            }
        }
    }

    public float WindSpeed
    {
        get { return windSpeed; }
        set { windSpeed = Math.Max(0, Math.Min(60, value)); } // ensures it is set to 0-60
    }

    private WeatherLogger logger;

    public void ChangeWindDirection(int newDirection)
    {
        int oldDirection = WindDirection;
        WindDirection = newDirection;

        // Ensure the wind direction stays within 0-359 degrees
        if (WindDirection < 0) newDirection += 360;
        if (WindDirection >= 360) newDirection -= 360;

        // logger.LogChange("WindDirection", oldDirection, WindDirection);
    }

    public void ChangeWindSpeed(float newSpeed)
    {
        float oldSpeed = WindSpeed;

        // Ensure the wind strength is not negative
        WindSpeed = newSpeed;

        // logger.LogChange("WindSpeed", oldSpeed, WindSpeed);
    }
}






public static class TileUtilities
{
    // TODO is specific to some simulation

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

    // Reset the weather and reset all non static atributes for all the tiles. 
    public static void Reset(this World world)
    {
        // Reset weather to init state
        world.Weather.Reset();

        // Reset every Tile to init state
        foreach (Tile tile in world.Grid)
        {
            tile.Reset();
        }
    }

    // TODO manage by or through weather simulation
    public static void UpdateWeather(this World world)
    {
        // Randomly change the wind direction by +/- 15 degrees
        int windDirectionChange = UnityEngine.Random.Range(-15, 15);

        // Randomly change the wind strength by +/- 3 km/h
        float windStrengthChange = UnityEngine.Random.Range(-3f, 3f);

        world.Weather.ChangeWindDirection(world.Weather.WindDirection + windDirectionChange);
        world.Weather.ChangeWindSpeed(world.Weather.WindSpeed + windStrengthChange);
    }
}






[System.Serializable]
public class SerializableWorld
{
    public int Width;
    public int Depth;
    public List<SerializableTile> GridTiles;
}

[System.Serializable]
public class SerializableTile
{
    public int widthPosition;
    public int depthPosition;
    public int moisture;
    public VegetationType Vegetation;
    public float Height;
}

public class SerializableConversion
{
    public static SerializableWorld ConvertToWorldSerializable(World world)
    {
        SerializableWorld serializableWorld = new SerializableWorld
        {
            Width = world.Width,
            Depth = world.Depth,
            GridTiles = new List<SerializableTile>()
        };

        for (int i = 0; i < world.Width; i++)
        {
            for (int j = 0; j < world.Depth; j++)
            {
                serializableWorld.GridTiles.Add(ConvertToTileSerializable(world.Grid[i, j]));
            }
        }
        return serializableWorld;
    }

    public static World ConvertFromWorldSerializable(SerializableWorld serializableWorld)
    {
        World world = new World(serializableWorld.Width, serializableWorld.Depth);
        for (int i = 0; i < serializableWorld.Width; i++)
        {
            for (int j = 0; j < serializableWorld.Depth; j++)
            {
                int index = i * serializableWorld.Depth + j;
                world.Grid[i, j] = ConvertFromTileSerializable(serializableWorld.GridTiles[index]);
            }
        }
        return world;
    }

    public static SerializableTile ConvertToTileSerializable(Tile tile)
    {
        return new SerializableTile
        {
            widthPosition = tile.WidthPosition,
            depthPosition = tile.DepthPosition,
            moisture = tile.Moisture,
            Vegetation = tile.Vegetation,
            Height = tile.Height,
        };
    }

    public static Tile ConvertFromTileSerializable(SerializableTile serializableTile)
    {
        Tile tile = new Tile(
            serializableTile.Height,
            serializableTile.moisture,
            serializableTile.Vegetation,
            serializableTile.widthPosition,
            serializableTile.depthPosition
        );

        return tile;
    }
}
// Core data structures

using System;
using System.Collections.Generic;
using System.IO;
using Palmmedia.ReportGenerator.Core.Logging;
using UnityEngine;

public class World
{
    private int width;
    private int depth;

    public Tile[,] Grid;

    public int Width
    {
        get { return width; }
        set { width = Math.Max(0, value); }
    }

    public int Depth
    {
        get { return depth; }
        set { depth = Math.Max(0, value); }
    }

    public World(int width, int depth)
    {
        Width = width;
        Depth = depth;
        Grid = new Tile[width, depth];
    }

    public void UpdateWeather(Weather newWeather)
    {
        // TODO some alogorithm that updates weather
        return;
    }

    // Return the tile at the specified position in the grid.
    public Tile GetTileAt(int x, int y)
    {
        if (x < 0 || x >= Grid.GetLength(0) || y < 0 || y >= Grid.GetLength(1))
        {
            throw new ArgumentOutOfRangeException($"Coordinates ({x}, {y}) are out of the grid bounds.");
        }
        return Grid[x, y];
    }

    // Returns a list of neighboring tiles given some tile.
    public IEnumerable<Tile> GetNeighborTiles(Tile tile)
    {
        int x = tile.WidthPosition;
        int y = tile.DepthPosition;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int nx = x + i;
                int ny = y + j;

                if (nx != x || ny != y) // not the same
                {
                    if (nx >= 0 && nx < Width && ny >= 0 && ny < Depth)
                    {
                        yield return GetTileAt(nx, ny);
                    }
                }
            }
        }
    }

    // Reset the world, reset all non static atributes for all the tiles. 
    public void Reset()
    {
        foreach (Tile tile in Grid)
        {
            tile.IsBurning = false;
            tile.HasBurned = false;
            tile.BurningFor = 0;
        }
    }

    private static readonly string SAVE_FILE_NAME = "worldSave.json";
    private static readonly string SAVE_FILE_PATH = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    
    public void Save()
    {
        SerializableWorld serializableWorld = SerializableConversion.ConvertToWorldSerializable(this);
        string json = JsonUtility.ToJson(serializableWorld);
        File.WriteAllText(SAVE_FILE_PATH, json);
    }

    public static World Load()
    {
        if (File.Exists(SAVE_FILE_PATH))
        {
            string json = File.ReadAllText(SAVE_FILE_PATH);
            SerializableWorld serializableWorld = JsonUtility.FromJson<SerializableWorld>(json);
            return SerializableConversion.ConvertFromWorldSerializable(serializableWorld);
        }
        else
        {
            throw new FileNotFoundException($"Save file not found at {SAVE_FILE_PATH}");
        }
    }
}

public class Tile
{
    private int widthPosition; // x position in the world
    private int depthPosition; // y position in the world
    private float height;

    private int moisture; // in percents, 100 is water
    public VegetationType Vegetation { get; set; }
    public bool HasBurned { get; set; }
    public Weather Weather;

    private int burnTime; // number of episodes required to burn this tile
    private int burningFor; // number of burning episodes - Non static during simulation
    private bool isBurning;

    public int WidthPosition
    {
        get { return widthPosition; }
        set { widthPosition = Math.Max(0, value); }
    }

    public int DepthPosition
    {
        get { return depthPosition; }
        set { depthPosition = Math.Max(0, value); }
    }

    public float Height
    {
        get { return height; }
        set { height = Math.Max(0, value); }
    }

    public int Moisture
    {
        get { return moisture; }
        set { moisture = Math.Max(0, Math.Min(100, value)); } // ensures it is set to 0-100
    }

    public int BurnTime
    {
        get { return burnTime; }
        set { burnTime = Math.Max(0, value); }
    }

    public int BurningFor
    {
        get { return burningFor; }
        set { burningFor = Math.Max(0, value); }
    }

    public bool IsBurning
    {
        get { return isBurning; }
        set { isBurning = value && !HasBurned; } // Ensure it won't be set if HasBurned is true.
    }

    public Tile(float height, int moisture, VegetationType vegetation, int positionX, int positionY)
    {
        Height = height;
        Moisture = moisture;
        Vegetation = vegetation;
        Weather = new Weather(0, 1);
        isBurning = false;
        BurningFor = 0;
        // TODO adjust rules for BurnTime
        switch (vegetation)
        {
            case VegetationType.Grass:
                BurnTime = 1;
                break;
            case VegetationType.Sparse:
                BurnTime = 2;
                break;
            case VegetationType.Swamp:
                BurnTime = 3;
                break;
            case VegetationType.Forest:
                BurnTime = 4;
                break;
            default:
                Debug.Log("Some vegetation type is not handled.");
                break;
        }
        if (moisture >= 50)
        {
            BurnTime++;
        }
        widthPosition = positionX;
        depthPosition = positionY;
    }

    // Start burning this tile just if it's not already burning or burned.
    public bool Ignite()
    {
        if (IsBurning == true || HasBurned == true || Moisture == 100)
        {
            return false;
        }
        IsBurning = true;
        return true;
    }

    // Extinguish the fire on this tile and set its state to burned.
    public void Extinguish()
    {
        IsBurning = false;
        HasBurned = true;
        BurningFor = 0;
    }
}

public class Weather
{
    private float windDirection { get; set; } // in degrees, 0-359
    private float windStrength; // in km/h

    public float WindDirection
    {
        get { return windDirection; }
        set { windDirection = value < 0 ? 0 : (value >= 360 ? 359 : value); }
    }

    public float WindStrength
    {
        get { return windStrength; }
        set { windStrength = Math.Max(0, Math.Min(100, value)); } // ensures it is set to 0-100
    }

    public Weather(float windDirection, float windStrength)
    {
        WindDirection = windDirection;
        WindStrength = windStrength;
    }
}

public enum VegetationType
{
    Grass,
    Sparse,
    Forest,
    Swamp
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
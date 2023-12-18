// Core data structures

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class World
{
    public World(int width, int depth)
    {
        Width = Math.Max(0, width);
        Depth = Math.Max(0, depth);
        Grid = new Tile[Width, Depth];
        Weather = new Weather(0, 0);
    }

    // Reset the weather and reset all non static atributes for all the tiles. 
    public void Reset()
    {
        // Reset weather to init state
        Weather.Reset();

        // Reset every Tile to init state
        foreach (Tile tile in Grid)
        {
            tile.Reset();
        }
    }

    public int Width { get; }
    public int Depth { get; }

    public Tile[,] Grid;

    // Return how many tiles you have to move from one tile to get to the second tile for x, y position.
    public (int xDiff, int yDiff) GetTilesDistanceXY(Tile tile1, Tile tile2)
    {
        int xDiff = tile1.WidthPosition - tile2.WidthPosition;
        int yDiff = tile1.DepthPosition - tile2.DepthPosition;

        return (xDiff, yDiff);
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

    // Returns a list of all neighboring tiles given some tile.
    public IEnumerable<Tile> GetNeighborTiles(Tile tile, int distance = 1)
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
                    if (nx >= 0 && nx < Grid.GetLength(0) && ny >= 0 && ny < Grid.GetLength(1))
                    {
                        yield return GetTileAt(nx, ny);
                    }
                }
            }
        }
    }

    // Returns a list of neighboring tiles given some tile but only ones that are neighbouring with some edge.
    public IEnumerable<Tile> GetEdgeNeighborTiles(Tile tile)
    {
        int x = tile.WidthPosition;
        int y = tile.DepthPosition;

        if (x + 1 < Grid.GetLength(0)) // check right boundary
            yield return GetTileAt(x + 1, y);

        if (x - 1 >= 0) // check left boundary
            yield return GetTileAt(x - 1, y);

        if (y + 1 < Grid.GetLength(1)) // check lower boundary
            yield return GetTileAt(x, y + 1);

        if (y - 1 >= 0) // check upper boundary
            yield return GetTileAt(x, y - 1);
    }

    public Weather Weather;

    public void UpdateWeather()
    {
        // Randomly change the wind direction by +/- 10 degrees
        int windDirectionChange = UnityEngine.Random.Range(-15, 15);

        // Randomly change the wind strength by +/- 5 km/h
        float windStrengthChange = UnityEngine.Random.Range(-4f, 4f);

        Weather.ChangeWindDirection(Weather.WindDirection + windDirectionChange);
        Weather.ChangeWindSpeed(Weather.WindSpeed + windStrengthChange);
    }

    private static readonly string FILE_NAME = "worldSave.json";
    private static readonly string FILE_PATH = Path.Combine(Application.persistentDataPath, FILE_NAME);
    
    public void Save()
    {
        SerializableWorld serializableWorld = SerializableConversion.ConvertToWorldSerializable(this);
        string json = JsonUtility.ToJson(serializableWorld);
        File.WriteAllText(FILE_PATH, json);
    }

    public static World Load(string fileName = "worldSave.json")
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        if (fileName == "worldSave.json")
        {
            filePath = FILE_PATH;
        }

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            SerializableWorld serializableWorld = JsonUtility.FromJson<SerializableWorld>(json);
            if (serializableWorld is not null)
            {
                // The JSON was valid and deserialized correctly.
                return SerializableConversion.ConvertFromWorldSerializable(serializableWorld);
            }
            else
            {
                // Handle the error, perhaps log an error message or throw an exception.
                Debug.LogError("Invalid JSON format.");
                return null; // Or handle it in another appropriate way.
            }
        }
        else
        {
            throw new FileNotFoundException($"Save file not found at {filePath}");
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
    public Tile(float height, int moisture, VegetationType vegetation, int positionX, int positionY)
    {
        Height = height;
        Moisture = moisture;
        Vegetation = vegetation;

        isBurning = false;
        BurningFor = 0;

        // TODO adjust rules for BurnTime / these rules should be outside interpreted by a firesimulation based on 
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

    // Resets non static variables
    public void Reset()
    {
        IsBurning = false;
        HasBurned = false;
        BurningFor = 0;
    }

    private int widthPosition; // x position in the world
    private int depthPosition; // y position in the world
    private float height;

    private int moisture; // in percents, 100 is water
    public VegetationType Vegetation { get; set; }
    public bool HasBurned { get; set; }

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
    public Weather(int windDirection, float windStrength)
    {
        WindDirection = windDirection;
        WindSpeed = windStrength;
        logger = new WeatherChangeLogger();
    }

    public void Reset()
    {
        WindDirection = UnityEngine.Random.Range(0, 360);
        WindSpeed = UnityEngine.Random.Range(0f, 15f);
        logger = new WeatherChangeLogger();
    }

    private int windDirection; // in degrees, 0-359 where 0 is Unity's +x axis, 90 is +z axis etc.
    private float windSpeed; // in km/h

    //private float temperature { get; set; } // in degrees Celsius
    //private float humidity; // in percentage, 0-100
    //private float precipitation; // in mm

    public int WindDirection
    {
        get { return windDirection; }
        set {
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
        set { windSpeed = Math.Max(0, Math.Min(100, value)); } // ensures it is set to 0-100
    }

    private WeatherChangeLogger logger;

    public void ChangeWindDirection(int newDirection)
    {
        int oldDirection = WindDirection;
        WindDirection = newDirection;

        // Ensure the wind direction stays within 0-359 degrees
        if (WindDirection < 0) newDirection += 360;
        if (WindDirection >= 360) newDirection -= 360;

        logger.LogChange("WindDirection", oldDirection, WindDirection);
    }

    public void ChangeWindSpeed(float newSpeed)
    {
        float oldSpeed = WindSpeed;

        // Ensure the wind strength is not negative
        WindSpeed = Mathf.Max(0f, newSpeed);

        logger.LogChange("WindSpeed", oldSpeed, WindSpeed);
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
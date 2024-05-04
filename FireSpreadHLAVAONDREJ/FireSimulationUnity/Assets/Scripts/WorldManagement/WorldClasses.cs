// Core data structures

using System;

public class World
{
    #region Properties
    public int Width { get; }
    public int Depth { get; }
    public Tile[,] Grid;

    public Wind Wind;
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
        Wind = new Wind();

        InitializeTiles();
    }

    // Deep copy constructor
    public World(World other)
    {
        Width = other.Width;
        Depth = other.Depth;
        Wind = new Wind(other.Wind.InitialWindDirection, other.Wind.InitialWindSpeed);
        Grid = new Tile[Width, Depth];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Depth; y++)
            {
                Tile prevTile = other.Grid[x, y];
                Grid[x, y] = new Tile(prevTile.Height, prevTile.Moisture, prevTile.Vegetation, x, y);
            }
        }
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
    private int moisture; // Percent 0-100, 0 (dry) and 100 (water)
    public int Moisture
    {
        get => moisture;
        set
        {
            moisture = value;
            if (moisture == 100)
            {
                IsWater = true;
            }
            else
            {
                IsWater = false;
            }
        }
    }
    public bool IsWater { get; private set; }
    public VegetationType Vegetation { get; set; }

    public bool IsBurning { get; set; }
    public bool IsBurned { get; set; }
    public int BurnTime { get; set; } // Episodes required to burn this tile
    public int BurningFor { get; set; } // Burning episodes - non static during simulation
    #endregion

    public Tile(float height, int moisture, VegetationType vegetation, int positionX, int positionY)
    {
        WidthPosition = Math.Max(0, positionX);
        DepthPosition = Math.Max(0, positionY);

        Height = height;
        Vegetation = vegetation;
        Moisture = moisture;
    }
}

public class Wind
{
    private int windDirection; // 0-359 degrees, where 0 is Unity's +x axis, 90 is +z axis etc.
    private float windSpeed;
    public readonly int InitialWindDirection;
    public readonly float InitialWindSpeed;

    public int WindDirection
    {
        get => windDirection;
        set => windDirection = ((value % 360) + 360) % 360; // 0-359
    }

    public float WindSpeed
    {
        get => windSpeed;
        set => windSpeed = Math.Clamp(value, 0f, 60f); // 0-60
    }

    public Wind()
    {
        InitialWindDirection = RandomUtility.Range(0, 360); // Random direction 0-359
        InitialWindSpeed = RandomUtility.Range(0.0f, 60.0f); // Random speed 0-60
        Reset();
    }

    public Wind(int initialDirection, float initialSpeed)
    {
        InitialWindDirection = initialDirection;
        InitialWindSpeed = initialSpeed;
        Reset();
    }

    // Resets and sets current wind to initial values.
    public void Reset()
    {
        windDirection = InitialWindDirection;
        windSpeed = InitialWindSpeed;
    }
}
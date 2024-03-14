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
        Wind = new Wind(0, 15);

        InitializeTiles();
    }

    // Deep copy constructor
    public World(World other)
    {
        Width = other.Width;
        Depth = other.Depth;
        Wind = new Wind(other.Wind.WindDirection, other.Wind.WindSpeed);
        Grid = new Tile[Width, Depth];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Depth; y++)
            {
                // Assuming Tile class has a copy constructor
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


public class Wind
{
    private int _windDirection; // in degrees, 0-359 where 0 is Unity's +x axis, 90 is +z axis etc.
    private float _windSpeed; // in km/h

    public int WindDirection
    {
        get => _windDirection;
        set => _windDirection = ((value % 360) + 360) % 360; // Normalize to 0-359
    }

    public float WindSpeed
    {
        get => _windSpeed;
        set => _windSpeed = Math.Clamp(value, 0f, 60f); // Clamp to 0-60
    }

    public Wind(int windDirection, float windStrength)
    {
        WindDirection = windDirection;
        WindSpeed = windStrength;
    }

    public void Reset()
    {
        WindDirection = UnityEngine.Random.Range(0, 360);
        WindSpeed = UnityEngine.Random.Range(0f, 15f);
    }
}
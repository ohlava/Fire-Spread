// Core data structures

using System;
using System.Collections.Generic;
using UnityEngine;

public class World
{
    public int Width { get; set; }
    public int Depth { get; set; }
    public Tile[,] Grid { get; set; }

    // Not yet used
    public Tile HighestTile { get; set; }
    public Weather Weather { get; set; }

    public World(int width, int depth)
    {
        Width = width;
        Depth = depth;
        Grid = new Tile[width, depth];
    }

    public void UpdateWeather(Weather newWeather)
    {
        // Update the weather conditions in the world.
        // TODO add World logger, fire simulation can log in, weather handler will also report any changes in non static weather in the future, Reseting world, clears it. 
    }

    // Return the tile at the specified position in the grid.
    public Tile GetTileAt(int x, int y)
    {
        return Grid[x, y];
    }

    // Returns a list of neighboring tiles given some tile.
    public List<Tile> GetNeighborTiles(Tile tile)
    {
        int x = tile.widthWorldPosition;
        int y = tile.depthWorldPosition;
        List<Tile> neighbours = new List<Tile>();
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
                        neighbours.Add(GetTileAt(nx, ny));
                    }
                }
            }
        }
        return neighbours;
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
}

public class Tile
{
    public int widthWorldPosition { get; set; }
    public int depthWorldPosition { get; set; }

    private int moisture; // in percents, 100 is water
    public int Moisture 
    {
        get { return moisture; }
        set { moisture = Math.Max(0, Math.Min(100, value)); } // ensures it is set to 0-100
    }
    public VegetationType Vegetation { get; set; }
    public float Height { get; set; }
    public int BurnTime { get; set; } // number of episodes required to burn this tile

    // Non static during simulation
    public int BurningFor = 0; // number of burning episodes
    public bool IsBurning { get; set; }
    public bool HasBurned { get; set; }

    public Tile(float height, int moisture, VegetationType vegetation, int positionX, int positionY)
    {
        Height = height;
        Moisture = moisture;
        Vegetation = vegetation;
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
        widthWorldPosition = positionX;
        depthWorldPosition = positionY;
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
    public float WindDirection { get; set; }
    public float WindStrength { get; set; }

    public Weather(float windDirection, float windStrength)
    {
        // TODO initialize the weather conditions, add changable Weather + with logger in World class.
    }
}

public enum VegetationType
{
    Grass,
    Sparse,
    Forest,
    Swamp
}
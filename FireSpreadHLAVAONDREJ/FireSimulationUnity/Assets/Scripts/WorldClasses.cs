// Core data structures
using System;

public class World
{
    public int Width { get; set; }
    public int Depth { get; set; }
    public Tile[,] Grid { get; set; }
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
    }

    // Return the tile at the specified position in the grid.
    public Tile GetTileAt(int x, int y)
    {
        return Grid[x, y];
    }
}

public class Tile
{
    private int moisture; // private backing field
    public int Moisture // 100% is water
    {
        get { return moisture; }
        set { moisture = Math.Max(0, Math.Min(100, value)); } // ensures it is set to 0-100
    }
    public VegetationType Vegetation { get; set; }
    public float Height { get; set; }
    public bool IsBurning { get; set; }
    public bool HasBurned { get; set; }

    public bool Ignite()
    {
        // Start burning this tile if it's not already burning or burned.
        if (IsBurning == true || HasBurned == true || Moisture == 100)
        {
            return false;
        }
        IsBurning = true;
        return true;
    }

    public void Extinguish()
    {
        // Extinguish the fire on this tile and set its state to burned.
        IsBurning = false;
        HasBurned = true;
    }
}

public class Weather
{
    public float WindDirection { get; set; }
    public float WindStrength { get; set; }

    public Weather(float windDirection, float windStrength)
    {
        // Initialize the weather conditions.
    }
}

public enum VegetationType
{
    Grass,
    Sparse,
    Forest,
    Swamp
}
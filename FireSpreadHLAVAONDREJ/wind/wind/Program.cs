using System;

namespace WindTileMap
{
    class Tile
    {
        public float WindSpeed { get; set; }
        public float WindDirection { get; set; } // In degrees, 0 to 359
    }

    class Program
    {
        static Random random = new Random();

        static void Main(string[] args)
        {
            int width = 10;
            int height = 10;

            Tile[,] map = GenerateTileMap(width, height);
            SmoothTileMap(map);

            PrintTileMap(map);
        }

        static Tile[,] GenerateTileMap(int width, int height)
        {
            Tile[,] map = new Tile[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    map[x, y] = new Tile
                    {
                        WindSpeed = (float)random.NextDouble() * 10,
                        WindDirection = random.Next(360)
                    };
                }
            }

            return map;
        }

        static void SmoothTileMap(Tile[,] map)
        {
            int width = map.GetLength(0);
            int height = map.GetLength(1);

            Tile[,] newMap = new Tile[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float totalSpeed = 0;
                    float totalDirection = 0;
                    int count = 0;

                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            int nx = x + i;
                            int ny = y + j;

                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            {
                                totalSpeed += map[nx, ny].WindSpeed;
                                totalDirection += map[nx, ny].WindDirection;
                                count++;
                            }
                        }
                    }

                    newMap[x, y] = new Tile
                    {
                        WindSpeed = totalSpeed / count,
                        WindDirection = totalDirection / count % 360
                    };
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    map[x, y] = newMap[x, y];
                }
            }
        }

        static void PrintTileMap(Tile[,] map)
        {
            int width = map.GetLength(0);
            int height = map.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Console.Write($"({map[x, y].WindSpeed:F1}, {map[x, y].WindDirection:F1}) ");
                }
                Console.WriteLine();
            }
        }
    }
}

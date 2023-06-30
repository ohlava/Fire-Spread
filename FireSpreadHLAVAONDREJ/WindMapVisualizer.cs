using UnityEngine;
using WindTileMap;


public class WindMapVisualizer : MonoBehaviour
{
    public GameObject ArrowPrefab;
    public int Width = 10;
    public int Height = 10;
    public float CellSize = 1;
    public float ArrowSize = 0.1f;

    private Tile[,] map;

    void Start()
    {
        if (ArrowPrefab == null)
        {
            Debug.LogError("ArrowPrefab is not assigned");
            return;
        }

        map = Program.GenerateTileMap(Width, Height);
        Program.SmoothTileMap(map);

        VisualizeMap();
    }

    private void VisualizeMap()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Tile tile = map[x, y];
                Vector3 position = new Vector3(x * CellSize, 0, y * CellSize);
                Quaternion rotation = Quaternion.Euler(0, -tile.WindDirection, 0);
                Vector3 scale = new Vector3(1, 1, tile.WindSpeed * ArrowSize);

                GameObject arrow = Instantiate(ArrowPrefab, position, rotation);
                arrow.transform.localScale = scale;
                arrow.transform.SetParent(transform);
            }
        }
    }
}

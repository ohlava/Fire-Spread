using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum VisualizerMode { Standard, Simplified }

public class Visualizer : MonoBehaviour
{
    public VisualizerMode mode;
    public float TileHeightMultiplier;

    #region Tile Management Fields
    private Dictionary<Tile, GameObject> tileToInstanceDict; // actuall tile instance GameObject created for each Tile
    private Dictionary<Tile, GameObject> tileToVegetationInstanceDict; // vegetation GameObject created on each Tile
    private Dictionary<Tile, GameObject> tileToFireInstanceDict; // fire GameObject created on each Tile
    private List<GameObject> waterChunks; // combined water tiles - chunks
    #endregion

    #region Prefabs and Materials
    [SerializeField] GameObject grassPrefab, forestPrefab, sparsePrefab, swampPrefab;
    [SerializeField] Material grassMaterial, waterMaterial, burnedMaterial, fireMaterial;
    [SerializeField] GameObject tilePrefab, firePrefab;
    #endregion

    // Awake is called when the script instance is being loaded.
    private void Awake()
    {
        InitializeDictionaries();
        InitializeDefaultValues();
    }

    private void InitializeDictionaries()
    {
        tileToInstanceDict = new Dictionary<Tile, GameObject>();
        tileToVegetationInstanceDict = new Dictionary<Tile, GameObject>();
        tileToFireInstanceDict = new Dictionary<Tile, GameObject>();
        waterChunks = new List<GameObject>();
    }

    private void InitializeDefaultValues()
    {
        mode = VisualizerMode.Standard;
        TileHeightMultiplier = 3.0f;
    }

    // Function to apply heatmap to world tiles
    public void ApplyHeatMapToWorld(Map<float> heatMap, World world)
    {
        if (heatMap.Width != world.Width || heatMap.Depth != world.Depth)
        {
            Debug.LogError("Heatmap dimensions do not match world dimensions.");
            return;
        }

        Color minColor = Color.white; // White
        Color maxColor = new Color(0.5f, 0, 0); // Dark Red

        for (int x = 0; x < heatMap.Width; x++)
        {
            for (int y = 0; y < heatMap.Depth; y++)
            {
                Tile tile = world.GetTileAt(x, y);
                GameObject tileInstance = GetTileInstance(tile);
                if (tileInstance != null)
                {
                    // Interpolate color based on heat value
                    float heatValue = heatMap.Data[x, y];
                    Color tileColor = Color.Lerp(minColor, maxColor, heatValue);
                    tileInstance.SetColorTo(tileColor);
                }
            }
        }
    }

    // Main function for creating the visualizer world representation.
    public void CreateWorldTiles(World world)
    {
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Depth; y++)
            {
                Tile worldTile = world.GetTileAt(x, y);
                float height = worldTile.Height;
                if (worldTile.IsWater)
                {
                    height = 0.01f;
                }

                GameObject tileInstance = Instantiate(tilePrefab, new Vector3(x, height, y), Quaternion.identity);

                tileInstance.transform.localScale = new Vector3(1, (float) (height * TileHeightMultiplier + 1), 1);
                tileInstance.transform.position = new Vector3(x, tileInstance.transform.localScale.y / 2, y);

                tileToInstanceDict[worldTile] = tileInstance;

                SetAppropriateMaterial(worldTile);
            }
        }

        CombineAllWaterTiles(world);
    }

    // Transforms all water tiles into list of bigger chunks - waterChunks. Combining all water tiles into one improves speed and performance.
    private void CombineAllWaterTiles(World world)
    {
        List<List<GameObject>> groups = FindContiguousWaterTileGroups(world);

        foreach (List<GameObject> group in groups)
        {
            GameObject combinedWater = CombineWaterTilesOfGroup(group, world);
            waterChunks.Add(combinedWater);
        }
    }

    // Finds and groups contiguous water tiles in the world and return a list of tile groups, where each group contains GameObjects of contiguous water tiles.
    private List<List<GameObject>> FindContiguousWaterTileGroups(World world)
    {
        // List to store groups of contiguous water tiles
        List<List<GameObject>> groups = new List<List<GameObject>>();

        // Set to keep track of tiles that have already been visited
        HashSet<Tile> visitedTiles = new HashSet<Tile>();

        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Depth; y++)
            {
                Tile tile = world.GetTileAt(x, y);

                // Skip the tile if it has already been visited or if it's not a water tile
                if (visitedTiles.Contains(tile) || !tile.IsWater)
                    continue;

                List<GameObject> group = new List<GameObject>();
                Queue<Tile> queue = new Queue<Tile>();
                queue.Enqueue(tile);

                // BFS to find all tiles in the current contiguous group
                while (queue.Count > 0)
                {
                    Tile currentTile = queue.Dequeue();
                    if (visitedTiles.Contains(currentTile))
                        continue;

                    visitedTiles.Add(currentTile);
                    group.Add(tileToInstanceDict[currentTile]);

                    foreach (Tile neighborTile in world.GetEdgeNeighborTiles(currentTile))
                    {
                        if (neighborTile.IsWater && !visitedTiles.Contains(neighborTile))
                        {
                            queue.Enqueue(neighborTile);
                        }
                    }
                }

                if (group.Count > 0)
                    groups.Add(group);
            }
        }

        return groups;
    }

    // Combines multiple contiguous water tiles into a single simplified mesh for optimization, returning a new GameObject with a combined mesh representing the group of water tiles.
    private GameObject CombineWaterTilesOfGroup(List<GameObject> tiles, World world)
    {
        // Lists to store the combined mesh's vertices and triangles
        List<Vector3> combinedVertices = new List<Vector3>();
        List<int> combinedTriangles = new List<int>();

        foreach (GameObject tileGO in tiles)
        {
            Mesh tileMesh = tileGO.GetComponent<MeshFilter>().mesh;

            Vector3[] vertices = tileMesh.vertices;
            int[] triangles = tileMesh.triangles;

            // Transform local vertices to world space coordinates
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = tileGO.transform.TransformPoint(vertices[i]);
            }

            // Retrieve the Tile instance corresponding to the current GameObject
            Tile tile = tileToInstanceDict.FirstOrDefault(kvp => kvp.Value == tileGO).Key;

            IEnumerable<Tile> neighbors = world.GetEdgeNeighborTiles(tile);

            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                bool skipFace = false;

                // Get the normal direction of the current face
                Vector3 faceNormal = tileGO.transform.TransformDirection(TileFaceNormals(faceIndex));

                if (faceNormal != Vector3.up && faceNormal != Vector3.down)
                {
                    foreach (Tile neighbor in neighbors)
                    {
                        if (tiles.Contains(tileToInstanceDict[neighbor])) // check if it is also water tile instance
                        {
                            // Check if the current face is facing the neighbor directly, that means the face is internal and should be skipped
                            if (faceNormal == Vector3.left && WorldExtensions.GetTilesDistanceXY(tile, neighbor) == (1, 0) ||
                                faceNormal == Vector3.right && WorldExtensions.GetTilesDistanceXY(tile, neighbor) == (-1, 0) ||
                                faceNormal == Vector3.forward && WorldExtensions.GetTilesDistanceXY(tile, neighbor) == (0, 1) ||
                                faceNormal == Vector3.back && WorldExtensions.GetTilesDistanceXY(tile, neighbor) == (0, -1))
                            {
                                skipFace = true;
                                break;
                            }
                        }
                    }
                }

                // If the face is not to be skipped, add its triangles to the combinedTriangles list
                if (!skipFace)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        combinedTriangles.Add(combinedVertices.Count + triangles[faceIndex * 6 + i]);
                    }
                }
            }

            // Add the vertices of the current tile to the combinedVertices list
            combinedVertices.AddRange(vertices);

            // Destroy the original tile GameObject as it's no longer needed
            Destroy(tileGO);
        }

        // Create a new mesh and assign the combined vertices and triangles
        Mesh combinedMesh = new Mesh();
        combinedMesh.vertices = combinedVertices.ToArray();
        combinedMesh.triangles = combinedTriangles.ToArray();
        combinedMesh.RecalculateNormals();

        // Create a new GameObject to hold the combined mesh
        GameObject combinedGO = new GameObject("CombinedWaterTiles");
        combinedGO.AddComponent<MeshFilter>().mesh = combinedMesh;
        combinedGO.AddComponent<MeshRenderer>().material = waterMaterial;

        return combinedGO;
    }

    // Utility function to return the normal of a face given its index
    private Vector3 TileFaceNormals(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0: return Vector3.back;
            case 1: return Vector3.up;
            case 2: return Vector3.forward;
            case 3: return Vector3.down;
            case 4: return Vector3.left;
            case 5: return Vector3.right;
            default:
                Debug.LogError($"Invalid face index: {faceIndex}");
                return Vector3.zero;
        }
    }

    // Returns a specific tileInstance GameObject representing that tile
    public GameObject GetTileInstance(Tile tile)
    {
        if (tileToInstanceDict.TryGetValue(tile, out GameObject tileInstance))
        {
            return tileInstance;
        }
        else
        {
            Debug.LogError("No created instance found for the given tile. It doesn't exist anymore in visualizer data.");
            return null;
        }
    }

    // Sets tile material based on its properties or state. Tile instance should already be in the tileToInstanceDict
    public void SetAppropriateMaterial(Tile tile)
    {
        GameObject tileInstance = GetTileInstance(tile);
        if (tileInstance == null)
        {
            return;  // Exit if tileInstance is null to avoid errors
        }

        int maxVegetationType = Enum.GetNames(typeof(VegetationType)).Length;

        if (tile.IsWater)
        {
            tileInstance.SetMaterialTo(waterMaterial);
        }
        else if (tile.IsBurning)
        {
            tileInstance.SetMaterialTo(fireMaterial);
        }
        else if (tile.IsBurned)
        {
            tileInstance.SetMaterialTo(burnedMaterial);
        }
        else // Set tile color based on vegetation level
        {
            float greenValue = 0.4f + (0.2f / (maxVegetationType - 1)) * (int)tile.Vegetation;
            tileInstance.SetColorTo(new Color(0, greenValue, 0));  // RGB color with variable green value/shade
        }
    }

    public void MakeTileBurned(Tile tile)
    {
        GetTileInstance(tile).SetMaterialTo(burnedMaterial);
    }

    // Generate all vegetationInstances - for the standard mode
    public void CreateAllVegetation(World world)
    {
        if (mode == VisualizerMode.Standard)
        {
            foreach (var tile in world.Grid)
            {
                if (!tile.IsWater)
                {
                    CreateVegetationOnTile(tile, tile.Vegetation);
                }
            }
        }
    }

    // Method to create a vegetation instance on a specific tile
    private void CreateVegetationOnTile(Tile tile, VegetationType vegetation)
    {
        GameObject tileInstance = tileToInstanceDict[tile];

        Vector3 tilePosition = tileInstance.GetPosition();
        float tileHeight = tileInstance.GetHeight();

        GameObject chosenPrefab = null;

        switch (vegetation)
        {
            case VegetationType.Grass:
                chosenPrefab = grassPrefab;
                break;
            case VegetationType.Forest:
                chosenPrefab = forestPrefab;
                break;
            case VegetationType.Sparse:
                chosenPrefab = sparsePrefab;
                break;
            case VegetationType.Swamp:
                chosenPrefab = swampPrefab;
                break;
            default:
                Debug.LogError("Unkwown vegetationType, add corresponding prefab.");
                break;
        }

        if (chosenPrefab != null)
        {
            // Create a new vegetation at the top of the tile
            GameObject vegetationInstance = Instantiate(chosenPrefab, tilePosition + new Vector3(0, tileHeight / 2, 0), Quaternion.identity);

            // Make a random rotation for that vegetation
            vegetationInstance.transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

            tileToVegetationInstanceDict[tile] = vegetationInstance;
        }
    }

    // Method to destroy a vegetation instance on a specific tile
    public void DestroyVegetationOnTile(Tile tile)
    {
        if (mode == VisualizerMode.Standard)
        {
            // Check that there is something to destroy
            if (tileToVegetationInstanceDict.TryGetValue(tile, out GameObject instance))
            {
                Destroy(instance);
                tileToVegetationInstanceDict.Remove(tile);
            }
            else
            {
                Debug.LogError("No created instance found for the given tile. It doesnt exist anymore in vizulizer data. VEGETATION");
            }
        }
    }

    // Method to create a fire instance on a specific tile
    public void CreateFireOnTile(Tile tile)
    {
        GameObject tileInstance = tileToInstanceDict[tile];

        tileInstance.SetMaterialTo(fireMaterial);

        // In addition if operating in standard settings add nicer fire animation on top of this tile
        if (mode == VisualizerMode.Standard)
        {
            Vector3 tilePosition = tileInstance.GetPosition();
            float tileHeight = tileInstance.GetHeight();

            // Create a new fire instance at the top of the tile
            GameObject fireInstance = Instantiate(firePrefab, tilePosition + new Vector3(0, tileHeight / 2, 0), Quaternion.Euler(-90, 0, 0));

            tileToFireInstanceDict[tile] = fireInstance;
        }
    }

    // Method to destroy a fire instance on a specific tile
    public void DestroyFireOnTile(Tile tile)
    {
        if (mode == VisualizerMode.Standard)
        {
            // Check that there is something to destroy
            if (tileToFireInstanceDict.TryGetValue(tile, out GameObject instance))
            {
                Destroy(instance);
                tileToFireInstanceDict.Remove(tile);
            }
            else
            {
                Debug.LogError("No created instance found for the given tile. It doesnt exist anymore in vizulizer data. FIRE");
            }
        }
    }

    //
    // Destruction Methods
    //

    // Method to destroy all created vegetation instances
    public void DestroyAllVegetation()
    {
        foreach (GameObject instance in tileToVegetationInstanceDict.Values)
        {
            Destroy(instance);
        }
        tileToVegetationInstanceDict.Clear(); 
    }

    // Method to destroy all created fire instances
    public void DestroyAllFire()
    {
        foreach (GameObject instance in tileToFireInstanceDict.Values)
        {
            Destroy(instance);
        }
        tileToFireInstanceDict.Clear(); 
    }

    // Method to destroy all created tile instances
    public void DestroyAllTile()
    {
        foreach (GameObject instance in tileToInstanceDict.Values)
        {
            Destroy(instance);
        }
        tileToInstanceDict.Clear();

        // Also destroy water mesh - chunks of tiles merged together
        foreach (GameObject chunk in waterChunks)
        {
            Destroy(chunk);
        }
        waterChunks.Clear();
    }

    // for clicking on some tile instance = to see which we clicked in combination with RayCast
    public Tile GetWorldTileFromInstance(GameObject instance)
    {
        foreach (Tile tile in tileToInstanceDict.Keys)
        {
            if (tileToInstanceDict[tile] == instance)
            {
                return tile;
            }
        }
        // Debug.LogError("No Tile found for the given instance.");
        return null;
    }
}
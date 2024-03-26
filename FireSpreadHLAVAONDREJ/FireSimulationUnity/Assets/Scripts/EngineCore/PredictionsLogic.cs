using System.Collections.Generic;
using UnityEngine;

public enum PredictionState { NewWorldState, Prediction }

public class PredictionLogic : MonoBehaviour
{
    [SerializeField] private GameObject inputHandlerObj, cameraHandlerObj, visualizerObj, uiManagerObj;

    private List<Tile> initBurningTiles;
    private World world;
    private WorldGenerator worldGenerator;

    private CameraHandler cameraHandler;
    private Tile currentlyHoveredTile;
    private Visualizer visualizer;
    private PredictionState currentState;
    private UIManager uiManager;

    private InputHandler inputHandler;

    // Awake is called when the script instance is being loaded.
    void Awake()
    {
        InitializeComponents();
        SubscribeToInputEvents();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetConstantProperties();
        GenereteNewWorld();
    }

    private void InitializeComponents()
    {
        initBurningTiles = new List<Tile>();
        worldGenerator = new WorldGenerator();
        inputHandler = inputHandlerObj.GetComponent<InputHandler>();
        cameraHandler = cameraHandlerObj.GetComponent<CameraHandler>();
        visualizer = visualizerObj.GetComponent<Visualizer>();
        uiManager = uiManagerObj.GetComponent<UIManager>();
    }

    private void SubscribeToInputEvents()
    {
        inputHandler.OnGenerateWorld += GenereteNewWorld;
        inputHandler.OnGenerateData += GenerateData;
        inputHandler.OnPythonPredict += PythonPredict;
        inputHandler.OnHeatMap += HeatMap;
        inputHandler.OnReset += Reset;

        inputHandler.OnTileClicked += HandleTileClick;
        inputHandler.OnTileHovered += HandleTileHover;
    }

    private void SetConstantProperties()
    {
        worldGenerator.width = 30;
        worldGenerator.depth = 30;
        worldGenerator.rivers = 3;
        worldGenerator.lakeThreshold = 0.1f;

        visualizer.mode = VisualizerMode.Simplified;
    }


    public void GenereteNewWorld()
    {
        world = worldGenerator.Generate();

        PrepareForNewWorld();
    }

    private void PrepareForNewWorld()
    {
        currentState = PredictionState.NewWorldState;
        uiManager.UpdateInfoPanel("New world - set fire");
        
        initBurningTiles.Clear();

        VisulizerRemakeAll();
        cameraHandler.SetWorldCenter(new Vector3(world.Width / 2f, 2f * visualizer.TileHeightMultiplier, world.Depth / 2f));
        cameraHandler.RotateCamera(); // Set to default position

        currentlyHoveredTile = null;
    }

    private void VisulizerRemakeAll()
    {
        visualizer.DestroyAllTile();
        visualizer.DestroyAllVegetation();
        visualizer.DestroyAllFire();

        visualizer.CreateWorldTiles(world);
        visualizer.CreateAllVegetation(world);
    }


    // Responds to clicks on tiles, igniting them if the simulation is in the appropriate state.
    private void HandleTileClick(Tile clickedTile)
    {
        // We can click on tiles only when simulation is not running
        if (currentState == PredictionState.NewWorldState)
        {
            if (clickedTile.Ignite())
            {
                initBurningTiles.Add(clickedTile);
                visualizer.CreateFireOnTile(clickedTile);
            }
        }
    }

    // Updates the visual state of a tile when it is hovered over.
    private void HandleTileHover(bool hovered, Tile hoveredOverTile)
    {
        ResetHoveredTileColor(); // Reset the old tile's color

        if (hovered == true && currentState == PredictionState.NewWorldState && currentlyHoveredTile != hoveredOverTile)
        {
            currentlyHoveredTile = hoveredOverTile;
            GameObject tileInstance = visualizer.GetTileInstance(hoveredOverTile);
            if (tileInstance is not null)
            {
                Renderer renderer = tileInstance.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.white;
                }
            }
        }
    }

    // Resets the visual state of the previously hovered tile.
    private void ResetHoveredTileColor()
    {
        if (currentlyHoveredTile is not null)
        {
            GameObject tileInstance = visualizer.GetTileInstance(currentlyHoveredTile);
            if (tileInstance != null)
            {
                // set color back to its original color
                visualizer.SetAppropriateMaterial(currentlyHoveredTile);
            }
            currentlyHoveredTile = null;
        }
    }

    // Resets the world to its initial state.
    public void Reset()
    {
        world.Reset();
        PrepareForNewWorld();
    }


    public void GenerateData()
    {
        Debug.Log("GenerateData called");
        return;
    }

    public void PythonPredict()
    {
        Debug.Log("PythonPredict called");
        return;
    }

    public void HeatMap()
    {
        FireSimParameters fireSimParameters = new FireSimParameters(); // TODO: default for now
        FirePredictor firePredictor = new FirePredictor(fireSimParameters);

        Map<float> heatMap = firePredictor.GenerateHeatMap(30, world, initBurningTiles);

        visualizer.ApplyHeatMapToWorld(heatMap, world);
        return;
    }
}

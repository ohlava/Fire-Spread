using System.Collections.Generic;
using System.Threading.Tasks;
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
    private FileBrowserHandler fileBrowserHandler;
    private FileManagementService fileManagementService;
    private bool canInteract;

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
        GenerateNewWorld();
    }

    private void InitializeComponents()
    {
        canInteract = true;
        initBurningTiles = new List<Tile>();
        worldGenerator = new WorldGenerator();
        inputHandler = inputHandlerObj.GetComponent<InputHandler>();
        cameraHandler = cameraHandlerObj.GetComponent<CameraHandler>();
        visualizer = visualizerObj.GetComponent<Visualizer>();
        uiManager = uiManagerObj.GetComponent<UIManager>();
        fileBrowserHandler = FindObjectOfType<FileBrowserHandler>();

        fileManagementService = new FileManagementService(fileBrowserHandler, new WorldFileManager(), new HeightMapImporter(), worldGenerator, inputHandler);
    }

    private void SubscribeToInputEvents()
    {
        inputHandler.OnGenerateWorld += GenerateNewWorld;
        inputHandler.OnGenerateData += GenerateData;
        inputHandler.OnPythonPredict += PythonPredict;
        inputHandler.OnHeatMap += HeatMap;
        inputHandler.OnReset += Reset;
        inputHandler.OnImport += OnImportClicked;
        inputHandler.OnSave += OnSaveClicked;

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


    public void GenerateNewWorld()
    {
        if (!canInteract)
        {
            uiManager.UpdateInfoPanel("Wait for other task to complete");
            Debug.Log("You can't generate new world while waiting for result from previous interaction");
            return;
        }

        world = worldGenerator.Generate();

        PrepareForNewWorld();
    }

    private void PrepareForNewWorld()
    {
        currentState = PredictionState.NewWorldState;
        uiManager.UpdateInfoPanel("New world - set some tiles on fire");
        
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
        if (canInteract && currentState == PredictionState.NewWorldState)
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
        if (!canInteract) return;

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
        if (!canInteract)
        {
            uiManager.UpdateInfoPanel("Wait for other task to complete");
            Debug.Log("You can't reset the world while waiting for result from previous interaction");
            return;
        }

        world.Reset();
        PrepareForNewWorld();
    }

    // Handles the import button click event to load external maps or serialized worlds.
    public void OnImportClicked()
    {
        if (!canInteract)
        {
            uiManager.UpdateInfoPanel("Wait for other task to complete");
            Debug.Log("You can't import new world while waiting for result from previous interaction");
            return;
        }

        fileManagementService.ImportFile(OnWorldImported);
    }

    private void OnWorldImported(World world)
    {
        this.world = world;
        PrepareForNewWorld();
    }

    public void OnSaveClicked()
    {
        if (!canInteract)
        {
            uiManager.UpdateInfoPanel("Wait for other task to complete");
            Debug.Log("You can't save the world while waiting for result from previous interaction");
            return;
        }

        fileManagementService.SaveWorld(world);
    }


    public void GenerateData()
    {
        if (!canInteract)
        {
            uiManager.UpdateInfoPanel("Wait for other task to complete");
            Debug.Log("You can't generate new while waiting for result from previous interaction");
            return;
        }

        Debug.Log("GenerateData called");
        return;
    }

    public void PythonPredict()
    {
        if (!canInteract)
        {
            uiManager.UpdateInfoPanel("Wait for other task to complete");
            Debug.Log("You can't call python for prediction while waiting for result from previous interaction");
            return;
        }

        Debug.Log("PythonPredict called");
        return;
    }

    public async void HeatMap()
    {
        if (initBurningTiles.Count == 0)
        {
            uiManager.UpdateInfoPanel("Set some tiles \n on fire first");
            return;
        }

        if (!canInteract)
        {
            uiManager.UpdateInfoPanel("Wait for other task to complete");
            Debug.Log("You can't call for heat map while waiting for result from previous interaction");
            return;
        }

        DisableInteractions();

        FireSimParameters fireSimParameters = new FireSimParameters(inputHandler.VegetationFactor, inputHandler.MoistureFactor, false, inputHandler.SlopeFactor, inputHandler.SpreadProbability);
        FirePredictor firePredictor = new FirePredictor(fireSimParameters);
        var heatMap = await Task.Run(() => firePredictor.GenerateHeatMap(inputHandler.HeatMapIterations, world, initBurningTiles));

        uiManager.UpdateInfoPanel($"Heat map prediction with {inputHandler.HeatMapIterations} runned simulations");
        currentState = PredictionState.Prediction;

        visualizer.ApplyHeatMapToWorld(heatMap, world); // Can only be called in main thread, no await

        EnableInteractions();
        return;
    }

    private void DisableInteractions()
    {
        canInteract = false;
    }

    private void EnableInteractions()
    {
        canInteract = true;
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private PythonCaller pythonCaller;
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
        pythonCaller = new PythonCaller();
        inputHandler = inputHandlerObj.GetComponent<InputHandler>();
        cameraHandler = cameraHandlerObj.GetComponent<CameraHandler>();
        visualizer = visualizerObj.GetComponent<Visualizer>();
        uiManager = uiManagerObj.GetComponent<UIManager>();
        fileBrowserHandler = FindObjectOfType<FileBrowserHandler>();

        fileManagementService = new FileManagementService(fileBrowserHandler, worldGenerator);
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
        worldGenerator.width = inputHandler.WorldWidth;
        worldGenerator.depth = inputHandler.WorldDepth;
        worldGenerator.rivers = inputHandler.Rivers;
        worldGenerator.lakeThreshold = inputHandler.LakeThreshold;

        visualizer.mode = VisualizerMode.Simplified;
    }


    public void GenerateNewWorld()
    {
        if (!canInteract)
        {
            uiManager.UpdateInfoPanel("Wait for previous task to complete");
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
        cameraHandler.SetCamera(world.Width, world.Depth);

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
            if (tileInstance != null)
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
        if (currentlyHoveredTile != null)
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
            uiManager.UpdateInfoPanel("Wait for previous task to complete");
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
            uiManager.UpdateInfoPanel("Wait for previous task to complete");
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
            uiManager.UpdateInfoPanel("Wait for previous task to complete");
            Debug.Log("You can't save the world while waiting for result from previous interaction");
            return;
        }

        fileManagementService.SaveWorld(world);
    }

    public async void GenerateData()
    {
        if (!canInteract)
        {
            uiManager.UpdateInfoPanel("Wait for previous task to complete");
            Debug.Log("You can't generate new while waiting for result from previous interaction");
            return;
        }

        DisableInteractions();

        uiManager.UpdateInfoPanel("Generating worlds and running simulations on them...");

        FireSimParameters fireSimParameters = new FireSimParameters(inputHandler.VegetationFactor, inputHandler.MoistureFactor, false, inputHandler.SlopeFactor, inputHandler.SpreadProbability);
        FirePredictor firePredictor = new FirePredictor(fireSimParameters);
        string filePath = Path.Join(Application.streamingAssetsPath, "PythonScripts/datafile.json");

        try
        {
            int numberOfWorlds = inputHandler.GenerateDataAmount; // Number of random worlds to generate
            int simulationIterations = 10; // Number of simulations to run per world
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < numberOfWorlds; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    World world = worldGenerator.Generate(); // Generate a random world
                    List<Tile> initBurningTiles = world.GetRandomInitBurningTiles(); // Generate initial burning tiles
                    Map<float> heatMap = await Task.Run(() => firePredictor.GenerateHeatMap(simulationIterations, world, initBurningTiles));

                    WorldFileManager.AppendSimulationDataToFile(world, heatMap, filePath); // Serialize and append the data for this world
                }));
            }

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during data generation: {ex.Message}");
        }

        uiManager.UpdateInfoPanel($"Done, generation complete: {filePath}");
        EnableInteractions();
        return;
    }

    public async void PythonPredict()
    {
        if (!canInteract)
        {
            uiManager.UpdateInfoPanel("Wait for previous task to complete");
            Debug.Log("You can't call python for prediction while waiting for result from previous interaction");
            return;
        }

        DisableInteractions();

        InputDataSerializationPackage inputData = SerializableConversion.ConvertToInputDataSerializationPackage(world, initBurningTiles);
        string output = await pythonCaller.CallPythonScript(inputData);

        if (string.IsNullOrEmpty(output))
        {
            uiManager.UpdateInfoPanel("Python script failed");
            Debug.LogError("Python script did not return a valid result.");
            EnableInteractions();
            return;
        }

        OutputData outputData;
        try
        {
            outputData = JsonUtility.FromJson<OutputData>(output);
        }
        catch (Exception ex)
        {
            uiManager.UpdateInfoPanel("Error parsing Python script output");
            Debug.LogError($"Error parsing Python output: {ex.Message}");
            EnableInteractions();
            return;
        }

        Map<float> predictedMap;
        try
        {
            predictedMap = SerializableConversion.ConvertToMap(outputData);
        }
        catch (Exception ex)
        {
            uiManager.UpdateInfoPanel("Error converting Python script output to map.");
            Debug.LogError($"Error converting Python output to map: {ex.Message}");
            EnableInteractions();
            return;
        }

        uiManager.UpdateInfoPanel($"Heat map prediction with Python script");
        currentState = PredictionState.Prediction;

        visualizer.ApplyHeatMapToWorld(predictedMap, world);

        EnableInteractions();
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
            uiManager.UpdateInfoPanel("Wait for previous task to complete");
            Debug.Log("You can't call for heat map while waiting for result from previous interaction");
            return;
        }

        DisableInteractions();

        FireSimParameters fireSimParameters = new FireSimParameters(inputHandler.VegetationFactor, inputHandler.MoistureFactor, false, inputHandler.SlopeFactor, inputHandler.SpreadProbability);
        FirePredictor firePredictor = new FirePredictor(fireSimParameters);
        Map<float> heatMap = await Task.Run(() => firePredictor.GenerateHeatMap(inputHandler.HeatMapIterations, world, initBurningTiles));

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
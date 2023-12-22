using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.UI;

public enum State { NewWorldState, RunningState, StoppedState }

public class MainLogic : MonoBehaviour
{
    public World world;
    private WorldGenerator worldGenerator;
    public WorldFileManager worldFileManager;

    FileBrowserHandler fileBrowserHandler;

    FireSpreadParameters fireSpreadParams;
    FireSpreadSimulation fireSpreadSimulation;
    List<Tile> initBurningTiles;

    Visulizer visulizer;
    [SerializeField] GameObject visulizerObj;

    InputHandler inputHandler;
    [SerializeField] GameObject inputHandlerObj;

    [SerializeField] private Button runButton;
    [SerializeField] private Button pauseButton;

    WindIndicator windIndicator;
    [SerializeField] GameObject windIndicatorObj;

    GraphVisulizer graphVisulizer;

    [SerializeField] TextMeshProUGUI InfoPanel;

    private Tile currentlyHoveredTile;

    private float elapsed;
    private float speedOfUpdates;

    private bool showingGraph;
    private State currentState;

    void Awake()
    {
        worldGenerator = new WorldGenerator();
        worldFileManager = new WorldFileManager();
        fileBrowserHandler = FindObjectOfType<FileBrowserHandler>();

        fireSpreadParams = new FireSpreadParameters();
        fireSpreadSimulation = null;
        initBurningTiles = new List<Tile>();

        currentlyHoveredTile = null;

        elapsed = 0f;
        speedOfUpdates = 1.2f; // seconds

        showingGraph = false;
        currentState = State.NewWorldState;

        visulizer = visulizerObj.GetComponent<Visulizer>();

        windIndicator = windIndicatorObj.GetComponent<WindIndicator>();

        // GraphVisulizer object is attached to a main camera, this finds it, there is only one graphVisualizer
        graphVisulizer = FindObjectOfType<GraphVisulizer>();

        inputHandler = inputHandlerObj.GetComponent<InputHandler>();
        inputHandler.OnTileClicked += HandleTileClick;
        inputHandler.OnTileHovered += HandleTileHover;
        inputHandler.OnCameraMove += HandleCameraMove;
        inputHandler.OnCameraAngleChange += HandleCameraAngleChange;
        inputHandler.OnGraph += OnGraphButtonClicked;
        inputHandler.OnReset += OnResetButtonClicked;
        inputHandler.OnGenerateWorld += GenereteNewWorld;
        inputHandler.OnFieldValueChange += ApplyInputValues; // plus GenereteNewWorld with that - set in Unity
        inputHandler.OnImport += OnImportClicked;
        inputHandler.OnSave += OnSaveClicked;
        inputHandler.OnRun += OnRunButtonClicked;
        inputHandler.OnPause += OnPauseButtonClicked;
        inputHandler.onSimulationSpeedChange += SetSimulationSpeed;
    }

    // Start is called before the first frame update
    void Start()
    {
        GenereteNewWorld();
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += Time.deltaTime;
        if (ShouldPerformUpdate())
        {
            PerformUpdate();
        }
    }

    // Determines whether an update should be performed based on elapsed time and current state.
    bool ShouldPerformUpdate()
    {
        return elapsed >= speedOfUpdates && currentState == State.RunningState;
    }

    // Carries out simulation updates and visual refreshes.
    void PerformUpdate()
    {
        elapsed = elapsed % speedOfUpdates;
        UpdateWorld();
        world.UpdateWeather();
        UpdateWindIndicator();
        UpdateGraph();
    }

    // Updates the display of the wind indicator based on current weather conditions.
    private void UpdateWindIndicator()
    {
        if (windIndicator is null) return;

        windIndicator.UpdateIndicator(world.Weather);

        return;
    }

    // Updates the camera position for the wind indicator.
    private void UpdateWindCamera()
    {
        if (windIndicator is null) return;

        windIndicator.UpdateCamera();

        return;
    }

    // Updates the simulation state of the world and handles fire spread events.
    private void UpdateWorld()
    {
        if (fireSpreadSimulation is null) return;

        // Run and then automatically stop running after simulation finishes
        if (!fireSpreadSimulation.Finished())
        {
            fireSpreadSimulation.Update();

            // Get the events from the last update
            List<FireEvent> events = fireSpreadSimulation.GetLastUpdateEvents();

            // Handle these events, for example by visualizing them
            foreach (FireEvent evt in events)
            {
                if (evt.Type == EventType.TileStartedBurning)
                {
                    visulizer.CreateFireOnTile(evt.Tile);
                }
                else if (evt.Type == EventType.TileStoppedBurning)
                {
                    visulizer.DestroyFireOnTile(evt.Tile);
                    visulizer.DestroyVegetationOnTile(evt.Tile);
                    visulizer.MakeTileBurned(evt.Tile);
                }
            }
        }
        else
        {
            currentState = State.StoppedState;
            InfoPanel.text = "Simulation paused";
        }
    }

    // Handles changes in camera zoom level.
    private void HandleCameraMove(float zoomChange)
    {
        float minZoom = 5f;
        float maxZoom = 50f;
        float zoomChangeSpeed = 0.05f;

        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + zoomChange * zoomChangeSpeed, minZoom, maxZoom);
    }

    // Adjusts the camera's rotation based on user input.
    private void HandleCameraAngleChange(Vector3 rotationChange)
    {
        Vector3 worldCenter = new Vector3(world.Width / 2f, 2f * visulizer.TileHeightMultiplier, world.Depth / 2f);
        float upDownSpeed = 50f;
        float speed = 50f;
        float cameraDistance = 100f;

        // Calculate new rotation angles
        Vector3 angles = Camera.main.transform.eulerAngles + new Vector3(-1 * rotationChange.x * upDownSpeed * Time.deltaTime, -1 * rotationChange.y * speed * Time.deltaTime, 0);

        angles.x = Mathf.Clamp(angles.x, 10f, 89f); // Min/max angle range

        Camera.main.transform.eulerAngles = angles;

        // Move camera to new position
        Camera.main.transform.position = worldCenter - (Camera.main.transform.forward * cameraDistance);

        // Always look at the world center
        Camera.main.transform.LookAt(worldCenter);

        // Update Wind Indicator camera
        UpdateWindCamera();
    }

    // Responds to clicks on tiles, igniting them if the simulation is in the appropriate state.
    private void HandleTileClick(Tile clickedTile)
    {
        // We can click on tiles only when simulation is not running
        if (currentState == State.NewWorldState)
        {
            if (clickedTile.Ignite()) // it is ignitable
            {
                initBurningTiles.Add(clickedTile);
                visulizer.CreateFireOnTile(clickedTile);
            }
        }
    }

    // Updates the visual state of a tile when it is hovered over.
    private void HandleTileHover(bool hovered, Tile hoveredOverTile)
    {
        ResetHoveredTileColor(); // Reset the old tile's color

        // NewWorld state and also check if we're hovering a new tile
        if (hovered == true && currentState == State.NewWorldState && currentlyHoveredTile != hoveredOverTile)
        {
            currentlyHoveredTile = hoveredOverTile;
            GameObject tileInstance = visulizer.GetTileInstance(hoveredOverTile);
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
            GameObject tileInstance = visulizer.GetTileInstance(currentlyHoveredTile);
            if (tileInstance != null)
            {
                // set color back to its original color
                visulizer.SetAppropriateMaterial(currentlyHoveredTile);
            }
            currentlyHoveredTile = null;
        }
    }

    // Manages transitions between different states of the program.
    public void HandleEvent(State nextState)
    {
        switch (currentState)
        {
            case State.NewWorldState:
                switch (nextState)
                {
                    case State.RunningState:
                        currentState = State.RunningState;
                        fireSpreadSimulation = new FireSpreadSimulation(fireSpreadParams, world, initBurningTiles);
                        InfoPanel.text = "Simulation running";
                        break;
                    case State.NewWorldState:
                        currentState = State.NewWorldState;
                        GenereteNewWorld();
                        InfoPanel.text = "New world - set fire";
                        break;
                    default:
                        return;
                }
                break;

            case State.RunningState:
                switch (nextState)
                {
                    case State.StoppedState:
                        currentState = State.StoppedState;
                        InfoPanel.text = "Simulation paused";
                        break;
                    case State.NewWorldState:
                        currentState = State.NewWorldState;
                        GenereteNewWorld();
                        InfoPanel.text = "New world - set fire";
                        break;
                    default:
                        return;
                }
                break;

            case State.StoppedState:
                switch (nextState)
                {
                    case State.RunningState:
                        currentState = State.RunningState;
                        InfoPanel.text = "Simulation running";
                        break;
                    case State.NewWorldState:
                        currentState = State.NewWorldState;
                        GenereteNewWorld();
                        InfoPanel.text = "New world - set fire";
                        break;
                    default:
                        return;
                }
                break;
        }

        currentState = nextState;
    }

    private void UpdateRunPauseButtons(State currentState)
    {
        if (currentState == State.NewWorldState || currentState == State.StoppedState)
        {
            runButton.interactable = true;
            pauseButton.interactable = false;
        }
        else if (currentState == State.RunningState)
        {
            runButton.interactable = false;
            pauseButton.interactable = true;
        }
        else
        {
            runButton.interactable = true;
            pauseButton.interactable = true;
            Debug.LogError("Missing world state");
        }
    }

    // Toggles the visibility of the graph and updates its content.
    public void OnGraphButtonClicked()
    {
        showingGraph = !showingGraph;

        UpdateGraph();
    }

    // Updates the graph visualization with the latest simulation data.
    private void UpdateGraph()
    {
        if (graphVisulizer is null) return;

        if (showingGraph)
        {
            Dictionary<int, int> burningTilesOverTime = new Dictionary<int, int> { { 0, 0 } };

            if (fireSpreadSimulation is not null)
            {
                burningTilesOverTime = fireSpreadSimulation.GetBurningTilesOverTime();
            }

            graphVisulizer.ClearGraph();
            graphVisulizer.SetData(burningTilesOverTime);
            graphVisulizer.UpdateGraph();
            graphVisulizer.ShowGraph();
        }
        else
        {
            graphVisulizer.HideGraph();
        }
    }

    // Resets the world to its initial state.
    public void OnResetButtonClicked()
    {
        world.Reset();

        PrepareForNewWorld();
    }

    // Handles user input to generate a new world state.
    public void OnNewWorldButtonClicked()
    {
        HandleEvent(State.NewWorldState);
        UpdateRunPauseButtons(currentState);
    }

    public void OnRunButtonClicked()
    {
        HandleEvent(State.RunningState);
        UpdateRunPauseButtons(currentState);
    }

    public void OnPauseButtonClicked()
    {
        HandleEvent(State.StoppedState);
        UpdateRunPauseButtons(currentState);
    }

    // Handles the import button click event to load external maps or serialized worlds.
    public void OnImportClicked()
    {
        if (fileBrowserHandler is null) return;

        fileBrowserHandler.ImportFile(HandleFileLoad);
    }

    private void HandleFileLoad(string filePath)
    {
        if (filePath != null)
        {
            Debug.Log("Loading file from path: " + filePath);

            // Check the file extension
            string fileExtension = Path.GetExtension(filePath).ToLower();

            if (fileExtension == ".png" || fileExtension == ".jpg" || fileExtension == ".jpeg")
            {
                Debug.Log("Importing height map from " + fileExtension + " file.");

                IMapImporter mapImporter = new HeightMapImporter();
                int requiredWidth = inputHandler.WorldWidth;
                int requiredDepth = inputHandler.WorldDepth;

                Map<float> customHeightMap = mapImporter.GetMap(requiredWidth, requiredDepth, filePath);

                Map<int> customMoistureMap = new Map<int>(requiredWidth, requiredDepth);
                customMoistureMap.FillWithDefault(0);
                Map<VegetationType> customVegetationMap = new Map<VegetationType>(requiredWidth, requiredDepth);
                customVegetationMap.FillWithDefault(VegetationType.Grass);

                if (customHeightMap != null)
                {
                    Debug.Log("Successfully imported height map from " + fileExtension + " file.");

                    world = worldGenerator.GenerateWorldFromMaps(customHeightMap, customMoistureMap, customVegetationMap);

                    WorldBuilder.ApplyHeightMapToWorld(world, customHeightMap);
                    // Apply other maps if desired / only some can be applied
                    // WorldBuilder.Apply...
                }
            }
            else if (fileExtension == ".json")
            {
                Debug.Log("Loading serialized world from JSON file.");
                world = worldFileManager.LoadWorld(filePath);
            }
            else
            {
                Debug.LogError("Unsupported file format: " + fileExtension);
            }

            PrepareForNewWorld();
        }
        else
        {
            Debug.Log("File loading was canceled.");
        }
    }

    // Imports a tutorial map based on a given map number.
    public void ImportTutorialMap(int mapNumber)
    {
        string tutorialFileName = mapNumber + "TutorialMap.json";

        world = worldFileManager.LoadWorld(Application.streamingAssetsPath + "/TutorialWorlds/" + tutorialFileName);

        PrepareForNewWorld();
    }

    public void OnSaveClicked()
    {
        if (fileBrowserHandler is null) return;

        fileBrowserHandler.SaveFile(HandleFileSave);
    }

    private void HandleFileSave(string filePath)
    {
        if (filePath != null)
        {
            Debug.Log("Saving file path: " + filePath);
            worldFileManager.SaveWorld(world, filePath);
        }
        else
        {
            Debug.Log("File saving was canceled.");
        }
    }

    public void SetSimulationSpeed(float newSpeed)
    {
        speedOfUpdates = newSpeed;
    }

    public void ApplyInputValues()
    {
        worldGenerator.width = inputHandler.WorldWidth;
        worldGenerator.depth = inputHandler.WorldDepth;
        worldGenerator.rivers = inputHandler.Rivers;
        worldGenerator.lakeThreshold = inputHandler.LakeThreshold;
    }

    // Generates a new world based on the current parameters set in the world generator.
    public void GenereteNewWorld()
    {
        world = worldGenerator.Generate();

        PrepareForNewWorld();

        UpdateWindCamera();
    }

    // Prepares the simulation and visualization for a new world.
    private void PrepareForNewWorld()
    {
        currentState = State.NewWorldState;
        InfoPanel.text = "New world - set fire";
        UpdateRunPauseButtons(currentState);

        if (world.Width * world.Depth >= 2500) // number of tiles is small enough
        {
            visulizer.mode = VisulizerMode.Simplified;
        }

        initBurningTiles.Clear();
        VisulizerRemakeAll();
        HandleCameraAngleChange(new Vector3(0, 0, 0)); // Set init camera position and rotation

        currentlyHoveredTile = null;

        if (graphVisulizer is not null)
        {
            graphVisulizer.ClearGraph();
            graphVisulizer.SetAxes("burning tiles", "time");
            graphVisulizer.SetData(new Dictionary<int, int> { { 0, 0 } });
            graphVisulizer.UpdateGraph();
            graphVisulizer.HideGraph();
        }
    }

    // Reconstructs the entire visual representation of the world.
    private void VisulizerRemakeAll()
    {
        visulizer.DestroyAllTile();
        visulizer.DestroyAllVegetation();
        visulizer.DestroyAllFire();

        visulizer.CreateWorldTiles(world);
        visulizer.CreateAllVegetation(world);
    }
}
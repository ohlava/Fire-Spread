using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum State { NewWorldState, RunningState, StoppedState }

public class MainLogic : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private GameObject uiManagerObj, visualizerObj, inputHandlerObj, cameraHandlerObj;
    #endregion

    #region Private Fields
    private CameraHandler cameraHandler;
    private WorldGenerator worldGenerator;
    private FileBrowserHandler fileBrowserHandler;
    private SimulationManager simulationManager;
    private FireSimulation fireSimulation;
    private List<Tile> initBurningTiles;
    private WindSimulation windSimulation;
    private UIManager uiManager;
    private Visualizer visualizer;
    private InputHandler inputHandler;
    private WindIndicator windIndicator;
    private GraphVisualizer graphVisualizer;
    private FileManagementService fileManagementService;
    private Tile currentlyHoveredTile;
    private float elapsed; // time elapsed for simulation updates
    private float speedOfUpdates; // in seconds
    private bool showingGraph;
    private State currentState;
    #endregion

    #region public Fields
    public World world;
    public Settings settings;
    public FireSimParameters fireSimParams;
    #endregion

    // Awake is called when the script instance is being loaded.
    void Awake()
    {
        InitializeComponents();
        SubscribeToInputEvents();
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

    private void InitializeComponents()
    {
        initBurningTiles = new List<Tile>();
        elapsed = 0f;
        speedOfUpdates = 1.2f;
        showingGraph = false;
        currentState = State.NewWorldState;
        fireSimParams = new FireSimParameters();
        worldGenerator = new WorldGenerator();

        settings = SettingsManager.LoadSettings();
        fileBrowserHandler = FindObjectOfType<FileBrowserHandler>();
        uiManager = uiManagerObj.GetComponent<UIManager>();
        graphVisualizer = uiManagerObj.GetComponent<GraphVisualizer>();
        windIndicator = uiManagerObj.GetComponent<WindIndicator>();
        visualizer = visualizerObj.GetComponent<Visualizer>();
        cameraHandler = cameraHandlerObj.GetComponent<CameraHandler>();
        inputHandler = inputHandlerObj.GetComponent<InputHandler>();

        fileManagementService = new FileManagementService(fileBrowserHandler, worldGenerator);
    }

    private void SubscribeToInputEvents()
    {
        inputHandler.OnTileClicked += HandleTileClick;
        inputHandler.OnTileHovered += HandleTileHover;
        inputHandler.OnGraph += OnGraphButtonClicked;
        inputHandler.OnReset += OnResetButtonClicked;
        inputHandler.OnGenerateWorld += GenereteNewWorld;
        inputHandler.OnImport += OnImportClicked;
        inputHandler.OnSave += OnSaveClicked;
        inputHandler.OnRun += OnRunButtonClicked;
        inputHandler.OnPause += OnPauseButtonClicked;
        inputHandler.OnSimulationSpeedChange += SetSimulationSpeed;
    }


    // Determines whether an update of the world and simulation should be performed.
    private bool ShouldPerformUpdate()
    {
        return elapsed >= speedOfUpdates && currentState == State.RunningState;
    }

    // Carries out simulation updates and visual refreshes.
    private void PerformUpdate()
    {
        elapsed = elapsed % speedOfUpdates;

        // Run and then automatically stop running after simulation finishes
        if (!fireSimulation.Finished()) 
        {
            simulationManager.UpdateAllSimulations();

            UpdateFire();
            UpdateWind();
        }
        else
        {
            currentState = State.StoppedState;
            uiManager.UpdateInfoPanel("Simulation paused");
        }

        UpdateGraph();
    }

    // Visually updates the wind indicator of the simulation.
    private void UpdateWind()
    {
        if (windSimulation == null) return;

        List<WindEvent> events = windSimulation.GetLastUpdateEvents();

        if (events.Count == 0 || windIndicator == null) return;

        int windDirection = 0;
        float windSpeed = 0.0f;

        foreach (WindEvent windEvent in events)
        {
            switch (windEvent.Type)
            {
                case EventType.WindDirectionChange:
                    windDirection = windEvent.NewWindDirection;
                    break;
                case EventType.WindSpeedChange:
                    windSpeed = windEvent.NewWindSpeed;
                    break;
                default:
                    Debug.Log("There seems to be a problem with wind events, Unknown event");
                    break;
            }
        }

        // Actually visualize the last update changes.
        windIndicator.UpdateIndicator(windDirection, windSpeed);

        return;
    }

    // Visually updates the simulation state of the world and handles fire spread events.
    private void UpdateFire()
    {
        if (fireSimulation == null) return;

        List<FireEvent> events = fireSimulation.GetLastUpdateEvents();

        foreach (FireEvent fireEvent in events)
        {
            if (fireEvent.Type == EventType.TileStartedBurning)
            {
                visualizer.CreateFireOnTile(fireEvent.Tile);
            }
            else if (fireEvent.Type == EventType.TileStoppedBurning)
            {
                visualizer.DestroyFireOnTile(fireEvent.Tile);
                visualizer.DestroyVegetationOnTile(fireEvent.Tile);
                visualizer.MakeTileBurned(fireEvent.Tile);
            }
        }
    }

    // Responds to clicks on tiles, igniting them if the simulation is in the appropriate state.
    private void HandleTileClick(Tile clickedTile)
    {
        // We can click on tiles only when simulation is not running
        if (currentState == State.NewWorldState)
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

        if (hovered == true && currentState == State.NewWorldState && currentlyHoveredTile != hoveredOverTile)
        {
            currentlyHoveredTile = hoveredOverTile;
            GameObject tileInstance = visualizer.GetTileInstance(hoveredOverTile);
            if (tileInstance != null)
            {
                tileInstance.SetColorTo(Color.white);
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

    // Manages transitions between different states of the program.
    public void HandleEvent(State nextState)
    {
        switch (currentState)
        {
            case State.NewWorldState:
                switch (nextState)
                {
                    case State.RunningState:
                        if (initBurningTiles.Count == 0)
                        {
                            uiManager.UpdateInfoPanel("Ignite some tiles first!");
                            return; // do nothing else
                        }
                        currentState = State.RunningState;
                        SetNewSimulation();

                        uiManager.UpdateInfoPanel("Simulation running");
                        break;
                    case State.NewWorldState:
                        currentState = State.NewWorldState;
                        GenereteNewWorld();
                        uiManager.UpdateInfoPanel("New world - set fire");
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
                        uiManager.UpdateInfoPanel("Simulation paused");
                        break;
                    case State.NewWorldState:
                        currentState = State.NewWorldState;
                        GenereteNewWorld();
                        uiManager.UpdateInfoPanel("New world - set fire");
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
                        uiManager.UpdateInfoPanel("Simulation running");
                        break;
                    case State.NewWorldState:
                        currentState = State.NewWorldState;
                        GenereteNewWorld();
                        uiManager.UpdateInfoPanel("New world - set fire");
                        break;
                    default:
                        return;
                }
                break;
        }

        currentState = nextState;
    }

    // Prepares simulationManager with simulations so that we can run them easily.
    private void SetNewSimulation()
    {
        fireSimulation = new FireSimulation(fireSimParams, world, initBurningTiles);
        windSimulation = new WindSimulation(world);

        simulationManager = new SimulationManager(world);
        simulationManager.AddSimulation(fireSimulation).AddSimulation(windSimulation);
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
        if (graphVisualizer == null) return;

        if (showingGraph)
        {
            Dictionary<int, int> burningTilesOverTime = new Dictionary<int, int>();

            if (fireSimulation != null)
            {
                burningTilesOverTime = fireSimulation.GetBurningTilesOverTime();
            }

            graphVisualizer.ClearGraph();
            graphVisualizer.SetData(burningTilesOverTime);
            graphVisualizer.UpdateGraph();
            graphVisualizer.ShowGraph();
        }
        else
        {
            graphVisualizer.HideGraph();
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
        uiManager.UpdateRunPauseButtons(currentState == State.RunningState);
    }

    public void OnRunButtonClicked()
    {
        HandleEvent(State.RunningState);
        uiManager.UpdateRunPauseButtons(currentState == State.RunningState);
    }

    public void OnPauseButtonClicked()
    {
        HandleEvent(State.StoppedState);
        uiManager.UpdateRunPauseButtons(currentState == State.RunningState);
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


    // Imports a tutorial map based on a given map number.
    public void ImportTutorialMap(int mapNumber)
    {
        string tutorialFileName = mapNumber + "TutorialMap.json";

        world = WorldFileManager.LoadWorld(Application.streamingAssetsPath + "/TutorialWorlds/" + tutorialFileName);

        PrepareForNewWorld();
    }

    // Handles the import button click event to load external maps or serialized worlds.
    public void OnImportClicked()
    {
        fileManagementService.ImportFile(OnWorldImported);
    }

    private void OnWorldImported(World world)
    {
        this.world = world;
        PrepareForNewWorld();
    }

    public void OnSaveClicked()
    {
        fileManagementService.SaveWorld(world);
    }


    // Generates a new world based on the current parameters set in the world generator.
    public void GenereteNewWorld()
    {
        ApplyInputValues();

        world = worldGenerator.Generate();

        if (settings.SaveTerrainAutomatically)
        {
            fileManagementService.SaveWorldAutomatically(world);
        }

        PrepareForNewWorld();
    }

    // Prepares the simulation and visualization for a new world.
    private void PrepareForNewWorld()
    {
        currentState = State.NewWorldState;
        uiManager.UpdateInfoPanel("New world - set fire");
        uiManager.UpdateRunPauseButtons(currentState == State.RunningState);
        
        // Settings enabled or number of tiles is too high!
        if (settings.UseSimplifiedWorldVisualization || world.Width * world.Depth >= 2500) 
        {
            visualizer.mode = VisualizerMode.Simplified;
        }
        else
        {
            visualizer.mode = VisualizerMode.Standard;
        }

        initBurningTiles.Clear();
        VisulizerRemakeAll();
        cameraHandler.SetWorldCenter(new Vector3(world.Width / 2f, 2f * visualizer.TileHeightMultiplier, world.Depth / 2f));
        cameraHandler.SetCamera(world.Width, world.Depth);
        windIndicator.SetIndicatorToDefault();
        windIndicator.UpdateIndicatorCameraAngle();

        currentlyHoveredTile = null;

        if (graphVisualizer != null)
        {
            graphVisualizer.ClearGraph();
            graphVisualizer.SetAxes("burning tiles", "time");
            graphVisualizer.UpdateGraph();
            graphVisualizer.HideGraph();
        }
    }

    // Reconstructs the entire visual representation of the world.
    private void VisulizerRemakeAll()
    {
        visualizer.DestroyAllTile();
        visualizer.DestroyAllVegetation();
        visualizer.DestroyAllFire();

        visualizer.CreateWorldTiles(world);
        visualizer.CreateAllVegetation(world);
    }
}
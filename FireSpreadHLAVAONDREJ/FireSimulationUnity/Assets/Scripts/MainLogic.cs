using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum State { NewWorldState, RunningState, StoppedState }

public class MainLogic : MonoBehaviour
{
    World world;

    FireSpreadParameters fireSpreadParams = new FireSpreadParameters();
    FireSpreadSimulation fireSpreadSimulation = null;
    List<Tile> initBurningTiles = new List<Tile>();

    Visulizer visulizer;
    [SerializeField] GameObject visulizerObj;

    InputHandler inputHandler;
    [SerializeField] GameObject inputHandlerObj;

    WindIndicator windIndicator;
    [SerializeField] GameObject windIndicatorObj;

    GraphVisulizer graphVisulizer;

    private Tile currentlyHoveredTile = null;

    [SerializeField] TextMeshProUGUI InfoPanel;

    private float elapsed = 0f;
    private float speedOfUpdates = 1.2f; // in seconds

    private WorldGenerator worldGenerator;

    private bool showingGraph = false;
    private State currentState = State.NewWorldState;

    void Awake()
    {
        worldGenerator = new WorldGenerator();

        visulizer = visulizerObj.GetComponent<Visulizer>();

        // GraphVisulizer object is attached to a main camera, this finds it, there is only one graphVisualizer
        graphVisulizer = FindObjectOfType<GraphVisulizer>();

        windIndicator = windIndicatorObj.GetComponent<WindIndicator>();


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
        if (elapsed >= speedOfUpdates) // is called once every speedOfUpdates seconds
        {
            elapsed = elapsed % speedOfUpdates;

            if (currentState == State.RunningState)
            {
                Debug.Log("Simulation running");
                updateWorld();

                world.UpdateWeather();

                updateWindIndicator();

                UpdateGraph();
            }
            else if (currentState == State.StoppedState) // simulation not running
            {
                Debug.Log("Simulation paused");
            }
            else
            {
                Debug.Log("NewWorldState: set tiles on fire");
            }
        }
    }

    private void updateWindIndicator()
    {
        if (windIndicator == null) return;

        windIndicator.UpdateIndicator(world.Weather);

        return;
    }

    private void updateWindCamera()
    {
        if (windIndicator == null) return;

        windIndicator.UpdateCamera();

        return;
    }

    private void updateWorld()
    {
        if (fireSpreadSimulation == null) return;

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

    private void HandleCameraMove(float zoomChange)
    {
        float minZoom = 5f;
        float maxZoom = 50f;
        float zoomChangeSpeed = 0.05f;

        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + zoomChange * zoomChangeSpeed, minZoom, maxZoom);
    }

    private void HandleCameraAngleChange(Vector3 rotationChange)
    {
        Vector3 worldCenter = new Vector3(world.Width / 2f, 0, world.Depth / 2f);
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
        updateWindCamera();
    }

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

    private void HandleTileHover(bool hovered, Tile hoveredOverTile)
    {
        ResetHoveredTileColor(); // Reset the old tile's color

        // NewWorld state and also check if we're hovering a new tile
        if (hovered == true && currentState == State.NewWorldState && currentlyHoveredTile != hoveredOverTile)
        {
            currentlyHoveredTile = hoveredOverTile;
            GameObject tileInstance = visulizer.GetTileInstance(hoveredOverTile);
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

    private void ResetHoveredTileColor()
    {
        if (currentlyHoveredTile != null)
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

    // Handling program states and possible state transitions
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

    public void OnGraphButtonClicked()
    {
        showingGraph = !showingGraph;

        UpdateGraph();
    }

    private void UpdateGraph()
    {
        if (graphVisulizer == null) return;

        if (showingGraph)
        {
            Dictionary<int, int> burningTilesOverTime = new Dictionary<int, int>{{ 0, 0 }};

            if (fireSpreadSimulation != null)
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

    public void OnResetButtonClicked()
    {
        world.Reset();

        PrepareForNewWorld();
    }

    public void OnNewWorldButtonClicked()
    {
        HandleEvent(State.NewWorldState);
    }

    public void OnRunButtonClicked()
    {
        HandleEvent(State.RunningState);
    }

    public void OnPauseButtonClicked()
    {
        HandleEvent(State.StoppedState);
    }

    public void OnImportClicked()
    {
        IMapImporter mapImporter = new HeightMapImporter();
        int requiredWidth = inputHandler.worldWidth;
        int requiredDepth = inputHandler.worldDepth;

        Map<float> customHeightMap = mapImporter.GetMap(requiredWidth, requiredDepth);

        // TODO import other types as well
        Map<int> customMoistureMap = new Map<int>(requiredWidth, requiredDepth);
        customMoistureMap.FillWithDefault(0);
        Map<VegetationType> customVegetationMap = new Map<VegetationType>(requiredWidth, requiredDepth);
        customVegetationMap.FillWithDefault(VegetationType.Grass);

        if (customHeightMap != null)
        {
            Debug.Log("Import from PNG heighmap");

            world = worldGenerator.GenerateWorldFromMaps(customHeightMap, customMoistureMap, customVegetationMap);

            WorldBuilder.ApplyHeightMapToWorld(world, customHeightMap);
            // Apply other maps if desired / only some can be applied

            PrepareForNewWorld();
        }
        else // use serialized World
        {
            world = World.Load();
            PrepareForNewWorld();
        }
    }

    public void ImportTutorialMap(int mapNumber)
    {
        string fileName = mapNumber + "TutorialMap.json";

        world = World.Load(fileName);

        PrepareForNewWorld();

        CameraHandler.SetCameraPositionAndOrientation(world.Width, world.Depth);
        HandleCameraAngleChange(new Vector3(0, 0, 0)); // trigger with default zero vector so cameras size is set correctly
    }

    public void OnSaveClicked()
    {
        world.Save();
    }

    public void SetSimulationSpeed(float newSpeed)
    {
        speedOfUpdates = newSpeed;
    }

    public void ApplyInputValues()
    {
        worldGenerator.width = inputHandler.worldWidth;
        worldGenerator.depth = inputHandler.worldDepth;
        worldGenerator.rivers = inputHandler.rivers;
        worldGenerator.lakeThreshold = inputHandler.lakeThreshold;
    }

    public void GenereteNewWorld()
    {
        world = worldGenerator.Generate();

        PrepareForNewWorld();

        CameraHandler.SetCameraPositionAndOrientation(world.Width, world.Depth);
        HandleCameraAngleChange(new Vector3(0, 0, 0)); // trigger with default zero vector so cameras size is set correctly

        updateWindCamera();
    }

    private void PrepareForNewWorld()
    {
        currentState = State.NewWorldState;
        InfoPanel.text = "New world - set fire";

        if (world.Width * world.Depth >= 3000) // number of tiles is small enough
        {
            visulizer.mode = VisulizerMode.Standard;
        }

        initBurningTiles.Clear();
        VisulizerRemakeAll();

        currentlyHoveredTile = null;

        if (graphVisulizer != null)
        {
            graphVisulizer.ClearGraph();
            graphVisulizer.SetAxes("burning tiles", "time");
            graphVisulizer.SetData(new Dictionary<int, int> { { 0, 0 } });
            graphVisulizer.UpdateGraph();
            graphVisulizer.HideGraph();
        }
    }

    private void VisulizerRemakeAll()
    {
        visulizer.DestroyAllTile();
        visulizer.DestroyAllVegetation();
        visulizer.DestroyAllFire();

        visulizer.CreateWorldTiles(world);
        visulizer.CreateAllVegetation(world);
    }
}
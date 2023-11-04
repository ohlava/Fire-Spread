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

    CameraHandler cameraHandler;
    [SerializeField] GameObject cameraHandlerObj;

    InputHandler inputHandler;
    [SerializeField] GameObject inputHandlerObj;

    WindIndicator windIndicator;
    [SerializeField] GameObject windIndicatorObj;

    GraphVisulizer graphVisulizer;

    [SerializeField] private Camera secondaryWindArrowCamera;
    [SerializeField] private GameObject arrow; // for wind indicator

    [SerializeField] TextMeshProUGUI InfoPanel;

    // for tile hover feature
    private Tile currentlyHoveredTile = null;
    private Color originalTileColor;

    private float elapsed = 0f;
    private float speedOfUpdates { get; set; } // in seconds

    private WorldGenerator worldGenerator;

    private bool showingGraph = false;
    private State currentState = State.NewWorldState;

    void Awake()
    {
        worldGenerator = new WorldGenerator();

        // object is attached to a main camera, this finds it, there is only one graphVisualizer
        graphVisulizer = FindObjectOfType<GraphVisulizer>();

        visulizer = visulizerObj.GetComponent<Visulizer>();

        cameraHandler = cameraHandlerObj.GetComponent<CameraHandler>();

        inputHandler = inputHandlerObj.GetComponent<InputHandler>();

        windIndicator = windIndicatorObj.GetComponent<WindIndicator>();

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
                //Debug.Log("Simulation running");
                updateWorld();

                world.UpdateWeather();
                windIndicator.UpdateIndicator(world.Weather);

                if (showingGraph) // TODO just show, update seperately
                {
                    showGraph(showingGraph); // for graph update
                }
            }
            else if (currentState == State.StoppedState) // simulation not running
            {
                //Debug.Log("Simulation paused");
            }
            else
            {
                //Debug.Log("NewWorldState: set tiles on fire");
            }
        }
    }

    private void updateWorld()
    {
        if (fireSpreadSimulation == null)
        {
            return;
        }

        // Run and then automatically stop running after simulation finishes
        if (!fireSpreadSimulation.Finished())
        {
            fireSpreadSimulation.Update();

            // Get the events from the last update
            List<FireEvent> events = fireSpreadSimulation.GetLastUpdateEvents();

            // Handle these events, for example by visualizing them
            foreach (FireEvent evt in events)
            {
                if (evt.Type == EventType.StartedBurning)
                {
                    visulizer.CreateFireOnTile(evt.Tile);
                }
                else if (evt.Type == EventType.StoppedBurning)
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







    private void HandleCameraMove(Vector3 direction)
    {
        // TODO calculate based on world size OR camera always pointing to the middle of the map
        float speed = 10f;

        Camera.main.transform.Translate(direction * speed * Time.deltaTime);
    }

    private void HandleCameraAngleChange(Vector3 rotationChange)
    {
        // TODO calculate based on world size OR camera always pointing to the middle of the map

        float speed = 30.0f; 
        float upDownSpeed = 30.0f;

        Vector3 adjustedRotationChange = new Vector3(rotationChange.x * upDownSpeed * Time.deltaTime, rotationChange.y * speed * Time.deltaTime, rotationChange.z);

        Vector3 newRotation = Camera.main.transform.eulerAngles + adjustedRotationChange;
        // Ensure the rotation stays normal for the X-axis
        if (newRotation.x > 90.0f && newRotation.x < 270.0f)
            newRotation.x = 90.0f;

        Camera.main.transform.eulerAngles = newRotation;

        // Second camera for wind indicator is responsible for maintaining same angle as the first so we see 
        SetWindIndicatorCamera();

        return;
    }

    private void SetWindIndicatorCamera()
    {
        // Calculate the direction vector from the main camera to its focal point (assuming it's the world's center)
        Vector3 mainCameraDirection = Camera.main.transform.forward;

        // Determine the distance from the arrow to the secondary camera
        float distanceToArrow = (secondaryWindArrowCamera.transform.position - arrow.transform.position).magnitude;

        // Update the secondary camera's position to be at the same distance from the arrow but in the opposite direction of the main camera
        // Here we invert the direction by using -mainCameraDirection
        Vector3 secondaryCameraPosition = arrow.transform.position - mainCameraDirection * distanceToArrow;

        secondaryWindArrowCamera.transform.position = secondaryCameraPosition;

        // Now, make the secondary camera LookAt the arrow
        secondaryWindArrowCamera.transform.LookAt(arrow.transform.position);

        return;
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
        if (hovered == true)
        {
            if (currentState == State.NewWorldState)
            {
                if (currentlyHoveredTile != hoveredOverTile) // Check if we're hovering a new tile
                {
                    ResetHoveredTileColor(); // Reset the old tile's color

                    currentlyHoveredTile = hoveredOverTile;
                    GameObject tileInstance = visulizer.GetTileInstance(hoveredOverTile);
                    if (tileInstance != null)
                    {
                        Renderer renderer = tileInstance.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            originalTileColor = renderer.material.color; // store original color
                            renderer.material.color = Color.white;
                        }
                    }
                }
            }
        }
        else
        {
            ResetHoveredTileColor();
        }
        
    }

    private void ResetHoveredTileColor()
    {
        if (currentlyHoveredTile != null)
        {
            GameObject tileInstance = visulizer.GetTileInstance(currentlyHoveredTile);
            if (tileInstance != null)
            {
                tileInstance.SetColorTo(originalTileColor);
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
                        graphVisulizer.ClearGraph();
                        InfoPanel.text = "Simulation running";
                        break;
                    case State.NewWorldState:
                        currentState = State.NewWorldState;
                        GenereteNewWorld();
                        InfoPanel.text = "New world - set fire";
                        break;
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
                        graphVisulizer.ClearGraph();
                        InfoPanel.text = "New world - set fire";
                        break;
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
                        graphVisulizer.ClearGraph();
                        InfoPanel.text = "New world - set fire";
                        break;
                }
                break;
        }

        currentState = nextState;
    }

    public void OnGraphButtonClicked()
    {
        showingGraph = !showingGraph;
        showGraph(showingGraph);
    }

    public void OnResetButtonClicked()
    {
        currentState = State.NewWorldState;
        InfoPanel.text = "New world - set fire";

        world.Reset();

        initBurningTiles.Clear();

        VisulizerRemakeAll();

        graphVisulizer.ClearGraph();
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

        Map<float> customHeightMap = mapImporter.GetMap(requiredWidth,requiredDepth);

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

    public void OnSaveClicked()
    {
        world.Save();
    }

    public void SetSimulationSpeed(float newSpeed)
    {
        speedOfUpdates = newSpeed;
    }

    private void ApplyInputValues()
    {
        worldGenerator.width = inputHandler.worldWidth;
        worldGenerator.depth = inputHandler.worldDepth;
        worldGenerator.rivers = inputHandler.rivers;
        worldGenerator.lakeThreshold = inputHandler.lakeThreshold;
    }

    private void GenereteNewWorld()
    {
        world = worldGenerator.Generate();

        PrepareForNewWorld();

        cameraHandler.SetCameraPositionAndOrientation(world.Width, world.Depth);

        SetWindIndicatorCamera();
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
    }

    private void VisulizerRemakeAll()
    {
        visulizer.DestroyAllTile();
        visulizer.DestroyAllVegetation();
        visulizer.DestroyAllFire();

        visulizer.CreateWorldTiles(world);
        visulizer.CreateAllVegetation(world);
    }

    // Uses GraphVisulizer to draw the graph of all the simulation update states, now tiles burning over time
    private void showGraph(bool show)
    {
        if (showingGraph)
        {
            if (fireSpreadSimulation != null)
            {
                Dictionary<int, int> d = fireSpreadSimulation.GetBurningTilesOverTime();
                graphVisulizer.DrawGraph(d, "burning tiles", "time");
            }
        }
        else
        {
            graphVisulizer.HideGraph();
        }
    }
}
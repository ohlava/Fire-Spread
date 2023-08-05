using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum State
{
    NewWorldState,
    RunningState,
    StoppedState
}

public class MainLogic : MonoBehaviour
{
    World world;
    FireSpreadSimulation fireSpreadSimulation = null;
    List<Tile> initBurningTiles = new List<Tile>();

    WorldGenerator worldGenerator;
    [SerializeField] GameObject generatorObj;

    Visulizer visulizer;
    [SerializeField] GameObject visulizerObj;

    InputHandler inputHandler;
    [SerializeField] GameObject inputHandlerObj;

    GraphVisulizer graphVisulizer;

    [SerializeField] TextMeshProUGUI InfoPanel;

    private float elapsed = 0f;
    private float speedOfUpdates { get; set; } // in seconds
    FireSpreadParameters fireSpreadParams = new FireSpreadParameters();

    private bool showingGraph = false;
    private State currentState = State.NewWorldState;

    void Awake()
    {
        worldGenerator = generatorObj.GetComponent<WorldGenerator>();
        visulizer = visulizerObj.GetComponent<Visulizer>();
        inputHandler = inputHandlerObj.GetComponent<InputHandler>();

        // object is attached to a main camera, this finds it, there is only one graphVisualizer
        graphVisulizer = GameObject.FindObjectOfType<GraphVisulizer>();

        inputHandler.OnTileClicked += HandleTileClick;
        inputHandler.OnCameraMove += HandleCameraMove;
        inputHandler.OnCameraAngleChange += HandleCameraAngleChange;

        inputHandler.OnGraph += OnGraphButtonClicked;
        inputHandler.OnReset += OnResetButtonClicked;
        inputHandler.OnGenerateWorld += GenereteNewWorld;
        inputHandler.OnFieldValueChange += ApplyInputValues;
        inputHandler.OnImportToggle += ToggleUseCustomMap;
        inputHandler.OnRun += OnRunButtonClicked;
        inputHandler.OnPause += OnPauseButtonClicked;
        inputHandler.onSimulationSpeedChange += SetSimulationSpeed;
    }

    private void HandleCameraMove(Vector3 direction)
    {
        // Implement your camera movement logic here, for example:
        Camera.main.transform.Translate(direction * Time.deltaTime * 10);
    }

    private void HandleCameraAngleChange(Vector3 rotationChange)
    {
        // Implement your camera rotation logic here, for example:
        Camera.main.transform.Rotate(rotationChange);
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

    public void ToggleUseCustomMap()
    {
        worldGenerator.useCustomMap = !worldGenerator.useCustomMap;
    }

    public void SetSimulationSpeed(float newSpeed)
    {
        speedOfUpdates = newSpeed;
    }





    private void ApplyInputValues()
    {
        worldGenerator.worldWidth = inputHandler.worldWidth;
        worldGenerator.worldDepth = inputHandler.worldDepth;
        worldGenerator.rivers = inputHandler.rivers;
        worldGenerator.lakeThreshold = inputHandler.lakeThreshold;
    }

    public void GenereteNewWorld()
    {
        currentState = State.NewWorldState;
        InfoPanel.text = "New world - set fire";

        world = worldGenerator.GetWorld();

        int numberOfTiles = world.Width * world.Depth;
        if (numberOfTiles <= 3000)
        {
            visulizer.mode = VisulizerMode.Standard;
        }
        else
        {
            visulizer.mode = VisulizerMode.Simplified;
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
        visulizer.SetCameraPositionAndOrientation(world.Width, world.Depth);
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
        if (elapsed >= speedOfUpdates)
        {
            elapsed = elapsed % speedOfUpdates;

            // is called once every speedOfUpdates seconds
            RunEverything();
        }
    }

    private void RunEverything()
    {
        if (currentState == State.RunningState)
        {
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

                if (showingGraph)
                {
                    showGraph(showingGraph); // for graph update
                }
            }
            else
            {
                currentState = State.StoppedState;
                InfoPanel.text = "Simulation paused";
            }

            Debug.Log("Simulation running");

        }
        else if (currentState == State.StoppedState) // simulation not running
        {
            Debug.Log("Simulation paused");
        }
        else // simulation not running State.NewWorldState
        {
            Debug.Log("New: set tiles on fire");
        }
        
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
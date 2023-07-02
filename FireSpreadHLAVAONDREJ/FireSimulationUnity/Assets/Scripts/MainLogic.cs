using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State
{
    NewWorldState,
    RunningState,
    StoppedState,
    GraphState
}

public class MainLogic : MonoBehaviour
{
    World world;
    FireSpreadSimulation fireSpreadSimulation;
    List<Tile> initBurningTiles = new List<Tile>();

    WorldGenerator worldGenerator;
    [SerializeField] GameObject generatorObj;

    Visulizer visulizer;
    [SerializeField] GameObject visulizerObj;

    InputHandler inputHandler;
    [SerializeField] GameObject inputHandlerObj;

    void Awake()
    {
        worldGenerator = generatorObj.GetComponent<WorldGenerator>();
        visulizer = visulizerObj.GetComponent<Visulizer>();
        inputHandler = inputHandlerObj.GetComponent<InputHandler>();

        inputHandler.OnTileClicked += HandleTileClick;
        inputHandler.OnCameraMove += HandleCameraMove;
        inputHandler.OnCameraAngleChange += HandleCameraAngleChange;
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



    private float elapsed = 0f;
    public float speedOfUpdates = 1f; // in seconds
    FireSpreadParameters fireSpreadParams = new FireSpreadParameters();
    private State currentState = State.NewWorldState;


    public void HandleEvent(State nextState)
    {
        switch (currentState)
        {
            case State.NewWorldState:
                switch (nextState)
                {
                    case State.RunningState:
                        currentState = State.RunningState;
                        break;
                    case State.NewWorldState:
                        currentState = State.NewWorldState;
                        GenereteNewWorld();
                        break;
                }
                break;

            case State.RunningState:
                switch (nextState)
                {
                    case State.StoppedState:
                        currentState = State.StoppedState;
                        break;
                    case State.NewWorldState:
                        currentState = State.NewWorldState;
                        GenereteNewWorld();
                        break;
                    case State.GraphState:
                        currentState = State.GraphState;
                        SaveGraph();
                        break;
                }
                break;

            case State.StoppedState:
                switch (nextState)
                {
                    case State.RunningState:
                        currentState = State.RunningState;
                        break;
                    case State.NewWorldState:
                        currentState = State.NewWorldState;
                        GenereteNewWorld();
                        break;
                    case State.GraphState:
                        currentState = State.GraphState;
                        SaveGraph();
                        break;
                }
                break;

            case State.GraphState:
                switch (nextState)
                {
                    case State.RunningState:
                        currentState = State.RunningState;
                        break;
                    case State.NewWorldState:
                        currentState = State.NewWorldState;
                        GenereteNewWorld();
                        break;
                }
                break;
        }

        currentState = nextState;
    }

    public void OnNewWorldButtonClicked()
    {
        HandleEvent(State.NewWorldState);
    }

    public void OnRunSimulationButtonClicked()
    {
        HandleEvent(State.RunningState);
    }

    public void OnPauseSimulationButtonClicked()
    {
        HandleEvent(State.StoppedState);
    }

    public void OnShowGraphsButtonClicked()
    {
        HandleEvent(State.GraphState);
    }

    public void OnResetButtonClicked()
    {
        currentState = State.NewWorldState;

        world.Reset();

        initBurningTiles.Clear();
        fireSpreadSimulation = new FireSpreadSimulation(fireSpreadParams, world, initBurningTiles);

        VisulizerRemakeAllBrandNew();
    }

    public void ToggleUseCustomMap()
    {
        // Toggle the useCustomMap value
        worldGenerator.useCustomMap = !worldGenerator.useCustomMap;

        Debug.Log("Toggled useCustomMap. New value: " + worldGenerator.useCustomMap);
    }





    public void GenereteNewWorld()
    {
        world = worldGenerator.GetWorld();

        int numberOfTiles = world.Width * world.Depth;
        if (numberOfTiles <= 10000)
        {
            visulizer.mode = VisulizerMode.Standard;
        }

        initBurningTiles.Clear();
        fireSpreadSimulation = new FireSpreadSimulation(fireSpreadParams, world, initBurningTiles);

        VisulizerRemakeAllBrandNew();
    }

    private void VisulizerRemakeAllBrandNew()
    {
        visulizer.DestroyAllTile();
        visulizer.DestroyAllVegetation();
        visulizer.DestroyAllFire();

        visulizer.CreateWorldTiles(world);
        visulizer.CreateAllVegetation(world);
        visulizer.SetCameraPositionAndOrientation(world);
    }

    private void SaveGraph()
    {
        Dictionary<int, int> d = fireSpreadSimulation.GetBurningTilesOverTime();
        GraphCreator.SaveToFile(d);
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

            RunEverything();
        }
    }

    private void RunEverything()
    {
        if (currentState == State.RunningState)
        {
            
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
            }
            
            Debug.Log("RUNNING");

        }
        else if (currentState == State.StoppedState) // simulation not running
        {
            Debug.Log("NOT running");
        }
        else if (currentState == State.NewWorldState) // simulation not running
        {
            Debug.Log("NEW WORLD");
        }
        else if (currentState == State.GraphState) // simulation not running
        {
            // TODO actually show graph on screen
            Debug.Log("GRAPH");
        }
        else // should never happen
        {
            Debug.Log("OTHER");
        }
    }
}
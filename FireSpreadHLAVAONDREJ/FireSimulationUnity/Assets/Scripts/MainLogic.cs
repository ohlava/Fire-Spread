using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (clickedTile.Ignite()) // is not ignited if water for example
        {
            initBurningTiles.Add(clickedTile);
            visulizer.CreateFireOnTile(clickedTile);
        }
    }

    private float elapsed = 0f;
    public float speedOfUpdates = 1f; // in seconds
    public bool simulationRunning = false;

    public void buttonStartSimulation()
    {
        // Initialize FireSpreadSimulation.
        FireSpreadParameters fireSpreadParams = new FireSpreadParameters();
        fireSpreadSimulation = new FireSpreadSimulation(fireSpreadParams, world, initBurningTiles);
        simulationRunning = true;

    }
    public void buttonStopSimulation()
    {
        simulationRunning = false;

    }
    public void sliderSetspeedOfUpdates(float speed)
    {
        speedOfUpdates = speed;
        Debug.Log(speedOfUpdates);
        Debug.Log("speed set to:");
        Debug.Log(speedOfUpdates);

    }
    public void buttonResetCurrentWorld()
    {
        simulationRunning = false;
        world.Reset();
        visulizer.DestroyAllFire();
        // renew vegetation
        // reset also colors of tiles
    }
    public void buttonGenereteNewWorld()
    {
        simulationRunning = false;
        world = worldGenerator.GetWorld();

        visulizer.DestroyAllTile();
        visulizer.DestroyAllVegetation();
        visulizer.DestroyAllFire();

        visulizer.CreateWorldTiles(world);
        visulizer.CreateAllVegetation(world);
        visulizer.SetCameraPositionAndOrientation(world);
    }



    // Start is called before the first frame update
    void Start()
    {
        world = worldGenerator.GetWorld();

        int numberOfTiles = world.Width * world.Depth;
        if (numberOfTiles <= 10000)
        {
            visulizer.mode = VisulizerMode.Standard;
        }

        visulizer.CreateWorldTiles(world);
        visulizer.CreateAllVegetation(world);
        visulizer.SetCameraPositionAndOrientation(world);
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
        if (simulationRunning)
        {
            Debug.Log("running");

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
            Debug.Log("Not running");
        }
    }
}
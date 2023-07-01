using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLogic : MonoBehaviour
{

    WorldGenerator worldGenerator;
    [SerializeField] GameObject generatorObj;

    Visulizer visulizer;
    [SerializeField] GameObject visulizerObj;

    InputHandler inputHandler;
    [SerializeField] GameObject inputHandlerObj;

    World world;

    float elapsed = 0f;
    float speedOfUpdates = 2f; // in seconds

    FireSpreadSimulation fireSpreadSimulation;
    public bool simulationRunning = false;
    public void buttonStartSimulation()
    {
        simulationRunning = true;
    }
    public void buttonStopSimulation()
    {
        simulationRunning = false;
    }
    public void sliderSetspeedOfUpdates(float speed)
    {
        speedOfUpdates = speed;
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

        FireSpreadParameters fireSpreadParams = new FireSpreadParameters();
        fireSpreadSimulation = new FireSpreadSimulation(fireSpreadParams, world);

        visulizer.DestroyAllTile();
        visulizer.DestroyAllVegetation();
        visulizer.DestroyAllFire();

        visulizer.CreateWorldTiles(world);
        visulizer.CreateVegetation(world);
        visulizer.SetCameraPositionAndOrientation(world);
    }



    void Awake()
    {
        worldGenerator = generatorObj.GetComponent<WorldGenerator>();
        visulizer = visulizerObj.GetComponent<Visulizer>();
        inputHandler = inputHandlerObj.GetComponent<InputHandler>();

        inputHandler.OnTileClicked += HandleTileClick;
        inputHandler.OnCameraMove += HandleCameraMove;
        inputHandler.OnCameraAngleChange += HandleCameraAngleChange;
    }

    private void HandleTileClick(Tile clickedTile)
    {
        visulizer.CreateFireOnTile(clickedTile);
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



    // Start is called before the first frame update
    void Start()
    {
        world = worldGenerator.GetWorld();

        // Initialize FireSpreadSimulation.
        FireSpreadParameters fireSpreadParams = new FireSpreadParameters();
        fireSpreadSimulation = new FireSpreadSimulation(fireSpreadParams, world);

        visulizer.CreateWorldTiles(world);
        visulizer.CreateVegetation(world);
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
            List<FireEvent> events = fireSpreadSimulation.GetEventsFromLastUpdate();

            // Handle these events, for example by visualizing them
            foreach (FireEvent evt in events)
            {
                if (evt.EventType == EventType.StartedBurning)
                {
                    visulizer.CreateFireOnTile(evt.Tile);
                }
                else if (evt.EventType == EventType.StoppedBurning)
                {
                    visulizer.DestroyFireOnTile(evt.Tile);
                    // set color of tile to brown
                }
            }

        }
        else
        {
            Debug.Log("Not running");
        }
    }
}
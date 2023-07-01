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
        visulizer.CreateWorldTiles(world);
        visulizer.SetCameraPositionAndOrientation(world);
        foreach (var tile in world.Grid)
        {
            if (tile.Moisture != 100) { visulizer.CreateVegetationOnTile(tile, tile.Vegetation); }
        }
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= 5f)
        {
            elapsed = elapsed % 5f;

            //world = worldGenerator.GenerateNewWorld();
            //visulizer.DeleteAllTiles();
            //visulizer.CreateWorld(world);
        }
    }

}
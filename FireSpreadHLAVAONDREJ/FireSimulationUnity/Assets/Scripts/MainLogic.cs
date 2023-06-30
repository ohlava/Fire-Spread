using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLogic : MonoBehaviour
{

    WorldGenerator worldGenerator;
    [SerializeField] GameObject generatorObj;

    Visulizer visulizer;
    [SerializeField] GameObject visulizerObj;

    World world;

    float elapsed = 0f;

    void Awake()
    {
        worldGenerator = generatorObj.GetComponent<WorldGenerator>();
        visulizer = visulizerObj.GetComponent<Visulizer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        world = worldGenerator.GenerateNewWorld();
        visulizer.CreateWorld(world);
        visulizer.SetCameraPositionAndOrientation(world);
    }

    // Update is called once per frame
    void Update()
    {
        Tile clickedTile = CheckForLeftClick();
        if (clickedTile != null)
        {
            visulizer.CreateFireOnTile(clickedTile);
        }

        clickedTile = CheckForRightClick();
        if (clickedTile != null)
        {
            System.Random rand = new System.Random();
            visulizer.CreateVegetationOnTile(clickedTile, (VegetationType)rand.Next(3));
        }



        elapsed += Time.deltaTime;
        if (elapsed >= 5f)
        {
            elapsed = elapsed % 5f;

            //world = worldGenerator.GenerateNewWorld();
            //visulizer.DeleteAllTiles();
            //visulizer.CreateWorld(world);
        }
    }

    private Tile CheckForLeftClick() // always check for null before using clickedTile.
    {
        // If left mouse button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            // Cast a ray from camera to click point
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hitInfo;

            // If ray hits a tile
            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity))
            {
                // Get the corresponding world tile
                Tile worldTile = visulizer.GetWorldTileFromInstance(hitInfo.transform.gameObject);
                if (worldTile != null)
                {
                    // Return the tile that was clicked
                    return worldTile;
                }
            }
        }

        // Return null if no tile was clicked
        return null;
    }

    private Tile CheckForRightClick() // always check for null before using clickedTile.
    {
        // If left mouse button is clicked
        if (Input.GetMouseButtonDown(1))
        {
            // Cast a ray from camera to click point
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hitInfo;

            // If ray hits a tile
            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity))
            {
                // Get the corresponding world tile
                Tile worldTile = visulizer.GetWorldTileFromInstance(hitInfo.transform.gameObject);
                if (worldTile != null)
                {
                    // Return the tile that was clicked
                    return worldTile;
                }
            }
        }

        // Return null if no tile was clicked
        return null;
    }

}
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
        Tile clickedTile = CheckForClick();
        if (clickedTile != null)
        {
            visulizer.createFireOnTile(clickedTile);
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

    private Tile CheckForClick() // always check for null before using clickedTile.
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
                // Change color to red
                Renderer renderer = hitInfo.transform.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.red;
                }

                // Get the corresponding world tile
                Tile worldTile = visulizer.GetWorldTileFromInstance(hitInfo.transform.gameObject);
                if (worldTile != null)
                {
                    Debug.Log(worldTile);
                    // Return the tile that was clicked
                    return worldTile;
                }
            }
        }

        // Return null if no tile was clicked
        return null;
    }


}

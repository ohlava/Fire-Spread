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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

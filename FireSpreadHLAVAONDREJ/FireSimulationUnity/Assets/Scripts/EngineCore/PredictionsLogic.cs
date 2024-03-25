using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PredictionLogic : MonoBehaviour
{
    [SerializeField] private GameObject inputHandlerObj;

    private InputHandler inputHandler;

    // Awake is called when the script instance is being loaded.
    void Awake()
    {
        InitializeComponents();
        SubscribeToInputEvents();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void InitializeComponents()
    {
        inputHandler = inputHandlerObj.GetComponent<InputHandler>();
    }

    private void SubscribeToInputEvents()
    {
        inputHandler.OnGenerateData += GenerateData;
        inputHandler.OnPythonPredict += PythonPredict;
        inputHandler.OnHeatMap += HeatMap;
        inputHandler.OnReset += Reset;
    }


    public void GenerateData()
    {
        Debug.Log("GenerateData called");
        return;
    }

    public void PythonPredict()
    {
        Debug.Log("PythonPredict called");
        return;
    }

    public void HeatMap()
    {
        Debug.Log("HeatMap called");
        return;
    }

    public void Reset()
    {
        Debug.Log("Reset called");
        return;
    }
}

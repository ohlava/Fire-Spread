using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class InputHandler : MonoBehaviour
{
    #region Delegates and Events
    public delegate void TileHoverHandler(bool hovered, Tile hoveredOverTile);
    public event TileHoverHandler OnTileHovered;

    public delegate void TileClickHandler(Tile clickedTile);
    public event TileClickHandler OnTileClicked;

    public delegate void GraphHandler();
    public event GraphHandler OnGraph;

    public delegate void ResetWorldHandler();
    public event ResetWorldHandler OnReset;

    public delegate void GenerateWorldHandler();
    public event GenerateWorldHandler OnGenerateWorld;

    public delegate void ImportHandler();
    public event ImportHandler OnImport;

    public delegate void SaveHandler();
    public event SaveHandler OnSave;

    public delegate void RunHandler();
    public event RunHandler OnRun;

    public delegate void PauseHandler();
    public event PauseHandler OnPause;

    public delegate void SimulationSpeedChangeHandler(float newSpeed);
    public event SimulationSpeedChangeHandler OnSimulationSpeedChange;

    public delegate void GenerateDataHandler();
    public event GenerateDataHandler OnGenerateData;

    public delegate void PythonHandler();
    public event PythonHandler OnPythonPredict;

    public delegate void HeatMapHandler();
    public event HeatMapHandler OnHeatMap;
    #endregion

    #region Serialized Fields
    [SerializeField] GameObject visulizerObj, cameraHandlerObj, uiManagerObj;
    [SerializeField] GameObject worldWidthInputFieldObj, worldDepthInputFieldObj, riversInputFieldObj;
    [SerializeField] GameObject heatMapInputFieldObj, generateDataAmountFieldObj, spreadProbabilityFieldObj, vegetationFactorFieldObj, slopeFactorFieldObj, moistureFactorFieldObj;
    [SerializeField] Slider simulationSpeedSlider;
    [SerializeField] Slider lakeThresholdSlider;
    #endregion

    #region Private Fields
    private bool initializing = false; // for input fields to not trigger generation of new world when being initialized to default value
    private WindIndicator windIndicator;
    private Visualizer visualizer;
    private CameraHandler cameraHandler;
    private TMP_InputField worldWidthInputField, worldDepthInputField, riversInputField;
    private TMP_InputField heatMapInputField, generateDataAmountField, spreadProbabilityField, vegetationFactorField, slopeFactorField, moistureFactorField;
    #endregion

    #region Public Properties
    public LayerMask ignoreLayer;
    public float SimulationSpeed { get; private set; }

    public int WorldWidth { get; private set; }
    public int WorldDepth { get; private set; }
    public int Rivers { get; private set; }
    public float LakeThreshold { get; private set; }

    public int MaxWorldWidth { get; private set; }
    public int MaxWorldDepth { get; private set; }
    public int MaxRivers { get; private set; }

    // predictions scene
    public int HeatMapIterations { get; private set; }
    public float SpreadProbability { get; private set; }
    public float VegetationFactor { get; private set; }
    public float SlopeFactor { get; private set; }
    public float MoistureFactor { get; private set; }
    public int GenerateDataAmount { get; private set; }

    public int MaxHeatMapIterations { get; private set; }
    public float MaxSpreadProbability { get; private set; }
    public float MaxVegetationFactor { get; private set; }
    public float MaxSlopeFactor { get; private set; }
    public float MaxMoistureFactor { get; private set; }
    public int MaxGenerateDataAmount { get; private set; }
    #endregion

    // Awake is called when the script instance is being loaded.
    private void Awake()
    {
        initializing = true; 
        InitializeFields();
        InitializeDefaultValues();
        initializing = false;
    }

    private void InitializeFields()
    {
        visualizer = visulizerObj?.GetComponent<Visualizer>();
        cameraHandler = cameraHandlerObj.GetComponent<CameraHandler>();
        windIndicator = uiManagerObj.GetComponent<WindIndicator>();

        if (worldWidthInputFieldObj != null)
            worldWidthInputField = worldWidthInputFieldObj.GetComponent<TMP_InputField>();

        if (worldDepthInputFieldObj != null)
            worldDepthInputField = worldDepthInputFieldObj.GetComponent<TMP_InputField>();

        if (riversInputFieldObj != null)
            riversInputField = riversInputFieldObj.GetComponent<TMP_InputField>();


        if (heatMapInputFieldObj != null)
            heatMapInputField = heatMapInputFieldObj.GetComponent<TMP_InputField>();

        if (spreadProbabilityFieldObj != null)
            spreadProbabilityField = spreadProbabilityFieldObj.GetComponent<TMP_InputField>();

        if (vegetationFactorFieldObj != null)
            vegetationFactorField = vegetationFactorFieldObj.GetComponent<TMP_InputField>();

        if (slopeFactorFieldObj != null)
            slopeFactorField = slopeFactorFieldObj.GetComponent<TMP_InputField>();

        if (moistureFactorFieldObj != null)
            moistureFactorField = moistureFactorFieldObj.GetComponent<TMP_InputField>();

        if (generateDataAmountFieldObj != null)
            generateDataAmountField = generateDataAmountFieldObj.GetComponent<TMP_InputField>();
    }

    private void InitializeDefaultValues()
    {
        SimulationSpeed = 1.2f;
        WorldWidth = 25;
        WorldDepth = 25;
        Rivers = 3;
        LakeThreshold = 0.12f;

        MaxWorldWidth = 100;
        MaxWorldDepth = 100;
        MaxRivers = 25;

        // predictions scene
        HeatMapIterations = 30;
        GenerateDataAmount = 10;
        SpreadProbability = 0.3f;
        VegetationFactor = 1f;
        SlopeFactor = 1f;
        MoistureFactor = 1f;

        MaxHeatMapIterations = 100;
        MaxGenerateDataAmount = 999;
        MaxSpreadProbability = 1.0f;
        MaxVegetationFactor = 2.0f;
        MaxSlopeFactor = 2.0f;
        MaxMoistureFactor = 2.0f;


        if (simulationSpeedSlider != null)
            simulationSpeedSlider.value = simulationSpeedSlider.maxValue - SimulationSpeed;

        if (worldWidthInputField != null)
            worldWidthInputField.text = WorldWidth.ToString();

        if (worldDepthInputField != null)
            worldDepthInputField.text = WorldDepth.ToString();

        if (riversInputField != null)
            riversInputField.text = Rivers.ToString();

        if (lakeThresholdSlider != null)
            lakeThresholdSlider.value = LakeThreshold;


        if (heatMapInputField != null)
            heatMapInputField.text = HeatMapIterations.ToString();

        if (generateDataAmountField != null)
            generateDataAmountField.text = GenerateDataAmount.ToString();

        if (spreadProbabilityField != null)
            spreadProbabilityField.text = SpreadProbability.ToString();

        if (vegetationFactorField != null)
            vegetationFactorField.text = VegetationFactor.ToString();

        if (slopeFactorField != null)
            slopeFactorField.text = SlopeFactor.ToString();

        if (moistureFactorField != null)
            moistureFactorField.text = MoistureFactor.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject()) // If cursor is NOT over UI object
        {
            HandleTileClick();
            HandleTileHover();
        }
        else
        {
            OnTileHovered?.Invoke(false, null);
        }

        HandleCameraMove();
        HandleCameraAngleChange();
        HandleActionButtons();
    }



    // Method that handles the common raycasting functionality
    private Tile RaycastForTile()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Casts a ray from camera to the clicked point
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, ~ignoreLayer)) // The tilde (~) is used to invert the mask, so the raycast will ignore the layers specified in the ignoreLayer
        {
            return visualizer.GetWorldTileFromInstance(hitInfo.transform.gameObject);
        }

        return null;
    }

    // Detects if the mouse pointer is hovering over a game tile.
    private void HandleTileHover()
    {
        Tile worldTile = RaycastForTile();
        if (worldTile != null)
        {
            OnTileHovered?.Invoke(true, worldTile);
        }
        else
        {
            OnTileHovered?.Invoke(false, null);
        }
    }

    // Detects mouse clicks on some world tile.
    private void HandleTileClick()
    {
        if (Input.GetMouseButton(0)) // Input.GetMouseButton(0) - (user can click and hold) / Input.GetMouseButtonDown(0) (only clicking)
        {
            Tile worldTile = RaycastForTile();
            if (worldTile != null)
            {
                OnTileClicked?.Invoke(worldTile);
            }
        }
    }

    // Responds to specific inputs to zoom the camera in or out.
    private void HandleCameraMove()
    {
        float zoomChange = Input.GetAxis("Mouse ScrollWheel") * 20f; // Make faster compared to keyboard input

        if (Input.GetKey(KeyCode.I))
            zoomChange -= 1;

        if (Input.GetKey(KeyCode.K))
            zoomChange += 1;

        if (zoomChange != 0 && cameraHandler != null)
        {
            cameraHandler.ZoomCamera(zoomChange);
        }
    }

    // Responds to key inputs to change the camera's viewing angle vertically and horizontally.
    private void HandleCameraAngleChange()
    {
        Vector3 rotationChange = new Vector3();

        // Vertical rotation changes
        if (Input.GetKey(KeyCode.S))
            rotationChange += Vector3.right;

        if (Input.GetKey(KeyCode.W))
            rotationChange += Vector3.left;

        // Horizontal rotation changes
        if (Input.GetKey(KeyCode.A))
            rotationChange += Vector3.down;

        if (Input.GetKey(KeyCode.D))
            rotationChange += Vector3.up;


        if (rotationChange != Vector3.zero)
        {
            if (cameraHandler != null)
                cameraHandler.RotateCamera(rotationChange);

            if (windIndicator != null) 
                windIndicator.UpdateIndicatorCameraAngle();
        }
    }

    // Responds to key inputs for some extra key quick access features.
    private void HandleActionButtons()
    {
        if (Input.GetKeyDown(KeyCode.R))
            OnReset?.Invoke();
            
        if (Input.GetKeyDown(KeyCode.Space))
            TriggerRun();

        if (Input.GetKeyDown(KeyCode.Escape))
            BackToMainMenu();

        if (Input.GetKey(KeyCode.P))
            cameraHandler.CaptureHighResolutionScreenshot();

        if (Input.GetKeyDown(KeyCode.N))
            cameraHandler.SaveCameraPosition();

        if (Input.GetKeyDown(KeyCode.M))
            cameraHandler.ApplySavedCameraPosition();
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void TriggerGraph()
    {
        OnGraph?.Invoke();
    }

    public void TriggerReset()
    {
        OnReset?.Invoke();
    }

    public void TriggerGenerateWorld()
    {
        OnGenerateWorld?.Invoke();
    }

    public void TriggerImport()
    {
        OnImport?.Invoke();
    }

    public void TriggerSave()
    {
        OnSave?.Invoke();
    }

    public void TriggerRun()
    {
        OnRun?.Invoke();
    }

    public void TriggerPause()
    {
        OnPause?.Invoke();
    }

    public void TriggerGenerateData()
    {
        OnGenerateData?.Invoke();
    }

    public void TriggerPythonPredict()
    {
        OnPythonPredict?.Invoke();
    }

    public void TriggerHeatMap()
    {
        OnHeatMap?.Invoke();
    }

    // Method to set values from input fields for ints.
    public void SetValueFromInput(string input, Action<int> setter, int maxValue, TMP_InputField inputField, Action eventTrigger = null)
    {
        if (int.TryParse(input, out int parsedValue))
        {
            parsedValue = Mathf.Clamp(parsedValue, 0, maxValue);
            setter(parsedValue);
            inputField.text = parsedValue.ToString();

            if (!initializing)
            {
                eventTrigger?.Invoke();
            }
        }
    }

    // Method to set values from input fields for floats, including those with a trailing dot or comma.
    public void SetValueFromInput(string input, Action<float> setter, float maxValue, TMP_InputField inputField, Action eventTrigger = null)
    {
        // Replace comma with dot if present
        string normalizedInput = input.Replace(',', '.').Trim();

        // Check if the input ends with a dot and remove it for parsing
        string parseString = normalizedInput.EndsWith(".") ? normalizedInput.Substring(0, normalizedInput.Length - 1) : normalizedInput;

        if (float.TryParse(parseString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float parsedValue))
        {
            parsedValue = Mathf.Clamp(parsedValue, 0f, maxValue);
            setter(parsedValue);

            // Update the input field text to ensure it reflects the parsed value, keeping the original format.
            inputField.text = input.EndsWith(".") || input.EndsWith(",") ? parsedValue.ToString() + input[input.Length - 1] : parsedValue.ToString();

            if (!initializing)
            {
                eventTrigger?.Invoke();
            }
        }
    }


    public void SetWorldWidth(string widthString)
    {
        SetValueFromInput(widthString, value => WorldWidth = value, MaxWorldWidth, worldWidthInputField, TriggerGenerateWorld);
    }

    public void SetWorldDepth(string depthString)
    {
        SetValueFromInput(depthString, value => WorldDepth = value, MaxWorldDepth, worldDepthInputField, TriggerGenerateWorld);
    }

    public void SetRivers(string riversString)
    {
        SetValueFromInput(riversString, value => Rivers = value, MaxRivers, riversInputField, TriggerGenerateWorld);
    }

    // Sets the simulation speed based on the user's input through a connected UI slider "onValueChanged" event in Unity editor.
    public void SetSimulationSpeed(float value)
    {
        if (simulationSpeedSlider == null) return;

        SimulationSpeed = simulationSpeedSlider.maxValue - value;
        SimulationSpeed = Mathf.Max(SimulationSpeed, 0.1f);

        OnSimulationSpeedChange?.Invoke(SimulationSpeed);
    }

    // Sets the lake threshold (amount of water tiles) based on the user's input through a connected UI slider "onValueChanged" event in Unity editor.
    public void SetLakeThreshold(float value)
    {
        if (lakeThresholdSlider == null) return;

        LakeThreshold = value;

        if (!initializing)
        {
            TriggerGenerateWorld();
        }
    }

    public void SetHeatMapIterations(string iterations)
    {
        SetValueFromInput(iterations, value => HeatMapIterations = value, MaxHeatMapIterations, heatMapInputField, null);
    }

    public void SetGenerateDataAmount(string amount)
    {
        SetValueFromInput(amount, value => GenerateDataAmount = value, MaxGenerateDataAmount, generateDataAmountField, null);
    }

    public void SetSpreadProbability(string probability)
    {
        SetValueFromInput(probability, value => SpreadProbability = value, MaxSpreadProbability, spreadProbabilityField, null);
    }

    public void SetVegetationFactor(string probability)
    {
        SetValueFromInput(probability, value => VegetationFactor = value, MaxVegetationFactor, vegetationFactorField, null);
    }

    public void SetSlopeFactor(string probability)
    {
        SetValueFromInput(probability, value => SlopeFactor = value, MaxSlopeFactor, slopeFactorField, null);
    }

    public void SetMoistureFactor(string probability)
    {
        SetValueFromInput(probability, value => MoistureFactor = value, MaxMoistureFactor, moistureFactorField, null);
    }
}
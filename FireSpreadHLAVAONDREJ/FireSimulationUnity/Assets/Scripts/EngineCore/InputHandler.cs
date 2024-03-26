using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
    [SerializeField] private GameObject visulizerObj, cameraHandlerObj, uiManagerObj;
    [SerializeField] private GameObject worldWidthInputFieldObj, worldDepthInputFieldObj, riversInputFieldObj;
    [SerializeField] private GameObject heatMapInputFieldObj, spreadProbabilityFieldObj, vegetationFactorFieldObj, slopeFactorFieldObj, moistureFactorFieldObj;
    [SerializeField] private Slider simulationSpeedSlider;
    [SerializeField] private Slider lakeThresholdSlider;
    #endregion

    #region Private Fields
    private bool initializing = false; // for input fields to not trigger generation of new world when being initialized to default value
    private WindIndicator windIndicator;
    private Visualizer visualizer;
    private CameraHandler cameraHandler;
    private TMP_InputField worldWidthInputField, worldDepthInputField, riversInputField;
    private TMP_InputField heatMapInputField, spreadProbabilityField, vegetationFactorField, slopeFactorField, moistureFactorField;
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

    public int MaxHeatMapIterations { get; private set; }
    public float MaxSpreadProbability { get; private set; }
    public float MaxVegetationFactor { get; private set; }
    public float MaxSlopeFactor { get; private set; }
    public float MaxMoistureFactor { get; private set; }
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
    }

    private void InitializeDefaultValues()
    {
        SimulationSpeed = 1.2f;
        WorldWidth = 25;
        WorldDepth = 25;
        Rivers = 3;
        LakeThreshold = 0.12f;

        MaxWorldWidth = 150;
        MaxWorldDepth = 150;
        MaxRivers = 25;

        // predictions scene
        HeatMapIterations = 30;
        SpreadProbability = 0.3f;
        VegetationFactor = 1f;
        SlopeFactor = 1f;
        MoistureFactor = 1f;

        MaxHeatMapIterations = 100;
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

    // Responds to specific key inputs to zoom the camera in or out.
    private void HandleCameraMove()
    {
        float zoomChange = 0;

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

    // Sets the world width based on the user's input through a connected UI input field's "onValueChanged" event in Unity editor.
    public void SetWorldWidth(string widthString)
    {
        if (worldWidthInputField == null) return;

        int parsedValue;
        if (int.TryParse(widthString, out parsedValue))
        {
            WorldWidth = Mathf.Min(parsedValue, MaxWorldWidth);
            WorldWidth = Mathf.Max(WorldWidth, 1);

            worldWidthInputField.text = WorldWidth.ToString();

            if (!initializing)
            {
                TriggerGenerateWorld();
            }
        }
    }

    // Sets the world depth based on the user's input through a connected UI input field's "onValueChanged" event in Unity editor.
    public void SetWorldDepth(string depthString)
    {
        if (worldDepthInputField == null) return;

        int parsedValue;
        if (int.TryParse(depthString, out parsedValue))
        {
            WorldDepth = Mathf.Min(parsedValue, MaxWorldDepth);
            WorldDepth = Mathf.Max(WorldDepth, 1);

            worldDepthInputField.text = WorldDepth.ToString();

            if (!initializing)
            {
                TriggerGenerateWorld();
            }
        }
    }

    // Sets the number of world rivers based on the user's input through a connected UI input field's "onValueChanged" event in Unity editor.
    public void SetRivers(string riversString)
    {
        if (riversInputField == null) return;

        int parsedValue;
        if (int.TryParse(riversString, out parsedValue))
        {
            Rivers = Mathf.Min(parsedValue, MaxRivers);
            Rivers = Mathf.Max(Rivers, 0);

            riversInputField.text = Rivers.ToString();

            if (!initializing)
            {
                TriggerGenerateWorld();
            }
        }
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


    // Sets the number of heat map iterations based on the user's input through a connected UI input field's "onValueChanged" event in Unity editor.
    public void SetHeatMapIterations(string iterations)
    {
        if (heatMapInputField == null) return;

        int parsedValue;
        if (int.TryParse(iterations, out parsedValue))
        {
            HeatMapIterations = Mathf.Min(parsedValue, MaxHeatMapIterations);
            HeatMapIterations = Mathf.Max(HeatMapIterations, 0);

            heatMapInputField.text = HeatMapIterations.ToString();
        }
    }

    // Sets the base probability for fire spread in simulation based on the user's input through a connected UI input field's "onValueChanged" event in Unity editor.
    public void SetSpreadProbability(string probability)
    {
        if (spreadProbabilityField == null) return;

        // Check if the input ends with a dot and remove it for parsing
        string parseString = probability.EndsWith(".") ? probability.Substring(0, probability.Length - 1) : probability;

        if (float.TryParse(parseString, out float parsedValue))
        {
            SpreadProbability = Mathf.Min(parsedValue, MaxSpreadProbability);
            SpreadProbability = Mathf.Max(SpreadProbability, 0);

            // Display the original string with the dot if it was present
            spreadProbabilityField.text = probability.EndsWith(".") ? SpreadProbability.ToString() + "." : SpreadProbability.ToString();
        }
    }

    // Sets the vegetation factor for fire spread in simulation based on the user's input through a connected UI input field's "onValueChanged" event in Unity editor.
    public void SetVegetationFactor(string probability)
    {
        if (vegetationFactorField == null) return;

        // Check if the input ends with a dot and remove it for parsing
        string parseString = probability.EndsWith(".") ? probability.Substring(0, probability.Length - 1) : probability;

        if (float.TryParse(parseString, out float parsedValue))
        {
            VegetationFactor = Mathf.Min(parsedValue, MaxVegetationFactor);
            VegetationFactor = Mathf.Max(VegetationFactor, 0);

            // Display the original string with the dot if it was present
            vegetationFactorField.text = probability.EndsWith(".") ? VegetationFactor.ToString() + "." : VegetationFactor.ToString();
        }
    }

    // Sets the slope factor for fire spread in simulation based on the user's input through a connected UI input field's "onValueChanged" event in Unity editor.
    public void SetSlopeFactor(string probability)
    {
        if (slopeFactorField == null) return;

        // Check if the input ends with a dot and remove it for parsing
        string parseString = probability.EndsWith(".") ? probability.Substring(0, probability.Length - 1) : probability;

        if (float.TryParse(parseString, out float parsedValue))
        {
            SlopeFactor = Mathf.Min(parsedValue, MaxSlopeFactor);
            SlopeFactor = Mathf.Max(SlopeFactor, 0);

            // Display the original string with the dot if it was present
            slopeFactorField.text = probability.EndsWith(".") ? SlopeFactor.ToString() + "." : SlopeFactor.ToString();
        }
    }


    // Sets the moisture factor for fire spread in simulation based on the user's input through a connected UI input field's "onValueChanged" event in Unity editor.
    public void SetMoistureFactor(string probability)
    {
        if (moistureFactorField == null) return;

        // Check if the input ends with a dot and remove it for parsing
        string parseString = probability.EndsWith(".") ? probability.Substring(0, probability.Length - 1) : probability;

        if (float.TryParse(parseString, out float parsedValue))
        {
            MoistureFactor = Mathf.Min(parsedValue, MaxMoistureFactor);
            MoistureFactor = Mathf.Max(MoistureFactor, 0);

            // Display the original string with the dot if it was present
            moistureFactorField.text = probability.EndsWith(".") ? MoistureFactor.ToString() + "." : MoistureFactor.ToString();
        }
    }
}
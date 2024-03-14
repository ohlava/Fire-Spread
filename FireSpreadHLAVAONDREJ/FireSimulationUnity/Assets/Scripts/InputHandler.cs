using System.Collections;
using System.Collections.Generic;
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

    public delegate void CameraMoveHandler(float zoomChange);
    public event CameraMoveHandler OnCameraMove;

    public delegate void CameraAngleChangeHandler(Vector3 rotation);
    public event CameraAngleChangeHandler OnCameraAngleChange;

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
    public event SimulationSpeedChangeHandler onSimulationSpeedChange;
    #endregion

    #region Serialized Fields
    [SerializeField] private GameObject visulizerObj;
    [SerializeField] private Slider simulationSpeedSlider;
    [SerializeField] private GameObject worldWidthInputFieldObj;
    [SerializeField] private GameObject worldDepthInputFieldObj;
    [SerializeField] private GameObject riversInputFieldObj;
    [SerializeField] private Slider lakeThresholdSlider;
    #endregion

    #region Private Fields
    private bool initializing = false; // for input fields not to trigger generation of new world
    private Visualizer visualizer;
    private TMP_InputField worldWidthInputField;
    private TMP_InputField worldDepthInputField;
    private TMP_InputField riversInputField;
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
    #endregion

    private void Awake()
    {
        initializing = true; // beggining of initialization
        InitializeFields();
        InitializeDefaultValues();
        initializing = false; // initialization is complete
    }

    private void InitializeFields()
    {
        visualizer = visulizerObj?.GetComponent<Visualizer>();

        if (worldWidthInputFieldObj != null)
            worldWidthInputField = worldWidthInputFieldObj.GetComponent<TMP_InputField>();

        if (worldDepthInputFieldObj != null)
            worldDepthInputField = worldDepthInputFieldObj.GetComponent<TMP_InputField>();

        if (riversInputFieldObj != null)
            riversInputField = riversInputFieldObj.GetComponent<TMP_InputField>();
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
    }

    void Update()
    {
        HandleTileClick();
        HandleTileHover();
        HandleCameraMove();
        HandleCameraAngleChange();
        HandleActionButtons();
    }

    // Detects if the mouse pointer is hovering over a game tile.
    private void HandleTileHover()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return; // ignore if over UI object

        // Cast a ray from camera to click point
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, ~ignoreLayer))
        {
            Tile worldTile = visualizer.GetWorldTileFromInstance(hitInfo.transform.gameObject);
            if (worldTile != null)
            {
                // Trigger the over Tile Hovered event
                OnTileHovered?.Invoke(true, worldTile);
            }
        }
        else
        {
            OnTileHovered?.Invoke(false,null);
        }
    }

    // Detects mouse clicks on some world tile.
    private void HandleTileClick()
    {
        // Checks if the left mouse button is pressed and ensures that the pointer is not over a UI object 
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) // Input.GetMouseButton(0) - (user can hold) / Input.GetMouseButtonDown(0) (only clicking)
        {
            // Cast a ray from camera to click point
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            // If ray hits a tile
            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, ~ignoreLayer)) // The tilde (~) is used to invert the mask, so the raycast will ignore the layers specified in the ignoreLayer
            {
                // Get the corresponding world tile
                Tile worldTile = visualizer.GetWorldTileFromInstance(hitInfo.transform.gameObject);
                if (worldTile != null)
                {
                    // Trigger the Tile Clicked event
                    OnTileClicked?.Invoke(worldTile);
                }
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

        if (zoomChange != 0)
            OnCameraMove?.Invoke(zoomChange);
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
            OnCameraAngleChange?.Invoke(rotationChange);
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

    public void SetWorldWidth(string widthString)
    {
        if (worldWidthInputField == null) return; // Early exit if the field is not present

        int parsedValue;
        if (int.TryParse(widthString, out parsedValue))
        {
            WorldWidth = Mathf.Min(parsedValue, MaxWorldWidth);
            WorldWidth = Mathf.Max(WorldWidth, 1);

            // Update the displayed value
            worldWidthInputField.text = WorldWidth.ToString();

            // Only trigger world generation if initialization is complete
            if (!initializing)
            {
                TriggerGenerateWorld();
            }
        }
    }

    public void SetWorldDepth(string depthString)
    {
        if (worldDepthInputField == null) return; // Early exit if the field is not present

        int parsedValue;
        if (int.TryParse(depthString, out parsedValue))
        {
            WorldDepth = Mathf.Min(parsedValue, MaxWorldDepth);
            WorldDepth = Mathf.Max(WorldDepth, 1);

            // Update the displayed value
            worldDepthInputField.text = WorldDepth.ToString();

            // Only trigger world generation if initialization is complete
            if (!initializing)
            {
                TriggerGenerateWorld();
            }
        }
    }

    public void SetRivers(string riversString)
    {
        if (riversInputField == null) return; // Early exit if the field is not present

        int parsedValue;
        if (int.TryParse(riversString, out parsedValue))
        {
            Rivers = Mathf.Min(parsedValue, MaxRivers);
            Rivers = Mathf.Max(Rivers, 0);

            // Update the displayed value
            riversInputField.text = Rivers.ToString();

            // Only trigger world generation if initialization is complete
            if (!initializing)
            {
                TriggerGenerateWorld();
            }
        }
    }

    public void SetSimulationSpeed(float value)
    {
        if (simulationSpeedSlider == null) return; // Early exit if the field is not present

        SimulationSpeed = simulationSpeedSlider.maxValue - value;
        SimulationSpeed = Mathf.Max(SimulationSpeed, 0.1f);

        onSimulationSpeedChange?.Invoke(SimulationSpeed);
    }

    public void SetLakeThreshold(float value)
    {
        if (lakeThresholdSlider == null) return; // Early exit if the field is not present

        LakeThreshold = value;

        // Only trigger world generation if initialization is complete
        if (!initializing)
        {
            TriggerGenerateWorld();
        }
    }
}
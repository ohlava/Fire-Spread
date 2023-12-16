using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class InputHandler : MonoBehaviour
{
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

    public delegate void FieldValueChangeHandler();
    public event FieldValueChangeHandler OnFieldValueChange;

    public delegate void ImportHandler();
    public event ImportHandler OnImport;

    public delegate void SaveHandler();
    public event SaveHandler OnSave;

    public delegate void RunHandler();
    public event RunHandler OnRun;

    public delegate void PauseHandler();
    public event PauseHandler OnPause;

    public delegate void OnSimulationSpeedChange(float newSpeed);
    public event OnSimulationSpeedChange onSimulationSpeedChange;

    public LayerMask ignoreLayer;

    Visulizer visulizer;
    [SerializeField] GameObject visulizerObj;

    [SerializeField] private Slider simulationSpeedSlider;
    [SerializeField] private GameObject worldWidthInputFieldObj;
    TMP_InputField worldWidthInputField;
    [SerializeField] private GameObject worldDepthInputFieldObj;
    TMP_InputField worldDepthInputField;
    [SerializeField] private GameObject riversInputFieldObj;
    TMP_InputField riversInputField;
    [SerializeField] private Slider lakeThresholdSlider;

    public float simulationSpeed { get; private set; }
    public int worldWidth { get; private set; }
    public int MaxWorldWidth = 150;
    public int worldDepth { get; private set; }
    public int MaxWorldDepth = 150;
    public int rivers { get; private set; }
    public int MaxRivers = 25;
    public float lakeThreshold { get; private set; }


    void Awake()
    {
        visulizer = visulizerObj.GetComponent<Visulizer>();

        if (worldWidthInputFieldObj != null)
            worldWidthInputField = worldWidthInputFieldObj.GetComponent<TMP_InputField>();

        if (worldDepthInputFieldObj != null)
            worldDepthInputField = worldDepthInputFieldObj.GetComponent<TMP_InputField>();

        if (riversInputFieldObj != null)
            riversInputField = riversInputFieldObj.GetComponent<TMP_InputField>();



        simulationSpeed = 1.2f; // speed is set as reverse of slider, define number of seconds
        worldWidth = 25;
        worldDepth = 25;
        rivers = 3;
        lakeThreshold = 0.12f;

        if (simulationSpeedSlider != null)
            simulationSpeedSlider.value = simulationSpeedSlider.maxValue - simulationSpeed;

        if (worldWidthInputField != null)
            worldWidthInputField.text = worldWidth.ToString();

        if (worldDepthInputField != null)
            worldDepthInputField.text = worldDepth.ToString();

        if (riversInputField != null)
            riversInputField.text = rivers.ToString();

        if (lakeThresholdSlider != null)
            lakeThresholdSlider.value = lakeThreshold;
    }

    void Update()
    {
        HandleTileClick();
        HandleTileHover();
        HandleCameraMove();
        HandleCameraAngleChange();
        HandleActionButtons();
    }

    private void HandleTileHover()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return; // ignore if over UI object

        // Cast a ray from camera to click point
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, ~ignoreLayer))
        {
            Tile worldTile = visulizer.GetWorldTileFromInstance(hitInfo.transform.gameObject);
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
                Tile worldTile = visulizer.GetWorldTileFromInstance(hitInfo.transform.gameObject);
                if (worldTile != null)
                {
                    // Trigger the Tile Clicked event
                    OnTileClicked?.Invoke(worldTile);
                }
            }
        }
    }

    private void HandleCameraMove()
    {
        float zoomChange = 0;

        if (Input.GetKey(KeyCode.I))
            zoomChange -= 1;

        if (Input.GetKey(KeyCode.K))
            zoomChange += 1;

        if (zoomChange != 0)
        {
            OnCameraMove?.Invoke(zoomChange);
        }
    }

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
            // Notify about the rotation changes
            OnCameraAngleChange?.Invoke(rotationChange);
    }

    private void HandleActionButtons()
    {
        if (Input.GetKey(KeyCode.R))
        {
            CameraHandler.SetCameraPositionAndOrientation(worldWidth, worldDepth);
            OnCameraAngleChange?.Invoke(new Vector3(0,0,0));
        }
            
        if (Input.GetKey(KeyCode.Space))
            TriggerRun();

        if (Input.GetKey(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu");
        }
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
            worldWidth = Mathf.Min(parsedValue, MaxWorldWidth);
            worldWidth = Mathf.Max(worldWidth, 1);

            // Update the displayed value
            worldWidthInputField.text = worldWidth.ToString();

            OnFieldValueChange?.Invoke();
        }
    }

    public void SetWorldDepth(string depthString)
    {
        if (worldDepthInputField == null) return; // Early exit if the field is not present

        int parsedValue;
        if (int.TryParse(depthString, out parsedValue))
        {
            worldDepth = Mathf.Min(parsedValue, MaxWorldDepth);
            worldDepth = Mathf.Max(worldDepth, 1);

            // Update the displayed value
            worldDepthInputField.text = worldDepth.ToString();

            OnFieldValueChange?.Invoke();
        }
    }

    public void SetRivers(string riversString)
    {
        if (riversInputField == null) return; // Early exit if the field is not present

        int parsedValue;
        if (int.TryParse(riversString, out parsedValue))
        {
            rivers = Mathf.Min(parsedValue, MaxRivers);
            rivers = Mathf.Max(rivers, 0);

            // Update the displayed value
            riversInputField.text = rivers.ToString();

            OnFieldValueChange?.Invoke();
        }
    }

    public void SetSimulationSpeed(float value)
    {
        if (simulationSpeedSlider == null) return; // Early exit if the field is not present

        simulationSpeed = simulationSpeedSlider.maxValue - value;
        simulationSpeed = Mathf.Max(simulationSpeed, 0.1f);

        onSimulationSpeedChange?.Invoke(simulationSpeed);
    }

    public void SetLakeThreshold(float value)
    {
        if (lakeThresholdSlider == null) return; // Early exit if the field is not present

        lakeThreshold = value;
        OnFieldValueChange?.Invoke();
    }
}
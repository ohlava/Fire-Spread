using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Presets;

// publisher class
public class InputHandler : MonoBehaviour
{
    public delegate void TileClickHandler(Tile clickedTile);
    public event TileClickHandler OnTileClicked;

    public delegate void CameraMoveHandler(Vector3 direction);
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

    public delegate void ImportToggleHandler();
    public event ImportToggleHandler OnImportToggle;

    public delegate void RunHandler();
    public event RunHandler OnRun;

    public delegate void PauseHandler();
    public event PauseHandler OnPause;

    public delegate void OnSimulationSpeedChange(float newSpeed);
    public event OnSimulationSpeedChange onSimulationSpeedChange;

    public LayerMask ignoreLayer;

    Visulizer visulizer;
    [SerializeField] GameObject visulizerObj;

    // These references for fields are there to later be change in case user for example exceeds limit for Max value
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
    public int MaxRivers = 50;
    public float lakeThreshold { get; private set; }


    void Awake()
    {
        visulizer = visulizerObj.GetComponent<Visulizer>();
        worldWidthInputField = worldWidthInputFieldObj.GetComponent<TMP_InputField>();
        worldDepthInputField = worldDepthInputFieldObj.GetComponent<TMP_InputField>();
        riversInputField = riversInputFieldObj.GetComponent<TMP_InputField>();

        simulationSpeed = 2f; // speed is set as reverse of slider so define number of seconds with minus
        worldWidth = 20;
        worldDepth = 20;
        rivers = 1;
        lakeThreshold = 0.12f;

        simulationSpeedSlider.value = simulationSpeed;
        worldWidthInputField.text = worldWidth.ToString();
        worldDepthInputField.text = worldDepth.ToString();
        riversInputField.text = rivers.ToString();
        lakeThresholdSlider.value = lakeThreshold;
    }

    void Update()
    {
        HandleTileClick();
        HandleCameraMove();
        HandleCameraAngleChange();
        HandleActionButtons();
    }

    private void HandleTileClick()
    {
        // Checks if the left mouse button is pressed and ensures that the pointer is not over a UI object 
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
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
        Vector3 direction = new Vector3();

        if (Input.GetKey(KeyCode.W))
            direction += Vector3.forward;

        if (Input.GetKey(KeyCode.S))
            direction += Vector3.back;

        if (Input.GetKey(KeyCode.A))
            direction += Vector3.left;

        if (Input.GetKey(KeyCode.D))
            direction += Vector3.right;

        if (direction != Vector3.zero)
            OnCameraMove?.Invoke(direction);
    }

    private void HandleCameraAngleChange()
    {
        float speed = 25.0f; // Change to any speed you feel comfortable with.
        float upDownSpeed = 0.5f;

        Vector3 rotationChange = new Vector3();

        if (Input.GetKey(KeyCode.K))
            rotationChange += Vector3.right;

        if (Input.GetKey(KeyCode.I))
            rotationChange -= Vector3.right;

        if (rotationChange != Vector3.zero)
            OnCameraAngleChange?.Invoke(rotationChange * upDownSpeed);

        if (Input.GetKey(KeyCode.J))
        {
            Camera.main.transform.Rotate(Vector3.down * speed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.L))
        {
            Camera.main.transform.Rotate(Vector3.up * speed * Time.deltaTime, Space.World);
        }
    }

    private void HandleActionButtons()
    {
        if (Input.GetKey(KeyCode.R))
            visulizer.SetCameraPositionAndOrientation(worldWidth, worldDepth);
        if (Input.GetKey(KeyCode.Space))
            TriggerRun();
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

    public void TriggerImportToggle()
    {
        OnImportToggle?.Invoke();
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
        simulationSpeed = simulationSpeedSlider.maxValue - value;
        simulationSpeed = Mathf.Max(simulationSpeed, 0.1f);

        onSimulationSpeedChange?.Invoke(simulationSpeed);
    }

    public void SetLakeThreshold(float value)
    {
        lakeThreshold = value;
        OnFieldValueChange?.Invoke();
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour
{
    public delegate void TileClickHandler(Tile clickedTile);
    public event TileClickHandler OnTileClicked;

    public delegate void CameraMoveHandler(Vector3 direction);
    public event CameraMoveHandler OnCameraMove;

    public delegate void CameraAngleChangeHandler(Vector3 rotation);
    public event CameraAngleChangeHandler OnCameraAngleChange;

    public LayerMask ignoreLayer;

    Visulizer visulizer;
    [SerializeField] GameObject visulizerObj;

    void Awake()
    {
        visulizer = visulizerObj.GetComponent<Visulizer>();
    }

    void Update()
    {
        HandleTileClick();
        HandleCameraMove();
        HandleCameraAngleChange();
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
}
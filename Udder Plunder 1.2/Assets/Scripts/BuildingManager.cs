using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class BuildingManager : MonoBehaviour
{
    [Header("References")]
    public Grid layoutGrid;
    public GameObject buildingPrefab;
    public LayerMask groundMask;

    [Header("Input")]
    public InputActionAsset inputActions;
    public string actionMapName = "Player";     // The name of your action map
    public string placeActionName = "Place";    // The name of the place action

    private InputAction placeAction;
    private GameObject previewInstance;
    private HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();

    private void OnEnable()
    {
        var actionMap = inputActions.FindActionMap(actionMapName);
        placeAction = actionMap.FindAction(placeActionName);

        if (placeAction != null)
        {
            placeAction.Enable();
            placeAction.performed += OnPlace;
        }
    }

    private void OnDisable()
    {
        if (placeAction != null)
        {
            placeAction.performed -= OnPlace;
            placeAction.Disable();
        }
    }

    private void Update()
    {
        HandlePreview();
    }

    private void HandlePreview()
    {
        if (previewInstance == null)
        {
            previewInstance = Instantiate(buildingPrefab);
            previewInstance.GetComponent<Collider>().enabled = false;
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = 0;

        Vector3Int cellPosition = layoutGrid.WorldToCell(mouseWorldPos);
        Vector3 snappedPosition = layoutGrid.GetCellCenterWorld(cellPosition);
        previewInstance.transform.position = snappedPosition;
    }

    private void OnPlace(InputAction.CallbackContext context)
    {
        if (previewInstance == null) return;

        Vector3Int cellPosition = layoutGrid.WorldToCell(previewInstance.transform.position);
        if (occupiedCells.Contains(cellPosition))
        {
            Debug.Log("Cell is already occupied!");
            return;
        }

        Vector3 placePos = layoutGrid.GetCellCenterWorld(cellPosition);
        Instantiate(buildingPrefab, placePos, Quaternion.identity);
        occupiedCells.Add(cellPosition);
    }
}

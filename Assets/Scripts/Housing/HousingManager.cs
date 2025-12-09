using UnityEngine;
using System.Collections.Generic;

public class HousingManager : MonoBehaviour
{
    public static HousingManager Instance;

    [Header("Settings")]
    public float GridSize = 1.0f;
    public LayerMask GroundLayer;
    public Material PreviewMaterialValid;
    public Material PreviewMaterialInvalid;

    private GameObject _currentPreviewObject;
    private Furniture _currentFurniturePrefab;
    private bool _isPlacementMode = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (_isPlacementMode)
        {
            UpdatePlacement();
        }
    }

    public void StartPlacement(string prefabName)
    {
        // Clean up previous preview if any
        CancelPlacement();

        var prefab = WorldObjectManager.Instance.WorldObjectPrefabs.Find(p => p.name == prefabName);
        if (prefab == null)
        {
            Debug.LogError($"[HousingManager] Prefab not found: {prefabName}");
            return;
        }

        _currentFurniturePrefab = prefab.GetComponent<Furniture>();
        if (_currentFurniturePrefab == null)
        {
             Debug.LogError($"[HousingManager] Prefab {prefabName} is not a Furniture");
             return;
        }

        _currentPreviewObject = Instantiate(prefab);
        
        // Disable colliders on preview to avoid self-collision checks
        foreach(var col in _currentPreviewObject.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        _isPlacementMode = true;
    }

    public void CancelPlacement()
    {
        if (_currentPreviewObject != null)
        {
            Destroy(_currentPreviewObject);
            _currentPreviewObject = null;
        }
        _currentFurniturePrefab = null;
        _isPlacementMode = false;
    }

    private void UpdatePlacement()
    {
        if (_currentPreviewObject == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, GroundLayer))
        {
            Vector3 snapPos = GetSnappedPosition(hit.point);
            _currentPreviewObject.transform.position = snapPos;

            // Rotate with R key
            if (Input.GetKeyDown(KeyCode.R))
            {
                _currentPreviewObject.transform.Rotate(0, 90, 0);
            }

            // Click to place
            if (Input.GetMouseButtonDown(0))
            {
                TryPlaceObject(snapPos, _currentPreviewObject.transform.rotation);
            }
            
            // Right click to cancel
            if (Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
            }
        }
    }

    private Vector3 GetSnappedPosition(Vector3 rawPos)
    {
        float x = Mathf.Round(rawPos.x / GridSize) * GridSize;
        float z = Mathf.Round(rawPos.z / GridSize) * GridSize;
        return new Vector3(x, rawPos.y, z);
    }

    private void TryPlaceObject(Vector3 pos, Quaternion rot)
    {
        // Here we should check for collisions/validity
        // For now, just place it
        
        if (_currentFurniturePrefab != null)
        {
            WorldObjectManager.Instance.SpawnObjectServerRpc(_currentFurniturePrefab.name, pos, rot);
            CancelPlacement(); // Exit placement mode after placing
        }
    }
}

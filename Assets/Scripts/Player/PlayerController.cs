using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    public float MoveSpeed = 5f;
    public float RotateSpeed = 10f;

    [Header("Interaction")]
    public float InteractionRange = 10f;
    public LayerMask GroundLayer;

    private Camera _camera;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        _camera = Camera.main;
        
        // Camera follow logic could be added here or on a separate script
        if (_camera != null)
        {
            var follow = _camera.gameObject.AddComponent<CameraFollow>();
            follow.Target = transform;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        HandleMovement();
        HandleInteraction();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 dir = new Vector3(h, 0, v).normalized;

        if (dir.magnitude >= 0.1f)
        {
            // Move
            transform.position += dir * MoveSpeed * Time.deltaTime;

            // Rotate
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, RotateSpeed * Time.deltaTime);
        }
    }

    private void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            // Toggle Housing UI
            if (HousingUI.Instance != null)
            {
                HousingUI.Instance.Toggle();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            // Place object or interact
            // Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            // if (Physics.Raycast(ray, out RaycastHit hit, InteractionRange, GroundLayer))
            // {
            //     HousingManager.Instance.TryPlaceObject(hit.point);
            // }
        }
    }
}

public class CameraFollow : MonoBehaviour
{
    public Transform Target;
    public Vector3 Offset = new Vector3(0, 10, -10);
    public float SmoothSpeed = 5f;

    private void LateUpdate()
    {
        if (Target == null) return;

        Vector3 desiredPos = Target.position + Offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, SmoothSpeed * Time.deltaTime);
        transform.LookAt(Target);
    }
}

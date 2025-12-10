using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    public float MoveSpeed = 6f;
    public float MouseSensitivity = 200f;

    public Transform CameraPivot;
    private float pitch = 0f;

    private Rigidbody rb;
    private Camera cam;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // X,Z는 에디터에서 Freeze, 코드에서는 전체를 고정

        cam = Camera.main;
        if (cam != null)
        {
            cam.transform.SetParent(CameraPivot);
            cam.transform.localPosition = Vector3.zero;
            cam.transform.localRotation = Quaternion.identity;
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (!IsOwner) return;

//        HandleMouseLook();
        HandleMovement();
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;

        HandleMouseLook();

    }
    // ===========================================
    // Mouse Look : 몸(Yaw) + 머리(Pitch)
    // ===========================================
    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;

        // 몸체(플레이어)는 Y축 회전만 한다 (좌우)
        transform.Rotate(Vector3.up * mouseX);

        // 머리(Pivot)는 X축 회전 (상하)
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -85f, 85f);

//        CameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        float yaw = transform.eulerAngles.y;

        CameraPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    // ===========================================
    // Movement
    // ===========================================
    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(h, 0f, v).normalized;
        Vector3 move = transform.TransformDirection(input) * MoveSpeed;

        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }
}

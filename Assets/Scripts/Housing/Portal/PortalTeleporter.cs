using UnityEngine;

public class PortalTeleporter : MonoBehaviour
{
    // [Inspector에서 연결] 포털의 반대편 Transform (다른 포털의 Root Transform)
    public Transform otherPortalTransform;

    // [Inspector에서 연결] 플레이어의 카메라 Transform (순간 이동에 필요)
    public Transform playerCameraTransform;

    private bool isPlayerOverlapping = false;
    private const float threshold = 0.5f;
    private CharacterController characterController;

    void Start()
    {
        playerCameraTransform = Camera.main.transform;
        // 플레이어의 부모(캐릭터)에 CharacterController가 있다고 가정
        if (playerCameraTransform != null && playerCameraTransform.parent != null)
        {
            characterController = playerCameraTransform.parent.GetComponent<CharacterController>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOverlapping = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOverlapping = false;
        }
    }

    void Update()
    {
        if (isPlayerOverlapping)
        {
            // 현재 포털의 Root Transform을 기준으로 계산
            Transform thisPortalRoot = transform.parent;
            Vector3 offsetFromPortal = playerCameraTransform.position - thisPortalRoot.position;
            float dotProduct = Vector3.Dot(thisPortalRoot.forward, offsetFromPortal);

            if (dotProduct > threshold)
            {
                TeleportPlayer(thisPortalRoot);
                isPlayerOverlapping = false;
            }
        }
    }

    private void TeleportPlayer(Transform thisPortalRoot)
    {
        Transform playerRoot = playerCameraTransform.parent;

        // 1. 위치 변환: 현재 포털을 기준으로 한 플레이어 위치를 반대 포털 기준으로 변환
        Vector3 newWorldPosition = otherPortalTransform.TransformPoint(
            thisPortalRoot.InverseTransformPoint(playerRoot.position)
        );

        // 2. 회전 변환: 반대 포털 회전에 맞게 플레이어 회전을 조정 + 180도 회전
        Quaternion rotationDifference = otherPortalTransform.rotation * Quaternion.Inverse(thisPortalRoot.rotation);
        rotationDifference *= Quaternion.Euler(0f, 180f, 0f);

        Quaternion newRotation = rotationDifference * playerRoot.rotation;

        // 3. 순간 이동 실행
        if (characterController != null)
        {
            characterController.enabled = false;
            playerRoot.position = newWorldPosition;
            playerRoot.rotation = newRotation;
            characterController.enabled = true;
        }
        else
        {
            playerRoot.position = newWorldPosition;
            playerRoot.rotation = newRotation;
        }
    }
}
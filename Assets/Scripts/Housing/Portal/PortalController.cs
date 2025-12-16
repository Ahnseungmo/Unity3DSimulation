using UnityEngine;

public class PortalController : MonoBehaviour
{
    // [Inspector에서 연결] 포털 표면을 찍을 전용 카메라
    public Camera portalCamera;

    // [Inspector에서 연결] 포털 반대편의 'Camera_Anchor' Transform
    public Transform otherPortalCameraAnchor;

    // [Inspector에서 연결] 플레이어의 카메라 Transform
    public Transform playerCameraTransform;

    // **선택 사항: Oblique Projection을 위한 설정**
    // 포털 표면 (Surface) 오브젝트의 Transform
    public Transform portalSurfaceTransform;

    private void LateUpdate()
    {
        // 1. 카메라 위치 동기화: 플레이어 카메라와 현재 포털의 상대적 위치 계산
        Vector3 playerOffsetFromThisPortal = playerCameraTransform.position - transform.position;
//        portalCamera = Camera.main;

        // 2. 반대편 앵커 기준으로 portalCamera의 위치 설정
        portalCamera.transform.position = otherPortalCameraAnchor.position + playerOffsetFromThisPortal;

        // 3. 카메라 회전 동기화: 플레이어 카메라와 현재 포털의 상대적 회전 계산
        Quaternion rotationDifference = Quaternion.Inverse(transform.rotation) * playerCameraTransform.rotation;

        // 4. 반대편 앵커를 기준으로 portalCamera의 회전 설정 (180도 회전은 시점 이동 스크립트에서 처리됨)
        portalCamera.transform.rotation = otherPortalCameraAnchor.rotation * rotationDifference;

        // 5. (고급) 포털 경계면을 기준으로 카메라 클리핑 조정 (Oblique Projection)
        // 이 코드는 더 복잡하며, 완벽한 포털을 위해 추가됩니다.
        // float crossProduct = Vector3.Dot(portalCamera.transform.forward, portalSurfaceTransform.forward);
        // if (crossProduct < 0)
        // {
        //     // Oblique Projection Matrix 설정 코드 (생략)
        // }
    }
}
using UnityEngine;

public class PortalDoor : MonoBehaviour
{
    // 실내의 모습을 담아 렌더링할 보조 카메라
    public Camera portalCamera;

    // 실내에 있는 문 오브젝트의 Transform (포털 반대편)
    public Transform otherPortalTransform;

    // 플레이어의 카메라 (렌더링 결과물을 볼 주체)
    public Transform playerCameraTransform;

    void LateUpdate()
    {
        // 1. 플레이어 카메라와 현재 포털(실외 문) 간의 상대적 위치 계산
        Vector3 playerOffsetFromPortal = playerCameraTransform.position - transform.position;

        // 2. 이 오프셋을 'otherPortalTransform'의 상대적인 위치로 변환하여 
        //    portalCamera의 목표 위치를 설정 (Mirroring Position)
        //    otherPortalTransform.position은 실내 문 반대편의 위치입니다.
        portalCamera.transform.position = otherPortalTransform.position + playerOffsetFromPortal;

        // 3. 플레이어 카메라와 현재 포털 간의 상대적인 회전 계산
        Quaternion playerRotationFromPortal = Quaternion.Inverse(transform.rotation) * playerCameraTransform.rotation;

        // 4. 이 회전 오프셋을 'otherPortalTransform'에 적용하여
        //    portalCamera의 목표 회전을 설정 (Mirroring Rotation)
        portalCamera.transform.rotation = otherPortalTransform.rotation * playerRotationFromPortal;
    }
}

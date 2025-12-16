using UnityEngine;

[ExecuteAlways]   // 에디터에서도 바로 반영
public class QuadControll : MonoBehaviour
{
    public MeshRenderer targetRenderer; // 기준이 될 Mesh

    [Header("Optional")]
    public bool matchPosition = true;
    public bool matchRotation = true;

    // Quad 실제 월드 크기 (다른 스크립트에서 사용 가능)
    [HideInInspector] public float width;
    [HideInInspector] public float height;

    void Start()
    {
        UpdateQuad();
    }

    void UpdateQuad()
    {
        if (targetRenderer == null) return;

        // 🔹 로컬 Mesh Bounds 사용 (회전 안정성)
        MeshFilter mf = targetRenderer.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return;

        Bounds localBounds = mf.sharedMesh.bounds;

        // 월드 스케일 반영
        Vector3 worldSize = Vector3.Scale(
            localBounds.size,
            targetRenderer.transform.lossyScale
        );

        width = worldSize.x;
        height = worldSize.y;

        // 🔹 Quad는 기본 1x1
        transform.localScale = new Vector3(width, height, 1f);

        // 🔹 위치 맞추기
        if (matchPosition)
            transform.position = targetRenderer.bounds.center;

        // 🔹 회전 맞추기 (포털 방향 핵심)
        if (matchRotation)
            transform.rotation = targetRenderer.transform.rotation;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        UpdateQuad();
    }
#endif
}

using UnityEngine;

public class CircularMotion : MonoBehaviour
{
    [Tooltip("优先使用此 Transform 作为旋转中心；若为空则使用 centerPosition")]
    public Transform centerTransform;
    public Vector3 centerPosition = Vector3.zero; // 备用中心点（仅在没有 centerTransform 时使用）
    public float radius = 5f; // 旋转半径（Inspector 显示 / 初始设置）
    public float speed = 60f; // 每秒旋转角度（度）
    [Tooltip("开启后在 Update 中打印调试日志（仅用于定位执行顺序问题）")]
    public bool debugLogs = false;

    void Start()
    {
        Vector3 center = GetCenter();
        var offset = transform.position - center;

        // 如果初始位置与中心几乎重合，则用 radius 放置一个默认位置
        if (offset.sqrMagnitude <= 0.0001f && radius > 0f)
        {
            transform.position = center + Vector3.right * radius;
        }
    }

    void Update()
    {
        Vector3 center = GetCenter();

        // 以当前 transform 为基础旋转（不会强制重写半径，只绕当前偏移旋转）
        float angleThisFrame = speed * Time.deltaTime;
        transform.RotateAround(center, Vector3.forward, angleThisFrame);

        if (debugLogs)
        {
            Debug.Log($"[CircularMotion] Update time={Time.time:F3} pos={transform.position} center={center}");
        }
    }

    Vector3 GetCenter()
    {
        return centerTransform != null ? centerTransform.position : centerPosition;
    }
}
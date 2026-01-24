using UnityEngine;

public class CircularMotion : MonoBehaviour
{
    public Vector3 centerPosition = Vector3.zero; // 旋转中心
    public float radius = 5f; // 旋转半径（Inspector 显示用）
    public float speed = 60f; // 每秒旋转角度（度）

    void Start()
    {
        // 可选：如果初始位置与中心重合，使用 radius 设置一个默认位置
        var offset = transform.position - centerPosition;
        if (offset.sqrMagnitude <= 0.0001f && radius > 0f)
        {
            transform.position = centerPosition + Vector3.right * radius;
        }
    }

    void Update()
    {
        // 基于当前 transform 进行旋转，这样外部写 transform.position 的操作（Pigeon）不会被完全覆盖
        float angleThisFrame = speed * Time.deltaTime;
        transform.RotateAround(centerPosition, Vector3.forward, angleThisFrame);
    }
}
using UnityEngine;

public class CircularMotion : MonoBehaviour
{
    public Vector3 centerPosition = Vector3.zero; // 旋转中心点
    public float radius = 5f; // 旋转半径
    public float speed = 60f; // 每秒旋转的角度

    private Vector3 r; // 从中心点到物体的初始向量

    void Awake()
    {
        r = transform.position - centerPosition; // 初始化向量
    }

    void Update()
    {
        // 每帧旋转向量
        r = Quaternion.AngleAxis(speed * Time.deltaTime, Vector3.forward) * r;
        // 更新物体的位置
        transform.position = centerPosition + r;
    }
}
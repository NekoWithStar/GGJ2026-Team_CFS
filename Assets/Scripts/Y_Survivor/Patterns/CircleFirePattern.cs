using UnityEngine;

/// <summary>
/// 环形发射模式 - 向四周均匀发射
/// 支持多发同时发射（例如每个方向发射多个子弹）
/// </summary>
[CreateAssetMenu(menuName = "Fire Pattern/Circle Pattern")]
public class CircleFirePattern : FirePattern
{
    [Header("环形参数")]
    [Tooltip("起始角度偏移（度）")]
    public float startAngleOffset = 0f;
    
    [Tooltip("是否顺时针旋转")]
    public bool clockwise = true;
    
    [Tooltip("是否随时间旋转")]
    public bool rotateOverTime = false;
    
    [Tooltip("旋转速度（度/秒）")]
    public float rotationSpeed = 45f;

    public override void OnFireStart()
    {
        // 无需初始化
    }

    public override Vector2 GetFireDirection(Transform weaponTransform, int shotIndex, int totalShots, float elapsedTime)
    {
        // 计算每发之间的角度间隔
        float angleStep = 360f / Mathf.Max(totalShots, 1);
        float baseAngle = startAngleOffset + (clockwise ? angleStep : -angleStep) * shotIndex;
        
        // 如果启用随时间旋转
        if (rotateOverTime)
        {
            baseAngle += rotationSpeed * elapsedTime;
        }
        
        // 转换为弧度并计算方向向量
        float radians = baseAngle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
    }
}

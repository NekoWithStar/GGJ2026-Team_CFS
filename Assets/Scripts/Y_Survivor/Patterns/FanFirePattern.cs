using UnityEngine;

/// <summary>
/// 扇形发射模式 - 向前方扇形区域发射
/// 支持多发同时发射（例如每个方向同时发射多个子弹形成更密集的弹幕）
/// </summary>
[CreateAssetMenu(menuName = "Fire Pattern/Fan Pattern")]
public class FanFirePattern : FirePattern
{
    [Header("扇形参数")]
    [Tooltip("扇形角度（度）")]
    [Range(0f, 360f)]
    public float spreadAngle = 60f;
    
    [Tooltip("是否从武器朝向开始计算")]
    public bool useWeaponForward = true;
    
    [Tooltip("是否摆动扇形（左右扫射）")]
    public bool oscillate = false;
    
    [Tooltip("摆动周期（秒）")]
    public float oscillationPeriod = 2f;

    public override Vector2 GetFireDirection(Transform weaponTransform, int shotIndex, int totalShots, float elapsedTime)
    {
        // 基础方向
        Vector2 baseDirection = useWeaponForward 
            ? (Vector2)weaponTransform.right 
            : Vector2.right;
        
        // 计算扇形范围内的角度
        float startAngle = -spreadAngle / 2f;
        float angleStep = totalShots > 1 ? spreadAngle / (totalShots - 1) : 0f;
        float angle = startAngle + angleStep * shotIndex;
        
        // 如果启用摆动
        if (oscillate)
        {
            float oscillationAngle = Mathf.Sin(elapsedTime * (2f * Mathf.PI / oscillationPeriod)) * spreadAngle / 2f;
            angle += oscillationAngle;
        }
        
        // 旋转基础方向
        float radians = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        
        Vector2 direction = new Vector2(
            baseDirection.x * cos - baseDirection.y * sin,
            baseDirection.x * sin + baseDirection.y * cos
        );
        
        return direction.normalized;
    }
}

using UnityEngine;

/// <summary>
/// 螺旋发射模式 - 螺旋状发射
/// 支持多发同时发射（例如双螺旋、三螺旋等）
/// 配置说明：
/// - shotsPerCycle: 螺旋完整旋转需要的发射次数
/// - bulletsPerShot: 同时发射的螺旋数量（1=单螺旋，2=双螺旋，3=三螺旋）
/// - multiShotPattern: 多螺旋时自动使用 Custom 模式
/// </summary>
[CreateAssetMenu(menuName = "Fire Pattern/Spiral Pattern")]
public class SpiralFirePattern : FirePattern
{
    [Header("螺旋参数")]
    [Tooltip("每发的角度增量")]
    public float anglePerShot = 30f;
    
    [Tooltip("螺旋半径增长速度")]
    public float radiusGrowth = 0.1f;
    
    [Tooltip("起始半径")]
    public float startRadius = 0f;
    
    [Tooltip("最大半径（0=无限制）")]
    public float maxRadius = 0f;
    
    [Tooltip("是否顺时针")]
    public bool clockwise = true;

    public override void OnFireStart()
    {
        // 仅初始化效果参数，不修改射击配置
    }

    public override Vector2 GetFireDirection(Transform weaponTransform, int shotIndex, int totalShots, float elapsedTime)
    {
        float angle = (clockwise ? anglePerShot : -anglePerShot) * shotIndex;
        float radians = angle * Mathf.Deg2Rad;
        
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
    }

    public override Vector2 GetPositionOffset(Transform weaponTransform, int shotIndex, int totalShots, float elapsedTime)
    {
        // 根据索引增加螺旋半径
        float radius = startRadius + radiusGrowth * shotIndex;
        if (maxRadius > 0f)
        {
            radius = Mathf.Min(radius, maxRadius);
        }
        
        float angle = (clockwise ? anglePerShot : -anglePerShot) * shotIndex;
        float radians = angle * Mathf.Deg2Rad;
        
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;
    }
    
    protected override Vector2[] GetCustomMultiShotDirections(Vector2 mainDirection, int shotIndex)
    {
        // 多重螺旋：使用基类的 bulletsPerShot
        // 每个螺旋之间相差 360/bulletsPerShot 度
        Vector2[] directions = new Vector2[bulletsPerShot];
        float angleOffset = 360f / bulletsPerShot;
        
        for (int i = 0; i < bulletsPerShot; i++)
        {
            float angle = Mathf.Atan2(mainDirection.y, mainDirection.x) * Mathf.Rad2Deg;
            angle += angleOffset * i;
            float radians = angle * Mathf.Deg2Rad;
            directions[i] = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
        }
        
        return directions;
    }
}

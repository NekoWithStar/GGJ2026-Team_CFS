using UnityEngine;

/// <summary>
/// 发射模式基类 - 用于持续自动开火武器的发射控制
/// 子类可以实现不同的发射模式（环形、螺旋、扇形等）
/// 支持单次发射多个子弹（用于散弹、多方向齐射等）
/// </summary>
public abstract class FirePattern : ScriptableObject
{
    [Header("基础发射参数")]
    [Tooltip("每次开火周期内的总发射数量")]
    public int shotsPerCycle = 10;
    
    [Header("多发同时发射")]
    [Tooltip("单次发射的子弹数量（1=单发，>1=多发齐射）")]
    public int bulletsPerShot = 1;
    
    [Tooltip("多发子弹的间距角度（度）- 仅当bulletsPerShot>1时生效")]
    public float bulletSpreadAngle = 10f;
    
    [Tooltip("多发子弹排列方式")]
    public MultiShotPattern multiShotPattern = MultiShotPattern.Spread;
    
    public enum MultiShotPattern
    {
        Spread,      // 扩散：以主方向为中心，向两侧扩散
        Cross,       // 十字：上下左右四个方向
        Circle,      // 环形：均匀分布在360度
        Line,        // 直线：沿主方向排成一线
        Custom       // 自定义：由子类实现GetMultiShotDirections
    }

    /// <summary>
    /// 获取下一次发射的主方向（归一化向量）
    /// </summary>
    /// <param name="weaponTransform">武器的Transform</param>
    /// <param name="shotIndex">当前是第几发（从0开始）</param>
    /// <param name="totalShots">总共要发射多少发</param>
    /// <param name="elapsedTime">从开始发射到现在经过的时间</param>
    /// <returns>发射主方向（归一化）</returns>
    public abstract Vector2 GetFireDirection(Transform weaponTransform, int shotIndex, int totalShots, float elapsedTime);
    
    /// <summary>
    /// 获取下一次发射的位置偏移（相对于muzzlePoint）
    /// </summary>
    /// <param name="weaponTransform">武器的Transform</param>
    /// <param name="shotIndex">当前是第几发</param>
    /// <param name="totalShots">总共要发射多少发</param>
    /// <param name="elapsedTime">从开始发射到现在经过的时间</param>
    /// <returns>位置偏移量</returns>
    public virtual Vector2 GetPositionOffset(Transform weaponTransform, int shotIndex, int totalShots, float elapsedTime)
    {
        return Vector2.zero; // 默认无偏移
    }
    
    /// <summary>
    /// 获取多发子弹的所有发射方向
    /// </summary>
    /// <param name="mainDirection">主发射方向</param>
    /// <param name="shotIndex">当前发射索引</param>
    /// <returns>所有子弹的发射方向数组</returns>
    public virtual Vector2[] GetMultiShotDirections(Vector2 mainDirection, int shotIndex)
    {
        if (bulletsPerShot <= 1)
        {
            return new Vector2[] { mainDirection };
        }
        
        Vector2[] directions = new Vector2[bulletsPerShot];
        
        switch (multiShotPattern)
        {
            case MultiShotPattern.Spread:
                // 扩散模式：以主方向为中心向两侧扩散
                float startAngle = -bulletSpreadAngle * (bulletsPerShot - 1) / 2f;
                for (int i = 0; i < bulletsPerShot; i++)
                {
                    float angle = startAngle + bulletSpreadAngle * i;
                    directions[i] = RotateVector(mainDirection, angle);
                }
                break;
                
            case MultiShotPattern.Cross:
                // 十字模式：上下左右（最多4发）
                int crossCount = Mathf.Min(bulletsPerShot, 4);
                for (int i = 0; i < crossCount; i++)
                {
                    float angle = 90f * i;
                    directions[i] = RotateVector(Vector2.right, angle);
                }
                // 如果超过4发，多余的随机分布
                for (int i = crossCount; i < bulletsPerShot; i++)
                {
                    float angle = Random.Range(0f, 360f);
                    directions[i] = RotateVector(Vector2.right, angle);
                }
                break;
                
            case MultiShotPattern.Circle:
                // 环形模式：360度均匀分布
                float angleStep = 360f / bulletsPerShot;
                for (int i = 0; i < bulletsPerShot; i++)
                {
                    float angle = angleStep * i;
                    directions[i] = RotateVector(Vector2.right, angle);
                }
                break;
                
            case MultiShotPattern.Line:
                // 直线模式：沿主方向排成一线（通过位置偏移实现，方向相同）
                for (int i = 0; i < bulletsPerShot; i++)
                {
                    directions[i] = mainDirection;
                }
                break;
                
            case MultiShotPattern.Custom:
                // 自定义模式：由子类重写此方法
                directions = GetCustomMultiShotDirections(mainDirection, shotIndex);
                break;
        }
        
        return directions;
    }
    
    /// <summary>
    /// 获取多发子弹的位置偏移（用于Line等模式）
    /// </summary>
    /// <param name="mainDirection">主发射方向</param>
    /// <param name="bulletIndex">当前是第几发子弹</param>
    /// <param name="totalBullets">总共几发子弹</param>
    /// <returns>位置偏移</returns>
    public virtual Vector2 GetMultiShotPositionOffset(Vector2 mainDirection, int bulletIndex, int totalBullets)
    {
        if (multiShotPattern == MultiShotPattern.Line)
        {
            // 直线模式：垂直于发射方向排列
            Vector2 perpendicular = new Vector2(-mainDirection.y, mainDirection.x);
            float spacing = 0.3f; // 子弹间距
            float offset = (bulletIndex - (totalBullets - 1) / 2f) * spacing;
            return perpendicular * offset;
        }
        
        return Vector2.zero;
    }
    
    /// <summary>
    /// 自定义多发模式（子类可重写）
    /// </summary>
    protected virtual Vector2[] GetCustomMultiShotDirections(Vector2 mainDirection, int shotIndex)
    {
        // 默认回退到扩散模式
        Vector2[] directions = new Vector2[bulletsPerShot];
        float startAngle = -bulletSpreadAngle * (bulletsPerShot - 1) / 2f;
        for (int i = 0; i < bulletsPerShot; i++)
        {
            float angle = startAngle + bulletSpreadAngle * i;
            directions[i] = RotateVector(mainDirection, angle);
        }
        return directions;
    }
    
    /// <summary>
    /// 旋转向量
    /// </summary>
    protected Vector2 RotateVector(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
    
    /// <summary>
    /// 是否需要等待一段时间再发射下一发
    /// </summary>
    /// <param name="shotIndex">当前发射索引</param>
    /// <returns>需要等待的时间（秒），0表示立即发射</returns>
    public virtual float GetDelayBeforeNextShot(int shotIndex)
    {
        return 0f; // 默认无延迟，由attackRate或customInterval控制
    }
    
    /// <summary>
    /// 在发射开始时调用（可用于重置状态）
    /// </summary>
    public virtual void OnFireStart()
    {
        // 子类可以重写以重置内部状态
    }
    
    /// <summary>
    /// 在发射结束时调用
    /// </summary>
    public virtual void OnFireEnd()
    {
        // 子类可以重写以执行清理
    }
}

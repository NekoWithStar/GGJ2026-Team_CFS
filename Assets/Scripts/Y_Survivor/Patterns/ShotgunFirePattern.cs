using UnityEngine;

/// <summary>
/// 散弹发射模式 - 一次发射多发子弹形成散射效果
/// 专门用于霰弹枪类武器
/// 配置说明：
/// - shotsPerCycle: 建议设为 1（散弹枪通常只发射一次然后冷却）
/// - bulletsPerShot: 每次发射的弹丸数量（替代旧的 pelletsPerShot）
/// - multiShotPattern: 自动使用 Custom 模式
/// </summary>
[CreateAssetMenu(menuName = "Fire Pattern/Shotgun Pattern")]
public class ShotgunFirePattern : FirePattern
{
    [Header("散弹参数")]
    [Tooltip("散射角度（度）")]
    [Range(0f, 180f)]
    public float shotgunSpread = 30f;
    
    [Tooltip("散射模式")]
    public ShotgunSpreadMode spreadMode = ShotgunSpreadMode.Random;
    
    [Tooltip("是否使用武器朝向")]
    public bool useWeaponForward = true;
    
    public enum ShotgunSpreadMode
    {
        Random,      // 随机散射
        Uniform,     // 均匀分布
        Cone         // 锥形（中心密集）
    }

    public override void OnFireStart()
    {
        // 仅初始化效果参数，不修改射击配置
    }

    public override Vector2 GetFireDirection(Transform weaponTransform, int shotIndex, int totalShots, float elapsedTime)
    {
        // 主方向
        return useWeaponForward 
            ? (Vector2)weaponTransform.right 
            : Vector2.right;
    }
    
    protected override Vector2[] GetCustomMultiShotDirections(Vector2 mainDirection, int shotIndex)
    {
        // 使用基类的 bulletsPerShot 而非自定义的 pelletsPerShot
        Vector2[] directions = new Vector2[bulletsPerShot];
        
        switch (spreadMode)
        {
            case ShotgunSpreadMode.Random:
                // 随机散射
                for (int i = 0; i < bulletsPerShot; i++)
                {
                    float randomAngle = Random.Range(-shotgunSpread / 2f, shotgunSpread / 2f);
                    directions[i] = RotateVector(mainDirection, randomAngle);
                }
                break;
                
            case ShotgunSpreadMode.Uniform:
                // 均匀分布
                float startAngle = -shotgunSpread / 2f;
                float angleStep = bulletsPerShot > 1 ? shotgunSpread / (bulletsPerShot - 1) : 0f;
                for (int i = 0; i < bulletsPerShot; i++)
                {
                    float angle = startAngle + angleStep * i;
                    directions[i] = RotateVector(mainDirection, angle);
                }
                break;
                
            case ShotgunSpreadMode.Cone:
                // 锥形分布（中心密集，边缘稀疏）
                for (int i = 0; i < bulletsPerShot; i++)
                {
                    // 使用平方根分布使弹丸更集中在中心
                    float t = i / (float)(bulletsPerShot - 1);
                    float spread = Mathf.Sqrt(t) * shotgunSpread - shotgunSpread / 2f;
                    // 添加随机性
                    spread += Random.Range(-shotgunSpread * 0.1f, shotgunSpread * 0.1f);
                    directions[i] = RotateVector(mainDirection, spread);
                }
                break;
        }
        
        return directions;
    }
}

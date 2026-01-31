using UnityEngine;

/// <summary>
/// 波浪发射模式 - 交替向两侧发射形成波浪效果
/// 支持多发同时发射
/// </summary>
[CreateAssetMenu(menuName = "Fire Pattern/Wave Pattern")]
public class WaveFirePattern : FirePattern
{
    [Header("波浪参数")]
    [Tooltip("波浪幅度（度）")]
    [Range(0f, 90f)]
    public float waveAmplitude = 30f;
    
    [Tooltip("波浪频率（每秒几个波）")]
    public float waveFrequency = 2f;
    
    [Tooltip("是否使用武器朝向")]
    public bool useWeaponForward = true;
    
    [Tooltip("波浪类型")]
    public WaveType waveType = WaveType.Sine;
    
    public enum WaveType
    {
        Sine,        // 正弦波
        Triangle,    // 三角波
        Square       // 方波
    }

    public override Vector2 GetFireDirection(Transform weaponTransform, int shotIndex, int totalShots, float elapsedTime)
    {
        // 基础方向
        Vector2 baseDirection = useWeaponForward 
            ? (Vector2)weaponTransform.right 
            : Vector2.right;
        
        // 计算波浪角度
        float waveAngle = 0f;
        float phase = elapsedTime * waveFrequency * 2f * Mathf.PI;
        
        switch (waveType)
        {
            case WaveType.Sine:
                waveAngle = Mathf.Sin(phase) * waveAmplitude;
                break;
                
            case WaveType.Triangle:
                // 三角波
                float t = (phase / (2f * Mathf.PI)) % 1f;
                waveAngle = (t < 0.5f ? t * 4f - 1f : 3f - t * 4f) * waveAmplitude;
                break;
                
            case WaveType.Square:
                // 方波
                waveAngle = (Mathf.Sin(phase) >= 0 ? 1f : -1f) * waveAmplitude;
                break;
        }
        
        // 旋转基础方向
        float radians = waveAngle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        
        Vector2 direction = new Vector2(
            baseDirection.x * cos - baseDirection.y * sin,
            baseDirection.x * sin + baseDirection.y * cos
        );
        
        return direction.normalized;
    }
}

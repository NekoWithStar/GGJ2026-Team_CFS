using UnityEngine;

/// <summary>
/// 简单伤害承载器：将该组件放到武器命中判定体或一次性命中特效上，便于受击对象读取伤害值。
/// 使用建议：
/// - 近战武器：在武器的 Hitbox（Trigger）上挂该组件并设置 damage；武器在触发时启用该 Hitbox。
/// - 远程子弹：子弹预制体携带该组件并在命中后销毁（destroyOnHit = true）。
/// </summary>
public class DamageDealer : MonoBehaviour
{
    [Header("基础伤害")]
    [Tooltip("基础伤害值")]
    public float damage = 10;

    [Tooltip("命中后是否销毁自己（子弹/一次性判定常用）")]
    public bool destroyOnHit = true;

    [Tooltip("伤害来源（可用于避免自伤），可在生成时设置为发起者 GameObject")]
    public GameObject owner;
    
    [Header("暴击系统")]
    [Tooltip("暴击率（0~1，0=0%，1=100%）")]
    [Range(0f, 1f)]
    public float critChance = 0f;
    
    [Tooltip("暴击伤害倍率（1.5 = 150%伤害）")]
    public float critDamageMultiplier = 1.5f;
    
    [Tooltip("本次攻击是否暴击（运行时计算）")]
    [HideInInspector]
    public bool isCritical = false;
    
    /// <summary>
    /// 初始化时计算是否暴击
    /// </summary>
    private void Awake()
    {
        RollCritical();
    }
    
    /// <summary>
    /// 根据暴击率计算本次攻击是否暴击
    /// </summary>
    public void RollCritical()
    {
        isCritical = Random.value < critChance;
    }
    
    /// <summary>
    /// 获取最终伤害值（考虑暴击）
    /// </summary>
    /// <returns>最终伤害值</returns>
    public int GetFinalDamage()
    {
        if (isCritical)
        {
            return Mathf.RoundToInt(damage * critDamageMultiplier);
        }
        return Mathf.RoundToInt(damage);
    }
    
    /// <summary>
    /// 设置完整的伤害参数
    /// </summary>
    /// <param name="baseDamage">基础伤害</param>
    /// <param name="critRate">暴击率</param>
    /// <param name="critMultiplier">暴击伤害倍率</param>
    /// <param name="damageOwner">伤害来源</param>
    public void Setup(int baseDamage, float critRate, float critMultiplier, GameObject damageOwner)
    {
        damage = baseDamage;
        critChance = critRate;
        critDamageMultiplier = critMultiplier;
        owner = damageOwner;
        RollCritical();
    }
}
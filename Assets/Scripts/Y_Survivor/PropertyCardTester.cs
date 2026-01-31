using UnityEngine;
using Y_Survivor;

/// <summary>
/// 属性卡测试器 - 用于在编辑器和运行时快速测试属性卡效果
/// 使用说明：
/// 1. 将此脚本挂载到场景中的空对象上
/// 2. 配置 Test Card、Target Weapon 等引用
/// 3. 运行游戏后按数字键测试：
///    - 按 1 应用卡片
///    - 按 2 移除卡片
///    - 按 3 切换武器（验证加成保持）
/// </summary>
public class PropertyCardTester : MonoBehaviour
{
    [Header("测试配置")]
    [Tooltip("要测试的属性卡")]
    public PropertyCard testCard;
    
    [Tooltip("目标武器控制器")]
    public WeaponControl targetWeapon;
    
    [Tooltip("目标玩家管理器（可选）")]
    public PlayerPropertyManager playerManager;
    
    [Header("测试操作")]
    [Tooltip("按下此键应用卡片")]
    public KeyCode applyKey = KeyCode.Alpha1;
    
    [Tooltip("按下此键移除卡片")]
    public KeyCode removeKey = KeyCode.Alpha2;
    
    [Tooltip("按下此键切换武器")]
    public KeyCode switchWeaponKey = KeyCode.Alpha3;
    
    [Tooltip("切换到的新武器数据")]
    public Weapon newWeaponData;
    
    [Header("运行时信息")]
    [Tooltip("显示当前已应用的卡片数量")]
    public int appliedCardsCount = 0;
    
    void Start()
    {
        Debug.Log("=== PropertyCardTester Ready ===");
        Debug.Log($"Press [{applyKey}] to apply card");
        Debug.Log($"Press [{removeKey}] to remove card");
        Debug.Log($"Press [{switchWeaponKey}] to switch weapon");
        
        if (targetWeapon != null)
        {
            LogCurrentStats();
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(applyKey))
        {
            ApplyCard();
        }
        
        if (Input.GetKeyDown(removeKey))
        {
            RemoveCard();
        }
        
        if (Input.GetKeyDown(switchWeaponKey))
        {
            SwitchWeapon();
        }
    }
    
    /// <summary>
    /// 应用测试卡片
    /// </summary>
    void ApplyCard()
    {
        if (testCard == null)
        {
            Debug.LogWarning("⚠️ Test card is not assigned!");
            return;
        }
        
        bool applied = false;
        
        // 判断卡片类型并应用到对应管理器
        foreach (var modifier in testCard.modifiers)
        {
            if (IsWeaponProperty(modifier.targetProperty))
            {
                if (targetWeapon != null && targetWeapon.propertyManager != null)
                {
                    targetWeapon.propertyManager.ApplyPropertyCard(testCard);
                    Debug.Log($"✅ Applied card '{testCard.cardName}' to weapon");
                    appliedCardsCount++;
                    LogCurrentStats();
                    applied = true;
                    break;
                }
                else
                {
                    Debug.LogError("❌ Target weapon or its PropertyManager is not assigned!");
                }
            }
            else if (IsPlayerProperty(modifier.targetProperty))
            {
                if (playerManager != null)
                {
                    playerManager.ApplyPropertyCard(testCard);
                    Debug.Log($"✅ Applied card '{testCard.cardName}' to player");
                    appliedCardsCount++;
                    applied = true;
                    break;
                }
                else
                {
                    Debug.LogWarning("⚠️ Player PropertyManager is not assigned!");
                }
            }
            else if (IsEnemyProperty(modifier.targetProperty))
            {
                if (EnemyPropertyManager.Instance != null)
                {
                    EnemyPropertyManager.Instance.ApplyPropertyCard(testCard);
                    Debug.Log($"✅ Applied card '{testCard.cardName}' to enemies");
                    appliedCardsCount++;
                    applied = true;
                    break;
                }
                else
                {
                    Debug.LogWarning("⚠️ EnemyPropertyManager instance not found in scene!");
                }
            }
        }
        
        if (!applied)
        {
            Debug.LogWarning("⚠️ Card has no valid modifiers or no target found!");
        }
    }
    
    /// <summary>
    /// 移除测试卡片
    /// </summary>
    void RemoveCard()
    {
        if (testCard == null)
        {
            Debug.LogWarning("⚠️ Test card is not assigned!");
            return;
        }
        
        bool removed = false;
        
        // 尝试从所有管理器中移除
        if (targetWeapon != null && targetWeapon.propertyManager != null)
        {
            targetWeapon.propertyManager.RemovePropertyCard(testCard);
            Debug.Log($"❌ Removed card '{testCard.cardName}' from weapon");
            appliedCardsCount = Mathf.Max(0, appliedCardsCount - 1);
            LogCurrentStats();
            removed = true;
        }
        
        if (playerManager != null)
        {
            playerManager.RemovePropertyCard(testCard);
            removed = true;
        }
        
        if (EnemyPropertyManager.Instance != null)
        {
            EnemyPropertyManager.Instance.RemovePropertyCard(testCard);
            removed = true;
        }
        
        if (!removed)
        {
            Debug.LogWarning("⚠️ No property manager found to remove card from!");
        }
    }
    
    /// <summary>
    /// 切换武器（验证属性加成是否保持）
    /// </summary>
    void SwitchWeapon()
    {
        if (newWeaponData == null || targetWeapon == null)
        {
            Debug.LogWarning("⚠️ New weapon data or target weapon is not assigned!");
            return;
        }
        
        string oldWeaponName = targetWeapon.weaponData != null ? targetWeapon.weaponData.weaponName : "Unknown";
        
        Debug.Log($"🔄 Switching weapon from '{oldWeaponName}' to '{newWeaponData.weaponName}'");
        Debug.Log($"⚠️ Note: All applied cards ({appliedCardsCount}) will remain active!");
        
        targetWeapon.SetWeaponData(newWeaponData);
        
        LogCurrentStats();
    }
    
    /// <summary>
    /// 输出当前武器属性
    /// </summary>
    void LogCurrentStats()
    {
        if (targetWeapon == null || targetWeapon.propertyManager == null)
        {
            Debug.LogWarning("⚠️ Cannot log stats: weapon or property manager not available");
            return;
        }
        
        var pm = targetWeapon.propertyManager;
        
        Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log($"📊 Current Weapon: {targetWeapon.weaponData.weaponName}");
        Debug.Log($"📊 Applied Cards: {appliedCardsCount}");
        Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log($"  💥 Damage: {pm.GetDamage()}");
        Debug.Log($"  ⚡ Attack Rate: {pm.GetAttackRate():F2}/s");
        Debug.Log($"  🎯 Crit Chance: {pm.GetCritChance() * 100:F1}%");
        Debug.Log($"  💢 Crit Multiplier: {pm.GetCritDamageMultiplier():F2}x");
        Debug.Log($"  ⏱️ Cooldown: {pm.GetCooldown():F2}s");
        Debug.Log($"  ⏳ Charging Time: {pm.GetChargingTime():F2}s");
        Debug.Log($"  🔥 Continuous Fire: {pm.GetContinuousFireDuration():F2}s");
        Debug.Log($"  ⚔️ Melee Range: {pm.GetMeleeRange():F2}");
        Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }
    
    /// <summary>
    /// 判断是否为武器属性
    /// </summary>
    bool IsWeaponProperty(PropertyType type)
    {
        return (int)type >= 100 && (int)type < 200;
    }
    
    /// <summary>
    /// 判断是否为玩家属性
    /// </summary>
    bool IsPlayerProperty(PropertyType type)
    {
        return (int)type >= 200 && (int)type < 300;
    }
    
    /// <summary>
    /// 判断是否为敌人属性
    /// </summary>
    bool IsEnemyProperty(PropertyType type)
    {
        return (int)type >= 300 && (int)type < 400;
    }
}

using UnityEngine;
using Y_Survivor;
using EasyPack.GamePropertySystem;
using EasyPack.Modifiers;
using System.Collections.Generic;
 
/// <summary>
/// 武器系统调试工具 - 实时查看和修改武器数据、属性
/// 可用于快速测试属性卡效果和武器切换
/// </summary>
public class WeaponDebugger : MonoBehaviour
{
    [System.Serializable]
    public class ModifierInfo
    {
        public string propertyName;
        public string modifierType;
        public float value;
        public int priority;
    }

    [SerializeField]
    private PlayerControl playerControl;

    [Header("当前武器数据（只读）")]
    [SerializeField]
    private string currentWeaponName = "无武器";
    
    [SerializeField]
    private string weaponType = "N/A";
    
    [SerializeField]
    private string weaponState = "N/A";

    [Header("当前属性数据（只读）")]
    [SerializeField]
    private int currentDamage;
    
    [SerializeField]
    private float currentAttackRate;
    
    [SerializeField]
    private float currentCooldown;
    
    [SerializeField]
    private float currentChargingTime;
    
    [SerializeField]
    private float currentMeleeRange;
    
    [SerializeField]
    private float currentCritChance;
    
    [SerializeField]
    private float currentCritDamage;

    [Header("玩家属性数据（只读）")]
    [SerializeField]
    private float playerMoveSpeed;
    
    [SerializeField]
    private float playerCurrentHealth;
    
    [SerializeField]
    private float playerMaxHealth;

    [Header("敌人属性数据（只读）")]
    [SerializeField]
    private float smallEnemyMoveSpeed;
    
    [SerializeField]
    private float mediumEnemyMoveSpeed;
    
    [SerializeField]
    private float largeEnemyMoveSpeed;

    [Header("当前修饰符列表（只读）")]
    [SerializeField]
    private List<ModifierInfo> currentModifiers = new List<ModifierInfo>();

    [Header("快速武器切换")]
    [SerializeField]
    private Weapon newWeaponData;

    [Header("属性修饰符调试")]
    [SerializeField]
    private EasyPack.Modifiers.ModifierType modifyType = EasyPack.Modifiers.ModifierType.Add;
    
    [SerializeField]
    private PropertyType propertyType = PropertyType.Damage;
    
    [SerializeField]
    private float modifierValue = 10f;

    // 属性类型枚举（用于编辑器选择）


    private void OnEnable()
    {
        if (playerControl == null)
        {
            playerControl = FindAnyObjectByType<PlayerControl>();
        }
    }

    /// <summary>
    /// 刷新数据显示（从Update或手动调用）
    /// </summary>
    public void RefreshDisplay()
    {
        if (playerControl == null || playerControl.ExternalWeaponInstance == null)
        {
            currentWeaponName = "无武器";
            weaponType = "N/A";
            weaponState = "N/A";
            currentModifiers.Clear();
            return;
        }

        var wc = playerControl.ExternalWeaponInstance.GetComponentInChildren<WeaponControl>();
        if (wc == null || wc.weaponData == null)
        {
            currentWeaponName = "WeaponControl未找到";
            currentModifiers.Clear();
            return;
        }

        // 更新武器信息
        currentWeaponName = wc.weaponData.weaponName;
        weaponType = wc.weaponData.weaponType.ToString();
        weaponState = wc.GetCurrentState().ToString();

        // 更新属性数据（通过PropertyManager或直接使用weaponData）
        if (wc.propertyManager != null)
        {
            currentDamage = wc.propertyManager.GetDamage();
            currentAttackRate = wc.propertyManager.GetAttackRate();
            currentCooldown = wc.propertyManager.GetCooldown();
            currentChargingTime = wc.propertyManager.GetChargingTime();
            currentMeleeRange = wc.propertyManager.GetMeleeRange();
            currentCritChance = wc.propertyManager.GetCritChance();
            currentCritDamage = wc.propertyManager.GetCritDamageMultiplier();
            
            // 更新修饰符列表
            UpdateModifiersList(wc.propertyManager);
        }
        else
        {
            currentDamage = wc.weaponData.damage;
            currentAttackRate = wc.weaponData.attackRate;
            currentCooldown = wc.weaponData.cooldown;
            currentChargingTime = wc.weaponData.chargingTime;
            currentMeleeRange = wc.weaponData.meleeRange;
            currentCritChance = wc.weaponData.critChanceBase;
            currentCritDamage = wc.weaponData.critDamageBase;
            currentModifiers.Clear();
        }
        
        // 更新玩家属性
        var playerPropMgr = playerControl.GetComponent<PlayerPropertyManager>();
        if (playerPropMgr != null)
        {
            playerMoveSpeed = playerPropMgr.GetMoveSpeed();
            playerCurrentHealth = playerPropMgr.GetCurrentHealth();
            //playerMaxHealth = playerPropMgr.GetMaxHealth();
        }
        
        // 更新敌人属性
        var enemyPropMgr = EnemyPropertyManager.Instance;
        if (enemyPropMgr != null)
        {
            smallEnemyMoveSpeed = enemyPropMgr.GetSmallEnemySpeed();
            mediumEnemyMoveSpeed = enemyPropMgr.GetMediumEnemySpeed();
            largeEnemyMoveSpeed = enemyPropMgr.GetLargeEnemySpeed();
        }
    }

    /// <summary>
    /// 更新当前应用的修饰符列表
    /// </summary>
    private void UpdateModifiersList(WeaponPropertyManager propertyManager)
    {
        currentModifiers.Clear();
        
        // 从appliedCards中提取所有修饰符
        foreach (var cardEntry in propertyManager.appliedCards)
        {
            var card = cardEntry.Key;
            var modifiers = cardEntry.Value;
            
            foreach (var (propType, modifier) in modifiers)
            {
                // 获取属性名称
                string propertyName = propType switch
                {
                    PropertyType.Damage => "伤害",
                    PropertyType.AttackRate => "攻击速率",
                    PropertyType.Cooldown => "冷却",
                    PropertyType.ChargingTime => "蓄力时间",
                    PropertyType.MeleeAttackRange => "近战范围",
                    PropertyType.CritChance => "暴击率",
                    PropertyType.CritDamageMultiplier => "暴击伤害",
                    PropertyType.ContinuousFireDuration => "持续开火时间",
                    _ => "未知属性"
                };
                
                currentModifiers.Add(new ModifierInfo
                {
                    propertyName = $"{propertyName} [来自 {card.cardName}]",
                    modifierType = modifier.Type.ToString(),
                    value = modifier is FloatModifier fm ? fm.Value : 0f,
                    priority = modifier.Priority
                });
            }
        }
    }

    /// <summary>
    /// 立即切换武器
    /// </summary>
    public void SwitchWeapon()
    {
        if (playerControl == null)
        {
            Debug.LogError("[WeaponDebugger] PlayerControl 未设置！");
            return;
        }

        if (newWeaponData == null)
        {
            Debug.LogError("[WeaponDebugger] 新武器数据为 null！请在 Inspector 中指定");
            return;
        }

        // 检查是否已有武器
        if (playerControl.ExternalWeaponInstance != null)
        {
            // 已有武器：使用 SwitchWeaponData
            bool success = playerControl.SwitchWeaponData(newWeaponData);
            if (success)
            {
                Debug.Log($"[WeaponDebugger] ✅ 武器已切换至: {newWeaponData.weaponName}");
                RefreshDisplay();
            }
        }
        else
        {
            // 首次装备
            if (newWeaponData.weaponPrefab != null)
            {
                playerControl.EquipExternalWeapon(newWeaponData.weaponPrefab, newWeaponData);
                Debug.Log($"[WeaponDebugger] ✅ 武器已装备: {newWeaponData.weaponName}");
                RefreshDisplay();
            }
            else
            {
                Debug.LogError("[WeaponDebugger] 武器的 weaponPrefab 为 null！");
            }
        }
    }

    /// <summary>
    /// 添加属性修饰符
    /// </summary>
    public void AddModifier()
    {
        if (playerControl == null || playerControl.ExternalWeaponInstance == null)
        {
            Debug.LogError("[WeaponDebugger] 未找到武器实例！");
            return;
        }

        var wc = playerControl.ExternalWeaponInstance.GetComponentInChildren<WeaponControl>();
        if (wc == null || wc.propertyManager == null)
        {
            Debug.LogError("[WeaponDebugger] 武器未配置 PropertyManager！");
            return;
        }

        // 根据选择的属性类型，获取对应的 GameProperty 并添加修饰符
        GameProperty property = null;
        string propertyName = "";

        switch (propertyType)
        {
            // ===== 武器属性 =====
            case PropertyType.Damage:
                property = wc.propertyManager.Damage;
                propertyName = "伤害";
                break;
            case PropertyType.AttackRate:
                property = wc.propertyManager.AttackRate;
                propertyName = "攻击速率";
                break;
            case PropertyType.Cooldown:
                property = wc.propertyManager.Cooldown;
                propertyName = "冷却";
                break;
            case PropertyType.ChargingTime:
                property = wc.propertyManager.ChargingTime;
                propertyName = "蓄力时间";
                break;
            case PropertyType.MeleeAttackRange:
                property = wc.propertyManager.MeleeAttackRange;
                propertyName = "近战范围";
                break;
            case PropertyType.CritChance:
                property = wc.propertyManager.CritChance;
                propertyName = "暴击率";
                break;
            case PropertyType.CritDamageMultiplier:
                property = wc.propertyManager.CritDamageMultiplier;
                propertyName = "暴击伤害";
                break;
            case PropertyType.ContinuousFireDuration:
                property = wc.propertyManager.ContinuousFireDuration;
                propertyName = "持续开火时间";
                break;
            
            // ===== 玩家属性 =====
            case PropertyType.PlayerMoveSpeed:
                {
                    var playerPropMgr = playerControl.GetComponent<PlayerPropertyManager>();
                    if (playerPropMgr != null)
                    {
                        property = playerPropMgr.MoveSpeed;
                        propertyName = "玩家移动速度";
                    }
                    else
                    {
                        Debug.LogError("[WeaponDebugger] 玩家未配置 PlayerPropertyManager！");
                        return;
                    }
                }
                break;
            case PropertyType.PlayerHealth:
                {
                    var playerPropMgr = playerControl.GetComponent<PlayerPropertyManager>();
                    if (playerPropMgr != null)
                    {
                        property = playerPropMgr.CurrentHealth;
                        propertyName = "玩家当前血量";
                    }
                    else
                    {
                        Debug.LogError("[WeaponDebugger] 玩家未配置 PlayerPropertyManager！");
                        return;
                    }
                }
                break;
            case PropertyType.PlayerMaxHealth:
                {
                    var playerPropMgr = playerControl.GetComponent<PlayerPropertyManager>();
                    if (playerPropMgr != null)
                    {
                        property = playerPropMgr.MaxHealth;
                        propertyName = "玩家最大血量";
                    }
                    else
                    {
                        Debug.LogError("[WeaponDebugger] 玩家未配置 PlayerPropertyManager！");
                        return;
                    }
                }
                break;
            
            // ===== 敌人属性 =====
            case PropertyType.SmallEnemyMoveSpeed:
                {
                    var enemyPropMgr = EnemyPropertyManager.Instance;
                    if (enemyPropMgr != null)
                    {
                        property = enemyPropMgr.SmallEnemyMoveSpeed;
                        propertyName = "小怪移动速度";
                    }
                    else
                    {
                        Debug.LogError("[WeaponDebugger] 场景中未找到 EnemyPropertyManager！");
                        return;
                    }
                }
                break;
            case PropertyType.MediumEnemyMoveSpeed:
                {
                    var enemyPropMgr = EnemyPropertyManager.Instance;
                    if (enemyPropMgr != null)
                    {
                        property = enemyPropMgr.MediumEnemyMoveSpeed;
                        propertyName = "中怪移动速度";
                    }
                    else
                    {
                        Debug.LogError("[WeaponDebugger] 场景中未找到 EnemyPropertyManager！");
                        return;
                    }
                }
                break;
            case PropertyType.LargeEnemyMoveSpeed:
                {
                    var enemyPropMgr = EnemyPropertyManager.Instance;
                    if (enemyPropMgr != null)
                    {
                        property = enemyPropMgr.LargeEnemyMoveSpeed;
                        propertyName = "大怪移动速度";
                    }
                    else
                    {
                        Debug.LogError("[WeaponDebugger] 场景中未找到 EnemyPropertyManager！");
                        return;
                    }
                }
                break;
            
            default:
                Debug.LogError($"[WeaponDebugger] ❌ 未知的属性类型: {propertyType}");
                return;
        }

        if (property != null)
        {
            // 创建 FloatModifier 实例并添加到属性
            var modifier = new FloatModifier(modifyType, 0, modifierValue);
            property.AddModifier(modifier);
            Debug.Log($"[WeaponDebugger] ✅ 已添加修饰符: {propertyName} {modifyType} {modifierValue}");
            RefreshDisplay();
        }
        else
        {
            Debug.LogError($"[WeaponDebugger] ❌ 属性 {propertyType} 获取失败！");
        }
    }

    /// <summary>
    /// 确认修改 - 刷新显示并记录当前状态
    /// </summary>
    public void ConfirmModifications()
    {
        RefreshDisplay();
        Debug.Log("[WeaponDebugger] ✅ 修改已确认，属性数据已更新");
    }

    /// <summary>
    /// 刷新修饰符列表显示
    /// </summary>
    public void RefreshModifiersList()
    {
        if (playerControl == null || playerControl.ExternalWeaponInstance == null)
        {
            Debug.LogError("[WeaponDebugger] 未找到武器实例！");
            return;
        }

        var wc = playerControl.ExternalWeaponInstance.GetComponentInChildren<WeaponControl>();
        if (wc == null || wc.propertyManager == null)
        {
            Debug.LogError("[WeaponDebugger] 武器未配置 PropertyManager！");
            return;
        }

        UpdateModifiersList(wc.propertyManager);
        Debug.Log($"[WeaponDebugger] ✅ 修饰符列表已刷新，共 {currentModifiers.Count} 个修饰符");
    }

    /// <summary>
    /// 清空所有修饰符
    /// </summary>
    public void ClearAllModifiers()
    {
        if (playerControl == null || playerControl.ExternalWeaponInstance == null)
        {
            Debug.LogError("[WeaponDebugger] 未找到武器实例！");
            return;
        }

        var wc = playerControl.ExternalWeaponInstance.GetComponentInChildren<WeaponControl>();
        if (wc == null || wc.propertyManager == null)
        {
            Debug.LogError("[WeaponDebugger] 武器未配置 PropertyManager！");
            return;
        }

        // 清空所有属性的修饰符
        wc.propertyManager.Damage.ClearModifiers();
        wc.propertyManager.AttackRate.ClearModifiers();
        wc.propertyManager.Cooldown.ClearModifiers();
        wc.propertyManager.ChargingTime.ClearModifiers();
        wc.propertyManager.MeleeAttackRange.ClearModifiers();
        wc.propertyManager.CritChance.ClearModifiers();
        wc.propertyManager.CritDamageMultiplier.ClearModifiers();
        wc.propertyManager.ContinuousFireDuration.ClearModifiers();
        
        Debug.Log("[WeaponDebugger] ✅ 已清空所有修饰符");
        RefreshDisplay();
    }

    /// <summary>
    /// 重置属性为基础值
    /// </summary>
    public void ResetToBaseValues()
    {
        if (playerControl == null || playerControl.ExternalWeaponInstance == null)
        {
            Debug.LogError("[WeaponDebugger] 未找到武器实例！");
            return;
        }

        var wc = playerControl.ExternalWeaponInstance.GetComponentInChildren<WeaponControl>();
        if (wc == null || wc.propertyManager == null)
        {
            Debug.LogError("[WeaponDebugger] 武器未配置 PropertyManager！");
            return;
        }

        wc.propertyManager.RefreshBaseValues();
        Debug.Log("[WeaponDebugger] ✅ 已重置属性为基础值");
        RefreshDisplay();
    }

    private void Update()
    {
        // 每帧自动刷新显示数据
        RefreshDisplay();
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor Context Menu - 编辑器右键菜单
    /// </summary>
    [ContextMenu("🔄 刷新所有数据")]
    public void ContextRefreshAll()
    {
        RefreshDisplay();
        RefreshModifiersList();
        Debug.Log("[WeaponDebugger] ✅ 所有数据已刷新");
    }

    [ContextMenu("➕ 添加修饰符")]
    public void ContextAddModifier()
    {
        AddModifier();
    }

    [ContextMenu("✅ 确认修改")]
    public void ContextConfirmModifications()
    {
        ConfirmModifications();
    }

    [ContextMenu("🔫 立即切换武器")]
    public void ContextSwitchWeapon()
    {
        SwitchWeapon();
    }

    [ContextMenu("🗑️ 清空所有修饰符")]
    public void ContextClearAllModifiers()
    {
        ClearAllModifiers();
    }

    [ContextMenu("🔁 重置属性为基础值")]
    public void ContextResetToBaseValues()
    {
        ResetToBaseValues();
    }
#endif
}

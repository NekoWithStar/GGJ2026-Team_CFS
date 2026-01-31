using UnityEngine;
using Y_Survivor;

/// <summary>
/// 武器系统兼容性验证器
/// 用于检查 WeaponControl 和 Weapon 数据的配置是否正确
/// </summary>
public class WeaponSystemValidator : MonoBehaviour
{
    [Header("验证选项")]
    [Tooltip("启动时自动验证")]
    public bool validateOnStart = true;
    
    [Tooltip("按下此键手动验证")]
    public KeyCode manualValidateKey = KeyCode.F1;
    
    void Start()
    {
        if (validateOnStart)
        {
            ValidateAllWeapons();
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(manualValidateKey))
        {
            ValidateAllWeapons();
        }
    }
    
    /// <summary>
    /// 验证场景中所有武器
    /// </summary>
    [ContextMenu("Validate All Weapons")]
    public void ValidateAllWeapons()
    {
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("🔍 开始验证武器系统...");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        
        WeaponControl[] weapons = FindObjectsByType<WeaponControl>(FindObjectsSortMode.None);
        
        if (weapons.Length == 0)
        {
            Debug.LogWarning("⚠️ 场景中没有找到武器对象！");
            return;
        }
        
        Debug.Log($"📊 找到 {weapons.Length} 个武器对象\n");
        
        int validCount = 0;
        int warningCount = 0;
        int errorCount = 0;
        
        foreach (var weapon in weapons)
        {
            var result = ValidateWeapon(weapon);
            
            if (result.hasError)
                errorCount++;
            else if (result.hasWarning)
                warningCount++;
            else
                validCount++;
        }
        
        Debug.Log("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("📈 验证结果总结:");
        Debug.Log($"  ✅ 完全正常: {validCount}");
        Debug.Log($"  ⚠️ 有警告: {warningCount}");
        Debug.Log($"  ❌ 有错误: {errorCount}");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }
    
    /// <summary>
    /// 验证单个武器
    /// </summary>
    public ValidationResult ValidateWeapon(WeaponControl weapon)
    {
        var result = new ValidationResult();
        
        Debug.Log($"\n🔎 验证武器: {weapon.name}");
        Debug.Log("─────────────────────────────────────────");
        
        // 检查 WeaponControl 基础配置
        if (weapon == null)
        {
            Debug.LogError("  ❌ WeaponControl 组件为空！");
            result.hasError = true;
            return result;
        }
        
        // 检查 weaponData
        if (weapon.weaponData == null)
        {
            Debug.LogWarning($"  ⚠️ weaponData 未配置（运行时会设置）", weapon);
            result.hasWarning = true;
        }
        else
        {
            Debug.Log($"  ✅ weaponData: {weapon.weaponData.weaponName}");
            ValidateWeaponData(weapon.weaponData, result);
        }
        
        // 检查 PropertyManager
        if (weapon.propertyManager == null)
        {
            Debug.LogError($"  ❌ WeaponPropertyManager 未挂载！属性卡系统将无法工作！", weapon);
            result.hasError = true;
        }
        else
        {
            Debug.Log($"  ✅ WeaponPropertyManager 已挂载");
            
            // 检查 PropertyManager 是否正确引用 WeaponControl
            if (weapon.propertyManager.weaponControl == null)
            {
                Debug.LogWarning($"  ⚠️ PropertyManager.weaponControl 引用为空（Awake时会自动设置）", weapon);
                result.hasWarning = true;
            }
        }
        
        // 检查 MuzzlePoint
        if (weapon.muzzlePoint == null)
        {
            Debug.LogWarning($"  ⚠️ muzzlePoint 未配置（将使用武器对象自身位置）", weapon);
            result.hasWarning = true;
        }
        else
        {
            Debug.Log($"  ✅ muzzlePoint: {weapon.muzzlePoint.name}");
        }
        
        // 检查 AudioSource
        if (weapon.audioSource == null)
        {
            Debug.LogWarning($"  ⚠️ audioSource 未配置（音效将无法播放）", weapon);
            result.hasWarning = true;
        }
        else
        {
            Debug.Log($"  ✅ audioSource 已配置");
        }
        
        if (!result.hasError && !result.hasWarning)
        {
            Debug.Log("  ✅ 该武器配置完全正确！");
        }
        
        return result;
    }
    
    /// <summary>
    /// 验证 Weapon ScriptableObject 数据
    /// </summary>
    private void ValidateWeaponData(Weapon weaponData, ValidationResult result)
    {
        Debug.Log($"    📄 武器类型: {weaponData.weaponType}");
        Debug.Log($"    💥 基础伤害: {weaponData.damage}");
        Debug.Log($"    ⚡ 攻击速率: {weaponData.attackRate}/s");
        Debug.Log($"    🎯 暴击率: {weaponData.critChanceBase * 100:F1}%");
        
        // 检查远程武器配置
        if (weaponData.weaponType == Weapon.WEAPON_TYPE.Ranged)
        {
            if (weaponData.projectilePrefab == null && weaponData.weaponPrefab == null)
            {
                Debug.LogError($"    ❌ 远程武器缺少 projectilePrefab 和 weaponPrefab！", weaponData);
                result.hasError = true;
            }
            else
            {
                Debug.Log($"    ✅ 子弹预制体: {(weaponData.projectilePrefab != null ? weaponData.projectilePrefab.name : "使用 weaponPrefab")}");
                Debug.Log($"    🚀 子弹速度: {weaponData.projectileSpeed}");
            }
        }
        
        // 检查近战武器配置
        if (weaponData.weaponType == Weapon.WEAPON_TYPE.Melee)
        {
            Debug.Log($"    ⚔️ 近战范围: {weaponData.meleeRange}");
            
            if (weaponData.meleeRange <= 0)
            {
                Debug.LogWarning($"    ⚠️ 近战范围 <= 0，可能无法命中敌人！", weaponData);
                result.hasWarning = true;
            }
        }
        
        // 检查 FirePattern（持续自动开火武器）
        if (weaponData.continuousAutoFire)
        {
            Debug.Log($"    🔥 持续自动开火: 开启");
            Debug.Log($"    ⏱️ 持续时间: {weaponData.continuousFireDuration}s");
            Debug.Log($"    ❄️ 冷却时间: {weaponData.cooldown}s");
            
            if (weaponData.firePattern == null)
            {
                Debug.LogWarning($"    ⚠️ 持续自动开火武器缺少 firePattern！将使用基础开火模式", weaponData);
                result.hasWarning = true;
            }
            else
            {
                Debug.Log($"    ✅ FirePattern: {weaponData.firePattern.GetType().Name}");
            }
        }
        
        // 检查蓄力配置
        if (weaponData.requiresCharging)
        {
            Debug.Log($"    ⏳ 需要蓄力: {weaponData.chargingTime}s");
        }
    }
    
    /// <summary>
    /// 验证武器切换兼容性
    /// </summary>
    [ContextMenu("Test Weapon Switching")]
    public void TestWeaponSwitching()
    {
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("🔄 测试武器切换兼容性...");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        
        WeaponControl[] weapons = FindObjectsByType<WeaponControl>(FindObjectsSortMode.None);
        
        if (weapons.Length == 0)
        {
            Debug.LogWarning("⚠️ 场景中没有武器对象！");
            return;
        }
        
        var testWeapon = weapons[0];
        
        Debug.Log($"\n📍 使用武器对象: {testWeapon.name}");
        
        // 查找所有 Weapon ScriptableObject
        var allWeaponData = Resources.FindObjectsOfTypeAll<Weapon>();
        
        if (allWeaponData.Length < 2)
        {
            Debug.LogWarning("⚠️ 需要至少 2 个 Weapon ScriptableObject 来测试切换！");
            return;
        }
        
        Debug.Log($"\n找到 {allWeaponData.Length} 个武器数据:");
        foreach (var wd in allWeaponData)
        {
            Debug.Log($"  - {wd.weaponName} ({wd.weaponType})");
        }
        
        Debug.Log("\n✅ 武器切换测试准备就绪！");
        Debug.Log("提示：使用 PlayerControl.SwitchWeaponData() 方法切换武器数据");
        Debug.Log("      属性卡加成将会保持不变，只有基础值会更新");
    }
    
    public class ValidationResult
    {
        public bool hasError = false;
        public bool hasWarning = false;
    }
}

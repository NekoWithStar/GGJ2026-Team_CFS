using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 武器卡片选择器示例
/// 展示如何正确使用 SwitchWeaponData() 来切换武器而不丢失属性加成
/// </summary>
public class WeaponCardPickerExample : MonoBehaviour
{
    [Header("玩家引用")]
    [Tooltip("玩家控制器")]
    public PlayerControl playerControl;
    
    [Header("统一武器 Prefab")]
    [Tooltip("通用武器 Prefab（包含 WeaponControl + WeaponPropertyManager）")]
    public GameObject universalWeaponPrefab;
    
    [Header("可选武器数据")]
    [Tooltip("所有可选的武器数据列表")]
    public List<Weapon> availableWeapons = new List<Weapon>();
    
    [Header("测试快捷键")]
    [Tooltip("按下此键选择下一个武器")]
    public KeyCode nextWeaponKey = KeyCode.N;
    
    [Tooltip("按下此键选择上一个武器")]
    public KeyCode prevWeaponKey = KeyCode.P;
    
    [Tooltip("按下此键显示当前状态")]
    public KeyCode statusKey = KeyCode.I;
    
    private int currentWeaponIndex = 0;
    private bool isWeaponEquipped = false;
    
    void Start()
    {
        if (playerControl == null)
        {
            playerControl = FindFirstObjectByType<PlayerControl>();
        }
        
        if (playerControl == null)
        {
            Debug.LogError("[WeaponCardPicker] PlayerControl not found!");
            return;
        }
        
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("🎮 武器卡片选择器已就绪");
        Debug.Log($"  按 [{nextWeaponKey}] 选择下一个武器");
        Debug.Log($"  按 [{prevWeaponKey}] 选择上一个武器");
        Debug.Log($"  按 [{statusKey}] 显示当前状态");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        
        // 自动装备第一个武器
        if (availableWeapons.Count > 0)
        {
            EquipInitialWeapon();
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(nextWeaponKey))
        {
            SelectNextWeapon();
        }
        
        if (Input.GetKeyDown(prevWeaponKey))
        {
            SelectPreviousWeapon();
        }
        
        if (Input.GetKeyDown(statusKey))
        {
            ShowCurrentStatus();
        }
    }
    
    /// <summary>
    /// 首次装备武器（创建武器对象）
    /// </summary>
    void EquipInitialWeapon()
    {
        if (universalWeaponPrefab == null)
        {
            Debug.LogError("[WeaponCardPicker] universalWeaponPrefab is not assigned!");
            return;
        }
        
        if (availableWeapons.Count == 0)
        {
            Debug.LogError("[WeaponCardPicker] No weapons available!");
            return;
        }
        
        // 首次装备：实例化武器对象 + 设置数据
        Weapon firstWeapon = availableWeapons[0];
        playerControl.EquipExternalWeapon(universalWeaponPrefab, firstWeapon);
        
        isWeaponEquipped = true;
        currentWeaponIndex = 0;
        
        Debug.Log($"✅ 首次装备武器: {firstWeapon.weaponName} ({firstWeapon.weaponType})");
    }
    
    /// <summary>
    /// 选择下一个武器（推荐方式：只更换数据）
    /// </summary>
    public void SelectNextWeapon()
    {
        if (availableWeapons.Count == 0) return;
        
        // 如果还没装备武器，先装备
        if (!isWeaponEquipped)
        {
            EquipInitialWeapon();
            return;
        }
        
        // 切换到下一个武器数据
        currentWeaponIndex = (currentWeaponIndex + 1) % availableWeapons.Count;
        SwitchToWeapon(currentWeaponIndex);
    }
    
    /// <summary>
    /// 选择上一个武器
    /// </summary>
    public void SelectPreviousWeapon()
    {
        if (availableWeapons.Count == 0) return;
        
        if (!isWeaponEquipped)
        {
            EquipInitialWeapon();
            return;
        }
        
        currentWeaponIndex = (currentWeaponIndex - 1 + availableWeapons.Count) % availableWeapons.Count;
        SwitchToWeapon(currentWeaponIndex);
    }
    
    /// <summary>
    /// 切换到指定索引的武器
    /// </summary>
    public void SwitchToWeapon(int index)
    {
        if (index < 0 || index >= availableWeapons.Count)
        {
            Debug.LogWarning($"[WeaponCardPicker] Invalid weapon index: {index}");
            return;
        }
        
        Weapon targetWeapon = availableWeapons[index];
        
        Debug.Log($"\n🔄 切换武器到: {targetWeapon.weaponName}");
        Debug.Log($"   类型: {targetWeapon.weaponType}");
        Debug.Log($"   基础伤害: {targetWeapon.damage}");
        Debug.Log($"   ⚠️ 重要：属性卡加成会保持不变！");
        
        // 使用新方法：只更换数据，不销毁重建对象
        bool success = playerControl.SwitchWeaponData(targetWeapon);
        
        if (success)
        {
            currentWeaponIndex = index;
            Debug.Log($"✅ 武器切换成功！\n");
        }
        else
        {
            Debug.LogError("❌ 武器切换失败！");
        }
    }
    
    /// <summary>
    /// 显示当前武器状态
    /// </summary>
    void ShowCurrentStatus()
    {
        if (!isWeaponEquipped)
        {
            Debug.Log("⚠️ 当前未装备武器");
            return;
        }
        
        Debug.Log("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("📊 当前武器状态");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        
        Weapon currentWeapon = availableWeapons[currentWeaponIndex];
        Debug.Log($"名称: {currentWeapon.weaponName}");
        Debug.Log($"类型: {currentWeapon.weaponType}");
        Debug.Log($"索引: {currentWeaponIndex + 1}/{availableWeapons.Count}");
        
        // 尝试获取实际属性值（包含属性卡加成）
        var weaponInstance = playerControl.ExternalWeaponInstance;
        if (weaponInstance != null)
        {
            var wc = weaponInstance.GetComponentInChildren<WeaponControl>();
            if (wc != null && wc.propertyManager != null)
            {
                var pm = wc.propertyManager;
                Debug.Log("\n📈 实际属性值（包含属性卡加成）:");
                Debug.Log($"  💥 伤害: {pm.GetDamage()} (基础: {currentWeapon.damage})");
                Debug.Log($"  ⚡ 攻速: {pm.GetAttackRate():F2}/s (基础: {currentWeapon.attackRate})");
                Debug.Log($"  🎯 暴击率: {pm.GetCritChance() * 100:F1}% (基础: {currentWeapon.critChanceBase * 100:F1}%)");
                Debug.Log($"  💢 暴击倍率: {pm.GetCritDamageMultiplier():F2}x (基础: {currentWeapon.critDamageBase:F2}x)");
                
                if (currentWeapon.weaponType == Weapon.WEAPON_TYPE.Melee)
                {
                    Debug.Log($"  ⚔️ 近战范围: {pm.GetMeleeRange():F2} (基础: {currentWeapon.meleeRange})");
                }
            }
        }
        
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
    }
    
    /// <summary>
    /// 通过卡片索引选择武器（供 UI 调用）
    /// </summary>
    public void OnWeaponCardSelected(int cardIndex)
    {
        SwitchToWeapon(cardIndex);
    }
    
    /// <summary>
    /// 通过武器数据选择武器（供其他系统调用）
    /// </summary>
    public void OnWeaponDataSelected(Weapon weaponData)
    {
        if (weaponData == null)
        {
            Debug.LogWarning("[WeaponCardPicker] Weapon data is null!");
            return;
        }
        
        // 查找武器在列表中的索引
        int index = availableWeapons.IndexOf(weaponData);
        
        if (index >= 0)
        {
            SwitchToWeapon(index);
        }
        else
        {
            // 如果不在列表中，直接切换
            Debug.Log($"🔄 切换到外部武器: {weaponData.weaponName}");
            playerControl.SwitchWeaponData(weaponData);
        }
    }
}

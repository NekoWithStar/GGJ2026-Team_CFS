using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Y_Survivor;

/// <summary>
/// 属性卡 UI 控制 - 将 PropertyCard ScriptableObject 绑定到 UI 元素
/// 仿照 WeaponCardControl 风格，供 Flip_Card 使用
/// </summary>
public class PropertyCardControl : MonoBehaviour
{
    public PropertyCard propertyCard; // 属性卡数据 ScriptableObject

    // UI 元素
    public Image icon; // 卡牌图标
    public Text cardName; // 卡牌名称
    public Text rarity; // 稀有度文本
    public Text description; // 卡牌描述
    public Text modifiersInfo; // 修饰符信息（可选，显示所有属性加成）
    public GameObject back; // 背面对象（可选）

    private void Awake()
    {
        if (propertyCard == null) return;
        UpdateUI();
    }

    private void OnValidate()
    {
        // 编辑器下实时同步显示，方便调试
        if (propertyCard == null) return;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (propertyCard == null) return;

        if (icon != null && propertyCard.cardIcon != null)
        {
            icon.sprite = propertyCard.cardIcon;
            icon.enabled = true;
        }

        if (cardName != null) cardName.text = propertyCard.cardName;
        if (rarity != null) rarity.text = propertyCard.rarity.ToString();
        if (description != null) description.text = propertyCard.description;

        // 显示所有属性修饰符信息
        if (modifiersInfo != null)
        {
            modifiersInfo.text = GetModifiersDisplayText();
        }
    }

    /// <summary>
    /// 在运行时由外部调用以绑定卡牌数据并刷新UI
    /// </summary>
    public void SetupCard(PropertyCard card)
    {
        propertyCard = card;
        Debug.Log($"[PropertyCardControl] 🔄 设置属性卡: {card?.cardName ?? "null"}");
        UpdateUI();
    }

    /// <summary>
    /// 生成修饰符显示文本
    /// </summary>
    private string GetModifiersDisplayText()
    {
        if (propertyCard.modifiers == null || propertyCard.modifiers.Count == 0)
        {
            return "无属性加成";
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var modifier in propertyCard.modifiers)
        {
            string propName = GetPropertyDisplayName(modifier.targetProperty);
            string modType = modifier.modifierType.ToString();
            sb.AppendLine($"{propName} {modType} {modifier.value}");
        }
        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// 获取属性类型的显示名称
    /// </summary>
    private string GetPropertyDisplayName(PropertyType type)
    {
        return type switch
        {
            PropertyType.Cooldown => "冷却时间",
            PropertyType.AttackRate => "攻击速率",
            PropertyType.Damage => "伤害",
            PropertyType.ChargingTime => "蓄力时间",
            PropertyType.ContinuousFireDuration => "持续开火时间",
            PropertyType.CritChance => "暴击率",
            PropertyType.CritDamageMultiplier => "暴击伤害",
            PropertyType.MeleeAttackRange => "近战范围",
            PropertyType.PlayerMoveSpeed => "玩家移动速度",
            PropertyType.PlayerHealth => "玩家当前血量",
            PropertyType.PlayerMaxHealth => "玩家最大血量",
            PropertyType.SmallEnemyMoveSpeed => "小怪移速",
            PropertyType.MediumEnemyMoveSpeed => "中怪移速",
            PropertyType.LargeEnemyMoveSpeed => "大怪移速",
            _ => type.ToString()
        };
    }
}

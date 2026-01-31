using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Y_Survivor
{
    /// <summary>
    /// 属性卡配置 - ScriptableObject
    /// 定义一张属性卡可以提供的所有加成效果
    /// </summary>
    [CreateAssetMenu(menuName = "Y_Survivor/Property Card", fileName = "NewPropertyCard")]
    public class PropertyCard : ScriptableObject
    {
        [Header("基础信息")]
        [Tooltip("卡片名称")]
        public string cardName = "新属性卡";
        
        [Tooltip("卡片描述")]
        [TextArea(2, 4)]
        public string description = "";
        
        [Tooltip("卡片图标 - 用于卡牌展示的Sprite")]
        public Sprite cardIcon; // 卡牌展示用的Sprite图标
        
        [Tooltip("卡片稀有度（用于显示和筛选）")]
        public CardRarity rarity = CardRarity.Common;
        
        [Header("属性加成")]
        [Tooltip("属性修饰符列表 - 每个元素代表一个属性加成")]
        public List<PropertyModifierElement> modifiers = new List<PropertyModifierElement>();
        
        [Header("自定义效果")]
        [Tooltip("是否启用自定义效果")]
        public bool hasCustomEffect = false;
        
        [Tooltip("自定义效果类型")]
        public CustomEffectType customEffectType = CustomEffectType.None;
        
        [Tooltip("自定义效果参数（根据效果类型使用）")]
        public float customEffectValue = 0f;
        
        [Tooltip("自定义效果参数2（某些效果需要）")]
        public float customEffectValue2 = 0f;
        
        [Tooltip("自定义效果持续时间（秒）")]
        public float customEffectDuration = 3f;
        
        [Tooltip("替换武器数据（用于以旧换新效果）- 若为空则从Resources自动加载")]
        public Weapon replacementWeapon;
        
        [Tooltip("随机播放的音频列表（用于猫耳耳机效果）- 若为空则从Resources/Audio/CatEarHeadset自动加载")]
        public List<AudioClip> randomAudioClips = new List<AudioClip>();
        
        /// <summary>
        /// 获取指定属性类型的所有修饰符
        /// </summary>
        public List<PropertyModifierElement> GetModifiersForProperty(PropertyType propertyType)
        {
            var result = new List<PropertyModifierElement>();
            foreach (var mod in modifiers)
            {
                if (mod.targetProperty == propertyType)
                {
                    result.Add(mod);
                }
            }
            return result;
        }
        
        /// <summary>
        /// 检查此卡是否包含指定属性类型的加成
        /// </summary>
        public bool HasModifierForProperty(PropertyType propertyType)
        {
            foreach (var mod in modifiers)
            {
                if (mod.targetProperty == propertyType)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 获取卡片效果的简短描述文本
        /// </summary>
        public string GetEffectSummary()
        {
            var lines = new List<string>();
            
            foreach (var mod in modifiers)
            {
                string sign = mod.value >= 0 ? "+" : "";
                string valueStr = mod.modifierType == EasyPack.Modifiers.ModifierType.Mul 
                    ? $"x{mod.value:F2}" 
                    : $"{sign}{mod.value:F1}";
                lines.Add($"{GetPropertyDisplayName(mod.targetProperty)}: {valueStr}");
            }
            
            if (hasCustomEffect && customEffectType != CustomEffectType.None)
            {
                lines.Add($"[特殊] {GetCustomEffectDisplayName(customEffectType)}");
            }
            
            return string.Join("\n", lines);
        }
        
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
                PropertyType.PlayerMoveSpeed => "移动速度",
                PropertyType.PlayerHealth => "生命值",
                // PropertyType.PlayerMaxHealth => "最大生命值", // 已完全移除
                PropertyType.SmallEnemyMoveSpeed => "小怪移速",
                PropertyType.MediumEnemyMoveSpeed => "中怪移速",
                PropertyType.LargeEnemyMoveSpeed => "大怪移速",
                _ => type.ToString()
            };
        }
        
        private string GetCustomEffectDisplayName(CustomEffectType type)
        {
            return type switch
            {
                CustomEffectType.None => "无",
                CustomEffectType.LimitedVision => $"视野受限 ({customEffectDuration}s)",
                CustomEffectType.AudioDamage => $"耳机损耗 ({customEffectDuration}s)",
                CustomEffectType.CatEarHeadset => GetCatEarHeadsetDisplayName(),
                CustomEffectType.BrokenCompass => $"失灵指南针 ({customEffectDuration}s)",
                CustomEffectType.WeaponSwitch => GetWeaponSwitchDisplayName(),
                CustomEffectType.EnemyModifier => "敌人控制",
                _ => type.ToString()
            };
        }

        /// <summary>
        /// 获取猫耳耳机效果的显示名称
        /// </summary>
        private string GetCatEarHeadsetDisplayName()
        {
            if (randomAudioClips != null && randomAudioClips.Count > 0)
            {
                return $"猫耳耳机 ({randomAudioClips.Count}首音乐)";
            }
            else
            {
                return "猫耳耳机 (自动加载)";
            }
        }

        /// <summary>
        /// 获取以旧换新效果的显示名称
        /// </summary>
        private string GetWeaponSwitchDisplayName()
        {
            if (replacementWeapon != null)
            {
                return $"以旧换新 -> {replacementWeapon.weaponName}";
            }
            else if (customEffectValue > 0)
            {
                return $"以旧换新 -> 武器ID:{Mathf.RoundToInt(customEffectValue)}";
            }
            else
            {
                return "以旧换新 (自动加载)";
            }
        }
    }
    
    /// <summary>
    /// 卡片稀有度
    /// </summary>
    public enum CardRarity
    {
        Common = 0,     // 普通
        Uncommon = 1,   // 稀有
        Rare = 2,       // 史诗
        Epic = 3,       // 传说
        Legendary = 4   // 神话
    }
}

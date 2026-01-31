using System.Collections.Generic;
using UnityEngine;
using EasyPack.GamePropertySystem;
using EasyPack.Modifiers;

namespace Y_Survivor
{
    /// <summary>
    /// 武器属性管理器 - 使用 GameProperty 系统管理武器的动态属性
    /// 挂载在武器对象上，与 WeaponControl 配合使用
    /// </summary>
    public class WeaponPropertyManager : MonoBehaviour
    {
        [Header("关联的武器控制器")]
        [Tooltip("如果为空，会尝试自动获取同对象上的 WeaponControl")]
        public WeaponControl weaponControl;
        
        // ===== 武器属性 =====
        public GameProperty Cooldown { get; private set; }
        public GameProperty AttackRate { get; private set; }
        public GameProperty Damage { get; private set; }
        public GameProperty ChargingTime { get; private set; }
        public GameProperty ContinuousFireDuration { get; private set; }
        public GameProperty CritChance { get; private set; }
        public GameProperty CritDamageMultiplier { get; private set; }
        public GameProperty MeleeAttackRange { get; private set; }
        
        // 已应用的卡片及其修饰符的映射
        private Dictionary<PropertyCard, List<(PropertyType, IModifier)>> appliedCards 
            = new Dictionary<PropertyCard, List<(PropertyType, IModifier)>>();
        
        private void Awake()
        {
            if (weaponControl == null)
            {
                weaponControl = GetComponent<WeaponControl>();
            }
            
            InitializeProperties();
        }
        
        /// <summary>
        /// 初始化所有属性，从 WeaponControl 的 weaponData 读取基础值
        /// </summary>
        public void InitializeProperties()
        {
            if (weaponControl == null || weaponControl.weaponData == null)
            {
                // 使用默认值初始化
                Cooldown = new GameProperty("Weapon.Cooldown", 0.5f);
                AttackRate = new GameProperty("Weapon.AttackRate", 1f);
                Damage = new GameProperty("Weapon.Damage", 10f);
                ChargingTime = new GameProperty("Weapon.ChargingTime", 0.2f);
                ContinuousFireDuration = new GameProperty("Weapon.ContinuousFireDuration", 5f);
                CritChance = new GameProperty("Weapon.CritChance", 0f);
                CritDamageMultiplier = new GameProperty("Weapon.CritDamageMultiplier", 1.5f);
                MeleeAttackRange = new GameProperty("Weapon.MeleeAttackRange", 2f);
                return;
            }
            
            var data = weaponControl.weaponData;
            
            Cooldown = new GameProperty("Weapon.Cooldown", data.cooldown);
            AttackRate = new GameProperty("Weapon.AttackRate", data.attackRate);
            Damage = new GameProperty("Weapon.Damage", data.damage);
            ChargingTime = new GameProperty("Weapon.ChargingTime", data.chargingTime);
            ContinuousFireDuration = new GameProperty("Weapon.ContinuousFireDuration", data.continuousFireDuration);
            CritChance = new GameProperty("Weapon.CritChance", data.critChanceBase);
            CritDamageMultiplier = new GameProperty("Weapon.CritDamageMultiplier", data.critDamageBase);
            MeleeAttackRange = new GameProperty("Weapon.MeleeAttackRange", data.meleeRange);
        }
        
        /// <summary>
        /// 当武器数据改变时，重新初始化属性的基础值
        /// </summary>
        public void RefreshBaseValues()
        {
            if (weaponControl == null || weaponControl.weaponData == null) return;
            
            var data = weaponControl.weaponData;
            
            Cooldown.SetBaseValue(data.cooldown);
            AttackRate.SetBaseValue(data.attackRate);
            Damage.SetBaseValue(data.damage);
            ChargingTime.SetBaseValue(data.chargingTime);
            ContinuousFireDuration.SetBaseValue(data.continuousFireDuration);
            CritChance.SetBaseValue(data.critChanceBase);
            CritDamageMultiplier.SetBaseValue(data.critDamageBase);
            MeleeAttackRange.SetBaseValue(data.meleeRange);
        }
        
        /// <summary>
        /// 应用一张属性卡的所有修饰符
        /// </summary>
        /// <param name="card">要应用的属性卡</param>
        public void ApplyPropertyCard(PropertyCard card)
        {
            if (card == null || appliedCards.ContainsKey(card)) return;
            
            var appliedModifiers = new List<(PropertyType, IModifier)>();
            
            foreach (var element in card.modifiers)
            {
                var property = GetPropertyByType(element.targetProperty);
                if (property != null)
                {
                    var modifier = element.CreateModifier();
                    property.AddModifier(modifier);
                    appliedModifiers.Add((element.targetProperty, modifier));
                }
            }
            
            appliedCards[card] = appliedModifiers;
            
            Debug.Log($"[WeaponPropertyManager] Applied card: {card.cardName} with {appliedModifiers.Count} modifiers");
        }
        
        /// <summary>
        /// 移除一张属性卡的所有修饰符
        /// </summary>
        /// <param name="card">要移除的属性卡</param>
        public void RemovePropertyCard(PropertyCard card)
        {
            if (card == null || !appliedCards.TryGetValue(card, out var modifiers)) return;
            
            foreach (var (propType, modifier) in modifiers)
            {
                var property = GetPropertyByType(propType);
                property?.RemoveModifier(modifier);
            }
            
            appliedCards.Remove(card);
            
            Debug.Log($"[WeaponPropertyManager] Removed card: {card.cardName}");
        }
        
        /// <summary>
        /// 清除所有应用的属性卡
        /// </summary>
        public void ClearAllCards()
        {
            foreach (var card in new List<PropertyCard>(appliedCards.Keys))
            {
                RemovePropertyCard(card);
            }
        }
        
        /// <summary>
        /// 根据属性类型获取对应的 GameProperty
        /// </summary>
        public GameProperty GetPropertyByType(PropertyType type)
        {
            return type switch
            {
                PropertyType.Cooldown => Cooldown,
                PropertyType.AttackRate => AttackRate,
                PropertyType.Damage => Damage,
                PropertyType.ChargingTime => ChargingTime,
                PropertyType.ContinuousFireDuration => ContinuousFireDuration,
                PropertyType.CritChance => CritChance,
                PropertyType.CritDamageMultiplier => CritDamageMultiplier,
                PropertyType.MeleeAttackRange => MeleeAttackRange,
                _ => null
            };
        }
        
        /// <summary>
        /// 获取属性的最终值
        /// </summary>
        public float GetPropertyValue(PropertyType type)
        {
            var property = GetPropertyByType(type);
            return property?.GetValue() ?? 0f;
        }
        
        // ===== 便捷访问器 =====
        
        /// <summary>获取最终冷却时间</summary>
        public float GetCooldown() => Cooldown.GetValue();
        
        /// <summary>获取最终攻击速率</summary>
        public float GetAttackRate() => AttackRate.GetValue();
        
        /// <summary>获取最终伤害值</summary>
        public int GetDamage() => Mathf.RoundToInt(Damage.GetValue());
        
        /// <summary>获取最终蓄力时间</summary>
        public float GetChargingTime() => ChargingTime.GetValue();
        
        /// <summary>获取最终持续开火时间</summary>
        public float GetContinuousFireDuration() => ContinuousFireDuration.GetValue();
        
        /// <summary>获取最终暴击率</summary>
        public float GetCritChance() => Mathf.Clamp01(CritChance.GetValue());
        
        /// <summary>获取最终暴击伤害倍率</summary>
        public float GetCritDamageMultiplier() => CritDamageMultiplier.GetValue();
        
        /// <summary>获取最终近战范围</summary>
        public float GetMeleeRange() => MeleeAttackRange.GetValue();
    }
}

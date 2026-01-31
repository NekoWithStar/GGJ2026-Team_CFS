using System.Collections.Generic;
using UnityEngine;
using EasyPack.GamePropertySystem;
using EasyPack.Modifiers;

namespace Y_Survivor
{
    /// <summary>
    /// 玩家属性管理器 - 使用 GameProperty 系统管理玩家的动态属性
    /// 挂载在玩家对象上，与 PlayerControl 配合使用
    /// </summary>
    public class PlayerPropertyManager : MonoBehaviour
    {
        [Header("基础值设置")]
        [Tooltip("基础移动速度")]
        public float baseMoveSpeed = 5f;
        
        [Tooltip("基础当前生命值")]
        public float baseHealth = 100f;
        
        // ===== 玩家属性 =====
        public GameProperty MoveSpeed { get; private set; }
        public GameProperty CurrentHealth { get; private set; }
        
        // 已应用的卡片及其修饰符的映射
        private Dictionary<PropertyCard, List<(PropertyType, IModifier)>> appliedCards 
            = new Dictionary<PropertyCard, List<(PropertyType, IModifier)>>();
        
        private CustomEffectHandler customEffectHandler;
        
        private void Awake()
        {
            InitializeProperties();
            customEffectHandler = GetComponent<CustomEffectHandler>();
            if (customEffectHandler == null)
            {
                Debug.LogWarning("[PlayerPropertyManager] CustomEffectHandler未找到，自定义效果将不可用");
            }
        }
        
        /// <summary>
        /// 初始化所有属性
        /// </summary>
        public void InitializeProperties()
        {
            MoveSpeed = new GameProperty("Player.MoveSpeed", baseMoveSpeed);
            CurrentHealth = new GameProperty("Player.CurrentHealth", baseHealth);
            
            Debug.Log("[PlayerPropertyManager] 属性系统已初始化 - 完全移除最大血量限制");
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
            
            // 处理自定义效果
            if (customEffectHandler != null && card.hasCustomEffect)
            {
                customEffectHandler.HandleCustomEffect(card);
            }
            
            Debug.Log($"[PlayerPropertyManager] Applied card: {card.cardName} with {appliedModifiers.Count} modifiers");
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
            
            Debug.Log($"[PlayerPropertyManager] Removed card: {card.cardName}");
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
                PropertyType.PlayerMoveSpeed => MoveSpeed,
                PropertyType.PlayerHealth => CurrentHealth,
                // PlayerMaxHealth 已完全移除，不再处理
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
        
        /// <summary>获取最终移动速度</summary>
        public float GetMoveSpeed() => Mathf.Max(0.1f, MoveSpeed.GetValue());
        
        /// <summary>获取当前生命值</summary>
        public float GetCurrentHealth() => CurrentHealth.GetValue();
        
        /// <summary>设置当前生命值（无上限限制）</summary>
        public void SetCurrentHealth(float value)
        {
            CurrentHealth.SetBaseValue(Mathf.Max(0f, value));
        }
        
        /// <summary>受到伤害</summary>
        public void TakeDamage(float damage)
        {
            SetCurrentHealth(GetCurrentHealth() - damage);
        }
        
        /// <summary>恢复生命</summary>
        public void Heal(float amount)
        {
            SetCurrentHealth(GetCurrentHealth() + amount);
        }
        
        /// <summary>检查是否死亡</summary>
        public bool IsDead => GetCurrentHealth() <= 0f;
    }
}

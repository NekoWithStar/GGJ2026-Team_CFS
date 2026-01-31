using System.Collections.Generic;
using UnityEngine;
using EasyPack.GamePropertySystem;
using EasyPack.Modifiers;

namespace Y_Survivor
{
    /// <summary>
    /// 敌人属性管理器 - 全局单例，管理敌人类型的移动速度等属性
    /// 所有同类型的敌人共享相同的属性修饰符
    /// </summary>
    public class EnemyPropertyManager : MonoBehaviour
    {
        public static EnemyPropertyManager Instance { get; private set; }
        
        [Header("基础值设置")]
        [Tooltip("小型怪物基础移动速度")]
        public float baseSmallEnemySpeed = 2f;
        
        [Tooltip("中型怪物基础移动速度")]
        public float baseMediumEnemySpeed = 1.5f;
        
        // ===== 敌人属性 =====
        public GameProperty SmallEnemyMoveSpeed { get; private set; }
        public GameProperty MediumEnemyMoveSpeed { get; private set; }
        
        // 已应用的卡片及其修饰符的映射
        private Dictionary<PropertyCard, List<(PropertyType, IModifier)>> appliedCards 
            = new Dictionary<PropertyCard, List<(PropertyType, IModifier)>>();
        
        private void Awake()
        {
            // 单例模式
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializeProperties();
        }
        
        /// <summary>
        /// 初始化所有属性
        /// </summary>
        public void InitializeProperties()
        {
            SmallEnemyMoveSpeed = new GameProperty("Enemy.SmallMoveSpeed", baseSmallEnemySpeed);
            MediumEnemyMoveSpeed = new GameProperty("Enemy.MediumMoveSpeed", baseMediumEnemySpeed);
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
            
            Debug.Log($"[EnemyPropertyManager] Applied card: {card.cardName} with {appliedModifiers.Count} modifiers");
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
            
            Debug.Log($"[EnemyPropertyManager] Removed card: {card.cardName}");
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
                PropertyType.SmallEnemyMoveSpeed => SmallEnemyMoveSpeed,
                PropertyType.MediumEnemyMoveSpeed => MediumEnemyMoveSpeed,
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
        
        /// <summary>获取小型怪物最终移动速度</summary>
        public float GetSmallEnemySpeed() => SmallEnemyMoveSpeed.GetValue();
        
        /// <summary>获取中型怪物最终移动速度</summary>
        public float GetMediumEnemySpeed() => MediumEnemyMoveSpeed.GetValue();
    }
}

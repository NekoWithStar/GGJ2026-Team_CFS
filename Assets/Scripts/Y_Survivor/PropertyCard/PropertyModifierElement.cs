using System;
using UnityEngine;
using EasyPack.Modifiers;

namespace Y_Survivor
{
    /// <summary>
    /// 属性修饰符元素 - 定义单个属性加成
    /// 可在 Inspector 中配置修饰符类型、目标属性和数值
    /// </summary>
    [Serializable]
    public class PropertyModifierElement
    {
        [Tooltip("要加成的目标属性")]
        public PropertyType targetProperty = PropertyType.None;
        
        [Tooltip("修饰符类型：Add=加法, Mul=乘法, PriorityAdd=优先加法, PriorityMul=优先乘法, AfterAdd=后置加法, Override=覆盖, Clamp=限制范围")]
        public ModifierType modifierType = ModifierType.Add;
        
        [Tooltip("修饰符数值（对于乘法，1.0=100%，1.5=150%）")]
        public float value = 0f;
        
        [Tooltip("优先级（数值越大越优先，默认0）")]
        public int priority = 0;
        
        /// <summary>
        /// 创建对应的 FloatModifier 实例
        /// </summary>
        public FloatModifier CreateModifier()
        {
            return new FloatModifier(modifierType, priority, value);
        }
    }
}

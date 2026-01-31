using System;

namespace Y_Survivor
{
    /// <summary>
    /// 可被属性卡加成的属性类型枚举
    /// </summary>
    [Serializable]
    public enum PropertyType
    {
        None = 0,
        
        // ===== 武器属性 =====
        /// <summary>冷却时间（秒）</summary>
        Cooldown = 100,
        /// <summary>攻击速率（每秒攻击次数）</summary>
        AttackRate = 101,
        /// <summary>伤害值</summary>
        Damage = 102,
        /// <summary>蓄力时间（秒）</summary>
        ChargingTime = 103,
        /// <summary>持续自动开火时间（秒）</summary>
        ContinuousFireDuration = 104,
        /// <summary>暴击率（0~1）</summary>
        CritChance = 105,
        /// <summary>暴击伤害倍率</summary>
        CritDamageMultiplier = 106,
        /// <summary>近战攻击范围</summary>
        MeleeAttackRange = 107,
        
        // ===== 玩家属性 =====
        /// <summary>玩家移动速度</summary>
        PlayerMoveSpeed = 200,
        /// <summary>玩家当前血量</summary>
        PlayerHealth = 201,
        /// <summary>玩家最大血量</summary>
        PlayerMaxHealth = 202,
        
        // ===== 敌人属性 =====
        /// <summary>小型怪物移动速度</summary>
        SmallEnemyMoveSpeed = 300,
        /// <summary>中型怪物移动速度</summary>
        MediumEnemyMoveSpeed = 301,
        /// <summary>大型怪物移动速度</summary>
        LargeEnemyMoveSpeed = 302,
    }
}

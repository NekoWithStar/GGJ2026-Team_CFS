using System;

namespace Y_Survivor
{
    /// <summary>
    /// 自定义效果类型枚举
    /// 用于实现无法通过数值修饰符表达的特殊效果
    /// </summary>
    [Serializable]
    public enum CustomEffectType
    {
        /// <summary>无自定义效果</summary>
        None = 0,
        
        /// <summary>视野受限 - 短暂影响摄像机视距</summary>
        LimitedVision = 1,
        
        /// <summary>耳机损耗 - 短暂影响音频源音量</summary>
        AudioDamage = 2,
        
        /// <summary>猫耳耳机 - 随机播放指定列表中的音频</summary>
        CatEarHeadset = 3,
        
        /// <summary>失灵指南针 - 短暂颠倒玩家运动方向</summary>
        BrokenCompass = 4,
        
        /// <summary>以旧换新 - 更改玩家手持武器数据源</summary>
        WeaponSwitch = 5,
        
        /// <summary>敌人控制 - 调整敌人的速度和伤害</summary>
        EnemyModifier = 6
    }
}

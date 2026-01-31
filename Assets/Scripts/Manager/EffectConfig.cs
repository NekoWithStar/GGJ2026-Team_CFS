using UnityEngine;
using System.Collections.Generic;

namespace Y_Survivor
{
    /// <summary>
    /// 自定义效果配置 - 不使用ScriptableObject
    /// 用于"假传入"效果实现，无需美术资源配置
    /// </summary>
    public static class EffectConfig
    {
        // ===== 猫耳耳机配置 =====
        public const float CAT_EAR_HEADSET_DURATION = 3f;
        
        // 硬编码的音频列表（可从Resources加载或使用默认音效）
        public static List<string> GetCatEarHeadsetAudios()
        {
            return new List<string>
            {
                "meow_1",    // 猫叫声1
                "meow_2",    // 猫叫声2
                "meow_3",    // 猫叫声3
                "purr_1"     // 猫咕噜声
            };
        }
        
        // ===== 失灵指南针配置 =====
        public const float BROKEN_COMPASS_DURATION = 2.5f;
        
        // ===== 以旧换新配置 =====
        public const float WEAPON_SWITCH_DURATION = 5f;  // 武器切换持续时间
        
        // 硬编码的武器配置（可从Resources加载或使用预设）
        public static Weapon GetAlternativeWeapon()
        {
            // 这里可以从Resources或其他地方动态加载
            // 或者返回null让系统随机选择
            return null;
        }
        
        // ===== 耳机损耗配置 =====
        public const float AUDIO_DAMAGE_DURATION = 4f;
        public const float AUDIO_DAMAGE_VOLUME_MULTIPLIER = 0.3f;  // 音量降至30%
        
        // ===== 视野受限配置 =====
        public const float LIMITED_VISION_DURATION = 3f;
        
        // ===== 敌人控制配置 =====
        public const float ENEMY_MODIFIER_DURATION = 3f;
        public const float ENEMY_SPEED_MULTIPLIER = 0.5f;    // 敌人速度降至50%
        public const float ENEMY_DAMAGE_MULTIPLIER = 1.5f;   // 敌人伤害增至150%
    }
}

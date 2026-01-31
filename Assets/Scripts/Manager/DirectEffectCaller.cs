using UnityEngine;
using System.Collections.Generic;
using Y_Survivor;

/// <summary>
/// 无PropertyCard的直接效果调用器
/// 完全不依赖ScriptableObject，直接调用效果方法
/// </summary>
public static class DirectEffectCaller
{
    private static CustomEffectHandler _handler;
    
    /// <summary>
    /// 初始化效果处理器
    /// </summary>
    public static void Initialize(CustomEffectHandler handler)
    {
        _handler = handler;
    }
    
    /// <summary>
    /// 直接调用猫耳耳机效果
    /// </summary>
    public static void ApplyCatEarHeadset(List<AudioClip> audioClips = null)
    {
        if (_handler == null) return;
        
        Debug.Log("[DirectEffectCaller] 应用猫耳耳机效果");
        _handler.ApplyCatEarHeadsetDirect(audioClips, EffectConfig.CAT_EAR_HEADSET_DURATION);
    }
    
    /// <summary>
    /// 直接调用失灵指南针效果
    /// </summary>
    public static void ApplyBrokenCompass()
    {
        if (_handler == null) return;
        
        Debug.Log("[DirectEffectCaller] 应用失灵指南针效果");
        _handler.ApplyBrokenCompassDirect(EffectConfig.BROKEN_COMPASS_DURATION);
    }
    
    /// <summary>
    /// 直接调用以旧换新效果
    /// </summary>
    public static void ApplyWeaponSwitch(Weapon replacementWeapon)
    {
        if (_handler == null) return;
        
        Debug.Log("[DirectEffectCaller] 应用以旧换新效果");
        _handler.ApplyWeaponSwitchDirect(replacementWeapon);
    }
    
    /// <summary>
    /// 直接调用耳机损耗效果
    /// </summary>
    public static void ApplyAudioDamage(float volumeMultiplier)
    {
        if (_handler == null) return;
        
        Debug.Log("[DirectEffectCaller] 应用耳机损耗效果");
        _handler.ApplyAudioDamageDirect(volumeMultiplier, EffectConfig.AUDIO_DAMAGE_DURATION);
    }
    
    /// <summary>
    /// 直接调用视野受限效果
    /// </summary>
    public static void ApplyLimitedVision()
    {
        if (_handler == null) return;
        
        Debug.Log("[DirectEffectCaller] 应用视野受限效果");
        _handler.ApplyLimitedVisionDirect(EffectConfig.LIMITED_VISION_DURATION);
    }
    
    /// <summary>
    /// 直接调用敌人修改效果
    /// </summary>
    public static void ApplyEnemyModifier(float speedMultiplier, float damageMultiplier)
    {
        if (_handler == null) return;
        
        Debug.Log("[DirectEffectCaller] 应用敌人修改效果");
        _handler.ApplyEnemyModifierDirect(speedMultiplier, damageMultiplier, EffectConfig.ENEMY_MODIFIER_DURATION);
    }
}

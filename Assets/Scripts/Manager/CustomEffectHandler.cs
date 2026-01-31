using UnityEngine;
using Y_Survivor;
using System.Collections.Generic;

/// <summary>
/// 自定义效果处理器 - 处理所有自定义效果的逻辑
/// 与 WeaponPropertyManager 和 PlayerPropertyManager 配合使用
/// </summary>
public class CustomEffectHandler : MonoBehaviour
{
    private PlayerControl playerControl;
    private PlayerPropertyManager playerPropertyManager;
    private Camera mainCam;
    private AudioSource audioSource;
    
    // 视野受限相关
    private float originalOrthographicSize;
    private Coroutine limitedVisionCoroutine;
    
    // 失灵指南针相关
    private bool directionReversed = false;
    private Coroutine brokenCompassCoroutine;
    
    // 猫耳耳机相关
    private Coroutine catEarHeadsetCoroutine;

    // 敌人控制相关
    private Dictionary<EnemyControl, (float originalSpeed, float originalDamage)> modifiedEnemies 
        = new Dictionary<EnemyControl, (float, float)>();

    private void Awake()
    {
        playerControl = GetComponent<PlayerControl>();
        playerPropertyManager = GetComponent<PlayerPropertyManager>();
        mainCam = Camera.main;
        audioSource = GetComponent<AudioSource>();
        
        if (mainCam == null)
        {
            mainCam = FindAnyObjectByType<Camera>();
        }
        
        if (originalOrthographicSize == 0f && mainCam != null)
        {
            originalOrthographicSize = mainCam.orthographicSize;
        }
    }

    /// <summary>
    /// 处理自定义效果
    /// </summary>
    public void HandleCustomEffect(PropertyCard card)
    {
        if (!card.hasCustomEffect || card.customEffectType == CustomEffectType.None)
        {
            return;
        }

        Debug.Log($"[CustomEffectHandler] 应用自定义效果: {card.customEffectType}");

        switch (card.customEffectType)
        {
            case CustomEffectType.LimitedVision:
                ApplyLimitedVision(card);
                break;
                
            case CustomEffectType.AudioDamage:
                ApplyAudioDamage(card);
                break;
                
            case CustomEffectType.CatEarHeadset:
                ApplyCatEarHeadset(card);
                break;
                
            case CustomEffectType.BrokenCompass:
                ApplyBrokenCompass(card);
                break;
                
            case CustomEffectType.WeaponSwitch:
                ApplyWeaponSwitch(card);
                break;
                
            case CustomEffectType.EnemyModifier:
                ApplyEnemyModifier(card);
                break;
        }
    }

    /// <summary>
    /// 1. 视野受限 - 短暂减小摄像机视距
    /// customEffectValue: 视距缩小倍数（例如 0.5 = 50% 视距）
    /// </summary>
    private void ApplyLimitedVision(PropertyCard card)
    {
        if (mainCam == null)
        {
            Debug.LogWarning("[CustomEffectHandler] 摄像机未找到，无法应用视野受限");
            return;
        }

        // 停止旧的协程
        if (limitedVisionCoroutine != null)
        {
            StopCoroutine(limitedVisionCoroutine);
        }

        float targetSize = originalOrthographicSize * card.customEffectValue;
        limitedVisionCoroutine = StartCoroutine(LimitedVisionCoroutine(targetSize, card.customEffectDuration));
    }

    private System.Collections.IEnumerator LimitedVisionCoroutine(float targetSize, float duration)
    {
        float elapsed = 0f;
        float startSize = mainCam.orthographicSize;

        // 缩小视距
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            mainCam.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            yield return null;
        }

        // 恢复视距
        elapsed = 0f;
        startSize = mainCam.orthographicSize;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.5f;
            mainCam.orthographicSize = Mathf.Lerp(startSize, originalOrthographicSize, t);
            yield return null;
        }

        mainCam.orthographicSize = originalOrthographicSize;
        Debug.Log("[CustomEffectHandler] 视野受限效果结束");
    }

    /// <summary>
    /// 2. 耳机损耗 - 短暂降低音频音量
    /// customEffectValue: 音量缩小倍数（例如 0.3 = 30% 音量）
    /// </summary>
    private void ApplyAudioDamage(PropertyCard card)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("[CustomEffectHandler] 音频源未找到，无法应用耳机损耗");
            return;
        }

        StartCoroutine(AudioDamageCoroutine(card.customEffectValue, card.customEffectDuration));
    }

    private System.Collections.IEnumerator AudioDamageCoroutine(float volumeMultiplier, float duration)
    {
        float originalVolume = audioSource.volume;
        float targetVolume = originalVolume * volumeMultiplier;
        float elapsed = 0f;

        // 降低音量
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            audioSource.volume = Mathf.Lerp(originalVolume, targetVolume, t);
            yield return null;
        }

        // 恢复音量
        elapsed = 0f;
        float currentVolume = audioSource.volume;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.5f;
            audioSource.volume = Mathf.Lerp(currentVolume, originalVolume, t);
            yield return null;
        }

        audioSource.volume = originalVolume;
        Debug.Log("[CustomEffectHandler] 耳机损耗效果结束");
    }

    /// <summary>
    /// 3. 猫耳耳机 - 随机播放列表中的音频
    /// 从 randomAudioClips 中随机选择一个音频播放
    /// </summary>
    private void ApplyCatEarHeadset(PropertyCard card)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("[CustomEffectHandler] 音频源未找到，无法应用猫耳耳机");
            return;
        }

        if (card.randomAudioClips == null || card.randomAudioClips.Count == 0)
        {
            Debug.LogWarning("[CustomEffectHandler] 猫耳耳机没有配置音频列表");
            return;
        }

        // 如果之前有协程在运行，先停止
        if (catEarHeadsetCoroutine != null)
        {
            StopCoroutine(catEarHeadsetCoroutine);
            audioSource.Stop(); // 停止当前播放
        }

        // 随机选择音频并开始循环播放
        catEarHeadsetCoroutine = StartCoroutine(CatEarHeadsetCoroutine(card, card.customEffectDuration));
    }

    private System.Collections.IEnumerator CatEarHeadsetCoroutine(PropertyCard card, float duration)
    {
        // 随机选择音频
        AudioClip selectedClip = card.randomAudioClips[Random.Range(0, card.randomAudioClips.Count)];
        
        // 设置循环播放
        audioSource.clip = selectedClip;
        audioSource.loop = true;
        audioSource.Play();
        
        Debug.Log($"[CustomEffectHandler] 开始循环播放猫耳耳机音频: {selectedClip.name}，持续时间: {duration}s");

        // 等待持续时间
        yield return new WaitForSeconds(duration);

        // 停止播放并清理
        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = null;
        catEarHeadsetCoroutine = null;
        
        Debug.Log($"[CustomEffectHandler] 停止猫耳耳机音频播放: {selectedClip.name}");
    }

    /// <summary>
    /// 4. 失灵指南针 - 短暂颠倒玩家运动方向
    /// </summary>
    private void ApplyBrokenCompass(PropertyCard card)
    {
        if (brokenCompassCoroutine != null)
        {
            StopCoroutine(brokenCompassCoroutine);
        }

        brokenCompassCoroutine = StartCoroutine(BrokenCompassCoroutine(card.customEffectDuration));
    }

    private System.Collections.IEnumerator BrokenCompassCoroutine(float duration)
    {
        directionReversed = true;
        Debug.Log("[CustomEffectHandler] 失灵指南针激活 - 运动方向已颠倒");

        yield return new WaitForSeconds(duration);

        directionReversed = false;
        Debug.Log("[CustomEffectHandler] 失灵指南针效果结束");
    }

    /// <summary>
    /// 获取是否方向被颠倒
    /// </summary>
    public bool IsDirectionReversed()
    {
        return directionReversed;
    }

    /// <summary>
    /// 5. 以旧换新 - 更改玩家手持武器数据源
    /// </summary>
    private void ApplyWeaponSwitch(PropertyCard card)
    {
        if (card.replacementWeapon == null)
        {
            Debug.LogWarning("[CustomEffectHandler] 以旧换新没有设置替换武器");
            return;
        }

        if (playerControl == null)
        {
            Debug.LogError("[CustomEffectHandler] PlayerControl未找到，无法更换武器");
            return;
        }

        bool success = playerControl.SwitchWeaponData(card.replacementWeapon);
        if (success)
        {
            Debug.Log($"[CustomEffectHandler] 成功更换武器: {card.replacementWeapon.weaponName}");
        }
        else
        {
            Debug.LogWarning($"[CustomEffectHandler] 武器更换失败");
        }
    }

    /// <summary>
    /// 6. 敌人控制 - 调整所有敌人的速度和伤害
    /// customEffectValue: 速度倍数
    /// customEffectValue2: 伤害倍数
    /// </summary>
    private void ApplyEnemyModifier(PropertyCard card)
    {
        var enemies = FindObjectsByType<EnemyControl>(FindObjectsSortMode.None);
        
        Debug.Log($"[CustomEffectHandler] 应用敌人控制效果 - 找到 {enemies.Length} 个敌人");
        Debug.Log($"[CustomEffectHandler] 速度倍数: {card.customEffectValue}, 伤害倍数: {card.customEffectValue2}");

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            // 保存原始值
            if (!modifiedEnemies.ContainsKey(enemy))
            {
                modifiedEnemies[enemy] = (enemy.moveSpeed, enemy.attackDamage);
            }

            // 应用修改
            enemy.moveSpeed *= card.customEffectValue;
            enemy.attackDamage *= card.customEffectValue2;

            Debug.Log($"[CustomEffectHandler] 修改敌人 {enemy.name} - " +
                     $"速度: {enemy.moveSpeed:F2}, 伤害: {enemy.attackDamage}");
        }
    }

    /// <summary>
    /// 恢复所有被修改的敌人到原始状态
    /// </summary>
    public void RestoreEnemyModifiers()
    {
        foreach (var kvp in modifiedEnemies)
        {
            if (kvp.Key != null)
            {
                kvp.Key.moveSpeed = kvp.Value.originalSpeed;
                kvp.Key.attackDamage = kvp.Value.originalDamage;
            }
        }

        modifiedEnemies.Clear();
        Debug.Log("[CustomEffectHandler] 所有敌人已恢复到原始状态");
    }

    /// <summary>
    /// 游戏结束时清理所有效果
    /// </summary>
    public void ClearAllEffects()
    {
        // 恢复视距
        if (mainCam != null)
        {
            mainCam.orthographicSize = originalOrthographicSize;
        }

        // 恢复音量并停止猫耳耳机音频
        if (audioSource != null)
        {
            audioSource.volume = 1f;
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }

        // 停止所有协程
        if (limitedVisionCoroutine != null)
        {
            StopCoroutine(limitedVisionCoroutine);
        }

        if (catEarHeadsetCoroutine != null)
        {
            StopCoroutine(catEarHeadsetCoroutine);
            catEarHeadsetCoroutine = null;
        }

        if (brokenCompassCoroutine != null)
        {
            StopCoroutine(brokenCompassCoroutine);
        }

        // 恢复敌人状态
        RestoreEnemyModifiers();

        // 恢复方向
        directionReversed = false;

        Debug.Log("[CustomEffectHandler] 所有自定义效果已清理");
    }
}

using UnityEngine;
using Y_Survivor;
using System.Collections.Generic;

// 检测Cinemachine命名空间是否存在
using System;

public class CustomEffectHandler : MonoBehaviour
{
    private PlayerControl playerControl;
    private PlayerPropertyManager playerPropertyManager;
    private Camera mainCam;
    private AudioSource audioSource;
    
    // 视野受限相关
    private Coroutine limitedVisionCoroutine;
    
    // 失灵指南针相关
    private bool directionReversed = false;
    private Coroutine brokenCompassCoroutine;
    
    // 猫耳耳机相关
    private Coroutine catEarHeadsetCoroutine;
    private AudioClip currentCatEarClip; // 当前播放的猫耳耳机音频

    // 敌人控制相关
    private Dictionary<EnemyControl, (float originalSpeed, float originalDamage)> modifiedEnemies 
        = new Dictionary<EnemyControl, (float, float)>();

    private void Awake()
    {
        playerControl = GetComponent<PlayerControl>();
        playerPropertyManager = GetComponent<PlayerPropertyManager>();
        mainCam = Camera.main;
        audioSource = GetComponent<AudioSource>();
        
        // 如果组件上没有AudioSource，尝试查找场景中的AudioSource
        if (audioSource == null)
        {
            AudioSource[] sceneAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            if (sceneAudioSources.Length > 0)
            {
                // 优先选择标记为"Music"或"Background"的AudioSource，否则选择第一个
                foreach (AudioSource source in sceneAudioSources)
                {
                    if (source.gameObject.name.Contains("Music") || source.gameObject.name.Contains("Background") || source.gameObject.tag == "Music")
                    {
                        audioSource = source;
                        Debug.Log($"[CustomEffectHandler] 找到场景音乐AudioSource: {source.gameObject.name}");
                        break;
                    }
                }
                
                // 如果没找到标记的AudioSource，使用第一个可用的
                if (audioSource == null)
                {
                    audioSource = sceneAudioSources[0];
                    Debug.Log($"[CustomEffectHandler] 使用场景中第一个AudioSource: {audioSource.gameObject.name}");
                }
            }
            else
            {
                Debug.LogWarning("[CustomEffectHandler] 场景中未找到任何AudioSource，自定义音频效果将不可用");
            }
        }
        
        if (mainCam == null)
        {
            mainCam = FindAnyObjectByType<Camera>();
        }

        Debug.Log($"[CustomEffectHandler] 初始化完成 - 相机: {(mainCam != null ? "找到" : "未找到")}");
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
    /// 1. 视野受限 - 短暂关闭摄像机
    /// customEffectDuration: 关闭持续时间（秒）
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

        Debug.Log($"[CustomEffectHandler] 应用视野受限 - 关闭摄像机，持续时间: {card.customEffectDuration}s");

        limitedVisionCoroutine = StartCoroutine(LimitedVisionCoroutine(card.customEffectDuration));
    }

    private System.Collections.IEnumerator LimitedVisionCoroutine(float duration)
    {
        // 关闭摄像机
        mainCam.enabled = false;
        Debug.Log("[CustomEffectHandler] 摄像机已关闭");

        // 等待持续时间（使用 WaitForSecondsRealtime 以处理 timeScale=0 的情况）
        yield return new WaitForSecondsRealtime(duration);

        // 恢复摄像机
        if (mainCam != null)
        {
            mainCam.enabled = true;
            Debug.Log("[CustomEffectHandler] 摄像机已恢复");
        }
        
        Debug.Log("[CustomEffectHandler] 视野受限效果结束");
    }

    /// <summary>
    /// 2. 耳机损耗 - 短暂关闭场景AudioListener（无需ScriptableObject参数）
    /// customEffectValue参数可选，默认仅关闭Listener
    /// </summary>
    private void ApplyAudioDamage(PropertyCard card)
    {
        // 尝试查找AudioListener
        AudioListener audioListener = FindAnyObjectByType<AudioListener>();
        if (audioListener == null)
        {
            Debug.LogWarning("[CustomEffectHandler] AudioListener未找到，无法应用耳机损耗");
            return;
        }

        StartCoroutine(AudioDamageCoroutine(audioListener, card.customEffectDuration));
    }

    private System.Collections.IEnumerator AudioDamageCoroutine(AudioListener audioListener, float duration)
    {
        // 关闭AudioListener（使所有音频静音）
        audioListener.enabled = false;
        Debug.Log("[CustomEffectHandler] 耳机损耗激活 - AudioListener已关闭，所有音频静音");

        // 等待持续时间（使用 WaitForSecondsRealtime 以处理 timeScale=0 的情况）
        yield return new WaitForSecondsRealtime(duration);

        // 恢复AudioListener
        if (audioListener != null)
        {
            audioListener.enabled = true;
            Debug.Log("[CustomEffectHandler] 耳机损耗效果结束 - AudioListener已恢复");
        }
    }

    /// <summary>
    /// 3. 猫耳耳机 - 随机播放音频（无需ScriptableObject引用）
    /// 歌曲循环播放，直到再次抽到猫耳耳机卡牌时换歌
    /// 自动从Resources/Audio/CatEarHeadset目录加载随机音频
    /// </summary>
    private void ApplyCatEarHeadset(PropertyCard card)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("[CustomEffectHandler] 音频源未找到，无法应用猫耳耳机");
            return;
        }

        // 尝试从PropertyCard获取音频列表，如果为空则从Resources加载
        List<AudioClip> audioClips = card.randomAudioClips;
        if (audioClips == null || audioClips.Count == 0)
        {
            // "假传入" - 从Resources目录动态加载
            audioClips = new List<AudioClip>(Resources.LoadAll<AudioClip>("Audio/CatEarHeadset"));

            if (audioClips.Count == 0)
            {
                Debug.LogWarning("[CustomEffectHandler] 猫耳耳机: 未在PropertyCard中设置音频列表，也未在Resources/Audio/CatEarHeadset找到音频");
                return;
            }

            Debug.Log($"[CustomEffectHandler] 猫耳耳机: 从Resources加载 {audioClips.Count} 个音频");
        }

        // 如果已经在播放猫耳耳机音乐，停止当前协程准备换歌
        if (catEarHeadsetCoroutine != null)
        {
            StopCoroutine(catEarHeadsetCoroutine);
            audioSource.Stop();
            Debug.Log($"[CustomEffectHandler] 停止当前猫耳耳机音频: {currentCatEarClip?.name ?? "无"}");
        }

        catEarHeadsetCoroutine = StartCoroutine(CatEarHeadsetCoroutine(audioClips));
    }

    private System.Collections.IEnumerator CatEarHeadsetCoroutine(List<AudioClip> audioClips)
    {
        // 随机选择音频（避免重复播放同一首歌）
        AudioClip selectedClip;
        if (audioClips.Count > 1 && currentCatEarClip != null)
        {
            // 如果有多首歌且当前有播放记录，尝试选择不同的歌
            do
            {
                selectedClip = audioClips[UnityEngine.Random.Range(0, audioClips.Count)];
            } while (selectedClip == currentCatEarClip);
        }
        else
        {
            selectedClip = audioClips[UnityEngine.Random.Range(0, audioClips.Count)];
        }

        currentCatEarClip = selectedClip;

        // 设置循环播放
        audioSource.clip = selectedClip;
        audioSource.loop = true;
        audioSource.Play();

        Debug.Log($"[CustomEffectHandler] 开始循环播放猫耳耳机音频: {selectedClip.name}（持续循环直到下次换歌）");

        // 无限循环，等待协程被外部停止
        while (true)
        {
            yield return null;
        }
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

        Debug.Log($"[CustomEffectHandler] 应用失灵指南针 - 颠倒方向，持续时间: {card.customEffectDuration}s");
        brokenCompassCoroutine = StartCoroutine(BrokenCompassCoroutine(card.customEffectDuration));
    }

    private System.Collections.IEnumerator BrokenCompassCoroutine(float duration)
    {
        directionReversed = true;
        Debug.Log("[CustomEffectHandler] 失灵指南针激活 - 运动方向已颠倒");

        yield return new WaitForSecondsRealtime(duration);

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
    /// 5. 以旧换新 - 更改玩家手持武器数据源（无需ScriptableObject引用）
    /// 自动从CardPoolManager的weaponCards列表中选择武器
    /// </summary>
    private void ApplyWeaponSwitch(PropertyCard card)
    {
        if (playerControl == null)
        {
            Debug.LogError("[CustomEffectHandler] PlayerControl未找到，无法更换武器");
            return;
        }

        Weapon replacementWeapon = null;

        // 优先使用PropertyCard中设置的武器
        if (card.replacementWeapon != null)
        {
            replacementWeapon = card.replacementWeapon;
            Debug.Log("[CustomEffectHandler] 以旧换新: 使用PropertyCard中设置的武器");
        }
        else
        {
            // "假传入" - 从CardPoolManager获取武器
            int weaponId = Mathf.RoundToInt(card.customEffectValue);
            if (weaponId > 0 && CardPoolManager.Instance != null)
            {
                // 从CardPoolManager的weaponCards列表中选择武器
                var weaponCards = CardPoolManager.Instance.weaponCards;
                if (weaponCards != null && weaponCards.Count > 0)
                {
                    // 使用weaponId作为索引（从1开始，所以要减1）
                    int weaponIndex = weaponId - 1;
                    if (weaponIndex >= 0 && weaponIndex < weaponCards.Count)
                    {
                        replacementWeapon = weaponCards[weaponIndex];
                        Debug.Log($"[CustomEffectHandler] 以旧换新: 从CardPoolManager选择武器 - 索引{weaponIndex}: {replacementWeapon.weaponName}");
                    }
                    else
                    {
                        // 如果索引超出范围，随机选择一个武器
                        replacementWeapon = weaponCards[UnityEngine.Random.Range(0, weaponCards.Count)];
                        Debug.Log($"[CustomEffectHandler] 以旧换新: 索引超出范围，随机选择武器: {replacementWeapon.weaponName}");
                    }
                }
                else
                {
                    Debug.LogWarning("[CustomEffectHandler] 以旧换新: CardPoolManager.weaponCards为空");
                    return;
                }
            }
            else
            {
                Debug.LogWarning("[CustomEffectHandler] 以旧换新: 未设置replacementWeapon，且CardPoolManager不可用或customEffectValue无效");
                return;
            }
        }

        bool success = playerControl.SwitchWeaponData(replacementWeapon);
        if (success)
        {
            Debug.Log($"[CustomEffectHandler] 成功更换武器: {replacementWeapon.weaponName}");
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
        // 恢复摄像机
        if (mainCam != null)
        {
            mainCam.enabled = true;
        }

        // 恢复音量并停止猫耳耳机音频
        if (audioSource != null)
        {
            audioSource.volume = 1f;
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }

        // 清理猫耳耳机状态
        currentCatEarClip = null;

        // 清理猫耳耳机状态
        currentCatEarClip = null;

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

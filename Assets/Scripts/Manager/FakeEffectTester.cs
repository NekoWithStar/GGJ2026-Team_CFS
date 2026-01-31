using UnityEngine;
using Y_Survivor;

/// <summary>
/// 假传入效果测试器 - 不使用PropertyCard，直接测试自定义效果
/// 用于验证效果逻辑，无需美术资源
/// </summary>
public class FakeEffectTester : MonoBehaviour
{
    private CustomEffectHandler customEffectHandler;
    
    private void Start()
    {
        customEffectHandler = GetComponent<CustomEffectHandler>();
        if (customEffectHandler == null)
        {
            Debug.LogError("[FakeEffectTester] 找不到CustomEffectHandler组件");
        }
    }
    
    private void Update()
    {
        // 按键测试各个效果
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestCatEarHeadset();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestBrokenCompass();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TestWeaponSwitch();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TestAudioDamage();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            TestLimitedVision();
        }
    }
    
    /// <summary>
    /// 测试猫耳耳机效果（按1）
    /// </summary>
    private void TestCatEarHeadset()
    {
        Debug.Log("[FakeEffectTester] 触发猫耳耳机效果");
        
        // 创建临时PropertyCard来传入参数
        var fakeCard = ScriptableObject.CreateInstance<PropertyCard>();
        fakeCard.hasCustomEffect = true;
        fakeCard.customEffectType = CustomEffectType.CatEarHeadset;
        fakeCard.customEffectDuration = EffectConfig.CAT_EAR_HEADSET_DURATION;
        
        // 设置音频列表（从Resources加载示例）
        fakeCard.randomAudioClips.Clear();
        
        // 尝试从Resources加载音频
        var audios = Resources.LoadAll<AudioClip>("Sounds");
        if (audios.Length > 0)
        {
            fakeCard.randomAudioClips.AddRange(audios);
            Debug.Log($"[FakeEffectTester] 加载了 {audios.Length} 个音频");
        }
        else
        {
            Debug.LogWarning("[FakeEffectTester] 未找到Resources/Sounds目录中的音频");
        }
        
        customEffectHandler.HandleCustomEffect(fakeCard);
        Destroy(fakeCard);
    }
    
    /// <summary>
    /// 测试失灵指南针效果（按2）
    /// </summary>
    private void TestBrokenCompass()
    {
        Debug.Log("[FakeEffectTester] 触发失灵指南针效果");
        
        var fakeCard = ScriptableObject.CreateInstance<PropertyCard>();
        fakeCard.hasCustomEffect = true;
        fakeCard.customEffectType = CustomEffectType.BrokenCompass;
        fakeCard.customEffectDuration = EffectConfig.BROKEN_COMPASS_DURATION;
        
        customEffectHandler.HandleCustomEffect(fakeCard);
        Destroy(fakeCard);
    }
    
    /// <summary>
    /// 测试以旧换新效果（按3）
    /// </summary>
    private void TestWeaponSwitch()
    {
        Debug.Log("[FakeEffectTester] 触发以旧换新效果");
        
        var fakeCard = ScriptableObject.CreateInstance<PropertyCard>();
        fakeCard.hasCustomEffect = true;
        fakeCard.customEffectType = CustomEffectType.WeaponSwitch;
        fakeCard.customEffectDuration = EffectConfig.WEAPON_SWITCH_DURATION;
        
        // 设置替换武器（可从Resources加载）
        var alternativeWeapon = EffectConfig.GetAlternativeWeapon();
        if (alternativeWeapon == null)
        {
            // 从Resources加载任意Weapon
            var weapons = Resources.LoadAll<Weapon>("Weapons");
            if (weapons.Length > 0)
            {
                fakeCard.replacementWeapon = weapons[UnityEngine.Random.Range(0, weapons.Length)];
                Debug.Log($"[FakeEffectTester] 加载替换武器: {fakeCard.replacementWeapon.weaponName}");
            }
            else
            {
                Debug.LogWarning("[FakeEffectTester] 未找到Resources/Weapons目录中的武器");
            }
        }
        else
        {
            fakeCard.replacementWeapon = alternativeWeapon;
        }
        
        customEffectHandler.HandleCustomEffect(fakeCard);
        Destroy(fakeCard);
    }
    
    /// <summary>
    /// 测试耳机损耗效果（按4）
    /// </summary>
    private void TestAudioDamage()
    {
        Debug.Log("[FakeEffectTester] 触发耳机损耗效果");
        
        var fakeCard = ScriptableObject.CreateInstance<PropertyCard>();
        fakeCard.hasCustomEffect = true;
        fakeCard.customEffectType = CustomEffectType.AudioDamage;
        fakeCard.customEffectValue = EffectConfig.AUDIO_DAMAGE_VOLUME_MULTIPLIER;
        fakeCard.customEffectDuration = EffectConfig.AUDIO_DAMAGE_DURATION;
        
        customEffectHandler.HandleCustomEffect(fakeCard);
        Destroy(fakeCard);
    }
    
    /// <summary>
    /// 测试视野受限效果（按5）
    /// </summary>
    private void TestLimitedVision()
    {
        Debug.Log("[FakeEffectTester] 触发视野受限效果");
        
        var fakeCard = ScriptableObject.CreateInstance<PropertyCard>();
        fakeCard.hasCustomEffect = true;
        fakeCard.customEffectType = CustomEffectType.LimitedVision;
        fakeCard.customEffectDuration = EffectConfig.LIMITED_VISION_DURATION;
        
        customEffectHandler.HandleCustomEffect(fakeCard);
        Destroy(fakeCard);
    }
    
    /// <summary>
    /// 获取键盘快捷键说明
    /// </summary>
    public static void PrintHotkeys()
    {
        Debug.Log("=== 假传入效果测试快捷键 ===");
        Debug.Log("按 1: 猫耳耳机 (持续 " + EffectConfig.CAT_EAR_HEADSET_DURATION + "s)");
        Debug.Log("按 2: 失灵指南针 (持续 " + EffectConfig.BROKEN_COMPASS_DURATION + "s)");
        Debug.Log("按 3: 以旧换新 (持续 " + EffectConfig.WEAPON_SWITCH_DURATION + "s)");
        Debug.Log("按 4: 耳机损耗 (持续 " + EffectConfig.AUDIO_DAMAGE_DURATION + "s，音量 " + EffectConfig.AUDIO_DAMAGE_VOLUME_MULTIPLIER + ")");
        Debug.Log("按 5: 视野受限 (持续 " + EffectConfig.LIMITED_VISION_DURATION + "s)");
    }
}

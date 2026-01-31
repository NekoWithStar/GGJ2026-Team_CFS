# 无PropertyCard效果系统 - 使用指南

## 概述

本系统提供了**3种不同的效果调用方式**，允许您选择最适合的实现方式，从而最小化对ScriptableObject的依赖。

---

## 📋 三种使用方式

### 方式一：假传入效果测试器（FakeEffectTester）
**用途**：快速测试效果，临时创建PropertyCard
**依赖**：需要创建临时PropertyCard对象
**优点**：易于调试，接近实际使用

```csharp
// 在PlayerControl或其他地方调用
var tester = GetComponent<FakeEffectTester>();
tester.TestCatEarHeadset();      // 按1
tester.TestBrokenCompass();      // 按2
tester.TestWeaponSwitch();       // 按3
tester.TestAudioDamage();        // 按4
tester.TestLimitedVision();      // 按5
```

### 方式二：直接效果调用器（DirectEffectCaller）
**用途**：完全不使用PropertyCard，直接调用效果
**依赖**：无需PropertyCard
**优点**：最简洁，零ScriptableObject依赖

```csharp
// 初始化（在启动时调用一次）
DirectEffectCaller.Initialize(customEffectHandler);

// 直接调用效果
DirectEffectCaller.ApplyCatEarHeadset(audioClips);
DirectEffectCaller.ApplyBrokenCompass();
DirectEffectCaller.ApplyWeaponSwitch(weapon);
DirectEffectCaller.ApplyAudioDamage(0.3f);      // 音量30%
DirectEffectCaller.ApplyLimitedVision();
DirectEffectCaller.ApplyEnemyModifier(0.5f, 1.5f);  // 速度50%，伤害150%
```

### 方式三：直接方法调用（CustomEffectHandler内）
**用途**：最直接的调用方式
**依赖**：无需PropertyCard
**优点**：最灵活，可完全自定义参数

```csharp
// 获取CustomEffectHandler引用
var handler = GetComponent<CustomEffectHandler>();

// 直接调用方法
handler.ApplyCatEarHeadsetDirect(audioClips, 3f);
handler.ApplyBrokenCompassDirect(2.5f);
handler.ApplyWeaponSwitchDirect(weapon);
handler.ApplyAudioDamageDirect(0.3f, 4f);
handler.ApplyLimitedVisionDirect(3f);
handler.ApplyEnemyModifierDirect(0.5f, 1.5f, 3f);
```

---

## ⚙️ 效果配置参数

所有效果的默认参数定义在 `EffectConfig.cs` 中：

```csharp
// 猫耳耳机
CAT_EAR_HEADSET_DURATION = 3f

// 失灵指南针
BROKEN_COMPASS_DURATION = 2.5f

// 以旧换新
WEAPON_SWITCH_DURATION = 5f

// 耳机损耗
AUDIO_DAMAGE_DURATION = 4f
AUDIO_DAMAGE_VOLUME_MULTIPLIER = 0.3f

// 视野受限
LIMITED_VISION_DURATION = 3f

// 敌人控制
ENEMY_MODIFIER_DURATION = 3f
ENEMY_SPEED_MULTIPLIER = 0.5f
ENEMY_DAMAGE_MULTIPLIER = 1.5f
```

---

## 🔧 集成步骤

### 步骤1：在PlayerControl中初始化
```csharp
private void Awake()
{
    // ... 其他初始化代码 ...
    
    var customEffectHandler = GetComponent<CustomEffectHandler>();
    DirectEffectCaller.Initialize(customEffectHandler);
}
```

### 步骤2：从任何地方调用效果
```csharp
// 例如：收集到某个物品时触发效果
public void OnPickupItem(ItemType type)
{
    switch(type)
    {
        case ItemType.CatEarHeadset:
            DirectEffectCaller.ApplyCatEarHeadset(audioList);
            break;
            
        case ItemType.CompassBreaker:
            DirectEffectCaller.ApplyBrokenCompass();
            break;
    }
}
```

---

## 📊 方式对比表

| 特性 | FakeEffectTester | DirectEffectCaller | 直接方法 |
|------|-----------------|-------------------|---------|
| PropertyCard依赖 | 有（临时创建）| 无 | 无 |
| 易用性 | ⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| 灵活性 | ⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| 性能 | 一般 | 好 | 最好 |
| 测试调试 | 最佳 | 好 | 一般 |

---

## 🎮 快速测试

### 使用FakeEffectTester快速测试：

1. 在PlayerControl对象上添加 `FakeEffectTester` 组件
2. 运行游戏，按下快捷键：
   - **按1**：猫耳耳机（需要Resources/Sounds中有音频）
   - **按2**：失灵指南针
   - **按3**：以旧换新（需要Resources/Weapons中有武器）
   - **按4**：耳机损耗
   - **按5**：视野受限

---

## ⚠️ 注意事项

### 1. 音频加载
对于猫耳耳机效果，需要在以下位置放置音频：
```
Assets/Resources/Sounds/
├── meow_1.wav
├── meow_2.wav
├── meow_3.wav
└── purr_1.wav
```

### 2. 武器加载
对于以旧换新效果，需要在以下位置放置武器：
```
Assets/Resources/Weapons/
└── alternative_weapon.asset
```

### 3. 原有PropertyCard系统兼容
新系统**完全兼容**原有的PropertyCard系统：
- PropertyCard方式仍然工作
- 新的直接调用方式并行存在
- 可以混合使用两种方式

---

## 📝 示例代码

### 示例1：从NPC获得效果
```csharp
public void ReceiveEffectFromNPC(string effectType)
{
    switch(effectType)
    {
        case "cat_headset":
            var audioClips = Resources.LoadAll<AudioClip>("Sounds");
            DirectEffectCaller.ApplyCatEarHeadset(new List<AudioClip>(audioClips));
            break;
            
        case "broken_compass":
            DirectEffectCaller.ApplyBrokenCompass();
            break;
    }
}
```

### 示例2：环境触发效果
```csharp
private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("MysticalZone"))
    {
        // 触发失灵指南针效果
        DirectEffectCaller.ApplyBrokenCompass();
        
        // 同时触发敌人增强
        DirectEffectCaller.ApplyEnemyModifier(1.2f, 1.3f);
    }
}
```

### 示例3：时间间隔触发
```csharp
private float effectTimer = 0f;

private void Update()
{
    effectTimer += Time.deltaTime;
    
    if (effectTimer > 30f)  // 每30秒触发一次
    {
        DirectEffectCaller.ApplyAudioDamage(0.3f);
        effectTimer = 0f;
    }
}
```

---

## 🚀 总结

**推荐使用流程**：

1. **开发/调试阶段**：使用 `FakeEffectTester`
   - 快速验证效果
   - 查看控制台日志
   - 调整参数

2. **集成阶段**：切换到 `DirectEffectCaller` 或直接方法
   - 完全移除PropertyCard依赖
   - 提高性能
   - 代码更清晰

3. **混合使用**：如果需要保留PropertyCard系统
   - PropertyCard方式继续使用
   - 新触发器使用DirectEffectCaller
   - 两种方式并行存在

---

## ❓ 常见问题

**Q: 是否会影响原有的PropertyCard系统？**
A: 否，完全兼容。新系统是可选的，原有系统继续工作。

**Q: 性能会改善吗？**
A: 是的，直接调用方式避免了PropertyCard对象的创建和销毁。

**Q: 能否同时使用多种方式？**
A: 可以，三种方式可以混合使用，没有冲突。

**Q: 如何自定义效果参数？**
A: 编辑 `EffectConfig.cs` 中的常量，或直接调用方法时传入自定义参数。

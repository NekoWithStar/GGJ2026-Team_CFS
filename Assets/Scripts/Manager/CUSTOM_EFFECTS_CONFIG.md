# 自定义效果配置指南 - 无ScriptableObject引用方案

## 概述
自定义效果系统支持**两种配置模式**：
1. **直接配置** - 在PropertyCard的Inspector中设置引用
2. **自动加载** - 从Resources目录动态加载资源（无需ScriptableObject引用）

## 各效果配置方案

### 1️⃣ 猫耳耳机 (CatEarHeadset)

**功能**：循环播放随机背景音乐，持续指定时间

#### 方案A：直接配置（传统方式）
在PropertyCard Inspector中：
- `randomAudioClips`: 拖入要播放的AudioClip列表

#### 方案B：自动加载（推荐 - 无需ScriptableObject）
1. 在项目中创建文件夹：`Assets/Resources/Audio/CatEarHeadset/`
2. 将音频文件放入该文件夹
3. PropertyCard中`randomAudioClips`保持为空
4. 游戏时会自动从该文件夹随机加载音频

**示例目录结构**：
```
Assets/Resources/Audio/CatEarHeadset/
├── bgm_1.mp3
├── bgm_2.mp3
├── bgm_3.mp3
└── bgm_4.mp3
```

---

### 2️⃣ 耳机损耗 (AudioDamage)

**功能**：关闭场景AudioListener，使所有音频静音

#### 配置方式
- 无需任何额外配置
- `customEffectDuration`：效果持续时间（秒）
- 游戏会自动查找场景中的AudioListener并关闭

**注意**：
- 必须确保场景中存在一个AudioListener组件
- 通常在MainCamera或AudioManager对象上

---

### 3️⃣ 失灵指南针 (BrokenCompass)

**功能**：暂时反转玩家移动方向

#### 配置方式
- 无需任何额外配置
- `customEffectDuration`：效果持续时间（秒）

**工作原理**：
- 设置`directionReversed = true`
- PlayerControl的Move方法检测此标志，反转输入方向
- 持续时间结束后自动恢复

---

### 4️⃣ 以旧换新 (WeaponSwitch)

**功能**：临时切换玩家持有的武器

#### 方案A：直接配置（传统方式）
在PropertyCard Inspector中：
- `replacementWeapon`: 拖入要切换到的Weapon ScriptableObject

#### 方案B：自动加载（推荐 - 无需ScriptableObject）
1. 在项目中创建文件夹：`Assets/Resources/Weapons/`
2. 将武器文件放入该文件夹，命名为：`Weapon_1.asset`、`Weapon_2.asset` 等
3. PropertyCard中`replacementWeapon`保持为空
4. 设置`customEffectValue`为武器ID（如1、2、3...）
5. 游戏时会自动加载`Resources/Weapons/Weapon_{ID}.asset`

**示例目录结构**：
```
Assets/Resources/Weapons/
├── Weapon_1.asset (弓)
├── Weapon_2.asset (剑)
├── Weapon_3.asset (法杖)
└── Weapon_4.asset (锤子)
```

**配置示例**：
- 武器ID 1 → customEffectValue = 1.0
- 武器ID 2 → customEffectValue = 2.0
- 武器ID 3 → customEffectValue = 3.0

---

## 影响分析

### ✅ 无负面影响
- PropertyCard字段保留，现有数据不丢失
- 两种方案可共存（优先使用PropertyCard配置）
- 不影响序列化和Inspector显示

### ✨ 优势
- **灵活性**：可混合使用两种方案
- **模块化**：资源独立管理，便于版本控制
- **可扩展**：便于后续添加新效果或资源
- **开发效率**：避免频繁修改PropertyCard

---

## 优先级规则

对于支持自动加载的效果（猫耳耳机、以旧换新）：

```
优先级 1: 使用PropertyCard中的直接配置（如果不为空）
优先级 2: 从Resources目录自动加载
优先级 3: 效果不生效（输出警告日志）
```

---

## 控制台调试

检查效果是否正确加载：
```
[CustomEffectHandler] 猫耳耳机: 从Resources加载 4 个音频
[CustomEffectHandler] 以旧换新: 从Resources加载武器 - Weapons/Weapon_1
[CustomEffectHandler] 耳机损耗激活 - AudioListener已关闭，所有音频静音
```

---

## 推荐实践

| 场景 | 建议方案 |
|------|--------|
| 原型设计 | 使用PropertyCard直接配置 |
| 规模扩展 | 切换到Resources自动加载 |
| 资源管理 | 使用Addressables + Resources加载 |
| 线上更新 | 使用AssetBundles替换Resources |

---

## 技术细节

### Resources加载路径
```csharp
// 猫耳耳机
Resources.LoadAll<AudioClip>("Audio/CatEarHeadset")

// 以旧换新（根据ID）
Resources.Load<Weapon>($"Weapons/Weapon_{weaponId}")
```

### 查找场景组件
```csharp
// 耳机损耗 - 自动查找AudioListener
AudioListener audioListener = FindAnyObjectByType<AudioListener>();
```

---

## 常见问题

**Q: 能否完全移除PropertyCard中的这两个字段？**
A: 不建议。会导致现有PropertyCard资源丢失数据。建议保留字段但标记为可选。

**Q: Resources加载会影响性能吗？**
A: 首次加载时会有轻微开销，建议在游戏开始或菜单时预加载。

**Q: 能否同时使用PropertyCard配置和Resources加载？**
A: 可以，系统会优先使用PropertyCard配置。


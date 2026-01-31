# 🎮 卡牌系统与敌人管理完整配置指南

## 📋 系统概览

本指南涵盖以下功能：
1. **属性卡系统** - PropertyCard 控制游戏属性加成
2. **统一卡池管理** - CardPoolManager 管理武器卡和属性卡
3. **卡牌选择UI** - Flip_Card 支持属性卡确认
4. **敌人自动刷新** - EnemySpawner 管理敌人生成

---

## 1️⃣ 属性卡系统（PropertyCard）配置

### 创建属性卡
1. **Project窗口右键** → `Create` → `Y_Survivor/Property Card`
2. **配置内容**：
   - `cardName` - 卡片名称（如"伤害加强卡"）
   - `cardIcon` - 卡片图标（Sprite）
   - `rarity` - 稀有度（Common/Rare/Epic/Legendary）
   - `description` - 描述文本
   - `modifiers` - 属性修饰符列表

### 添加属性修饰符
1. 在属性卡的 `modifiers` 列表中点击 "+"
2. 配置每个修饰符：
   - `targetProperty` - 目标属性（如PlayerMoveSpeed、Damage等）
   - `modifierType` - 修饰类型（Add/Mul/AfterAdd）
   - `value` - 修饰值

**示例配置**：
```
属性卡："玩家移速+30%"
- targetProperty: PlayerMoveSpeed
- modifierType: Mul
- value: 0.3

属性卡："伤害+50"
- targetProperty: Damage
- modifierType: Add
- value: 50
```

---

## 2️⃣ CardPoolManager 配置

### 场景设置
1. **创建空物体** → 命名为 "CardPoolManager"
2. **添加脚本** `CardPoolManager.cs`
3. **Inspector 配置**：

```
=== 卡池设置 ===
Weapon Cards: [拖入所有武器卡资源]
Property Cards: [拖入所有属性卡资源]

=== 选择规则 ===
Cards To Show: 4 (每次显示4张卡）
Cards To Select: 1 (玩家选1张)
```

### 卡池使用
在其他脚本中使用：
```csharp
// 获取随机卡牌组
List<Card> randomCards = CardPoolManager.Instance.GetRandomCards(4);

// 获取单张随机武器卡
Weapon weapon = CardPoolManager.Instance.GetRandomWeaponCard();

// 获取单张随机属性卡
PropertyCard propCard = CardPoolManager.Instance.GetRandomPropertyCard();
```

---

## 3️⃣ 卡牌UI系统（Flip_Card）配置

### 武器卡UI预制体
1. **创建 UI Canvas** 并添加以下结构：
```
Canvas
├─ BackFace (背面)
│  └─ 显示"?"或普通卡背
└─ FrontFace (正面)
   ├─ IconImage (武器图标)
   ├─ NameText (武器名称)
   ├─ DamageText (伤害值)
   ├─ CooldownText (冷却时间)
   ├─ RangeText (范围)
   └─ DescribeText (描述)
```

2. **添加脚本**：
   - 整个Card按钮添加 `Flip_Card.cs`
   - FrontFace 添加 `WeaponCardControl.cs`
3. **Inspector 配置**：
```
=== Flip_Card ===
Front Face: [指向FrontFace]
Back Face: [指向BackFace]
Hover Scale: 1.08
Second Click Is Confirm: true

=== WeaponCardControl ===
weapon_data: [拖入Weapon资源]
icon: [指向IconImage]
weapon_name: [指向NameText]
damage: [指向DamageText]
cooldown: [指向CooldownText]
range: [指向RangeText]
describe: [指向DescribeText]
```

### 属性卡UI预制体
类似武器卡，结构如下：
```
Canvas
├─ BackFace (背面)
└─ FrontFace (正面)
   ├─ IconImage (属性卡图标)
   ├─ NameText (卡片名称)
   ├─ RarityText (稀有度)
   ├─ DescriptionText (描述)
   └─ ModifiersInfoText (修饰符信息，可选)
```

1. **整个Card按钮添加** `Flip_Card.cs`
2. **FrontFace 添加** `PropertyCardControl.cs`
3. **Inspector 配置**：
```
=== Flip_Card ===
Front Face: [指向FrontFace]
Back Face: [指向BackFace]
Second Click Is Confirm: true

=== PropertyCardControl ===
Property Card: [拖入PropertyCard资源]
icon: [指向IconImage]
card_name: [指向NameText]
rarity: [指向RarityText]
description: [指向DescriptionText]
modifiers_info: [指向ModifiersInfoText]
```

### 卡牌确认事件监听
1. **创建UI管理器**并添加以下脚本：
   - `WeaponCardPicker.cs` - 处理武器卡确认
   - `PropertyCardPicker.cs` - 处理属性卡确认

2. **PropertyCardPicker Inspector 配置**：
```
=== 应用对象 ===
Apply To Player: true (应用到玩家)
Apply To All Enemies: true (应用到所有敌人)
```

---

## 4️⃣ 敌人自动刷新（EnemySpawner）配置

### 场景设置
1. **创建空物体** → 命名为 "EnemySpawner"
2. **添加脚本** `EnemySpawner.cs`
3. **Inspector 配置**：

```
=== 敌人管理 ===
Enemy Prefabs: [拖入1-N个敌人预制体]
Initial Enemy Count: 3 (初始3个敌人)
Max Enemy Count: 10 (最多10个敌人)

=== 生成范围 ===
Target Camera: [指向MainCamera]
Spawn Distance: 2 (摄像机可见范围外2倍距离)

=== 刷新设置 ===
Enable Auto Spawn: true (启用自动刷新)
Spawn Interval: 3 (每3秒生成一个)
```

### 参数说明
- **Spawn Distance**: 值越大，敌人生成位置离摄像机越远
  - 1.0 = 恰好在摄像机边界外
  - 2.0 = 在摄像机视口外2倍距离（推荐）
  - 3.0+ = 更远的位置

- **Max Enemy Count**: 超过此数量将不再自动生成
  - 建议值：8-15（取决于性能）

---

## 5️⃣ 完整工作流示例

### 玩家选卡流程
1. **游戏触发升级/选卡**：
```csharp
// 获取随机4张卡牌
List<Card> randomCards = CardPoolManager.Instance.GetRandomCards(4);
// 显示给玩家选择（UI上显示4张卡）
```

2. **玩家点击卡牌**：
   - Flip_Card 翻转显示卡牌信息
   - 玩家再次点击确认

3. **确认事件触发**：
   - 武器卡 → WeaponCardPicker 处理 → 更换武器
   - 属性卡 → PropertyCardPicker 处理 → 应用加成

---

## 6️⃣ 常见问题解决

### 属性卡无法应用
- ✅ 确保 PropertyCard 中的 `modifiers` 列表不为空
- ✅ 确保 PlayerPropertyManager 和 EnemyPropertyManager 已挂载
- ✅ 检查修饰符的 `targetProperty` 是否有效

### 卡牌UI不显示
- ✅ 确保 Flip_Card 的 frontFace 和 backFace 指向正确的UI对象
- ✅ 检查 CardControl/WeaponCardControl/PropertyCardControl 是否正确添加到对应的UI对象
- ✅ 确保卡牌资源已正确拖入

### 敌人不生成
- ✅ 确保 EnemySpawner 中的 `enemyPrefabs` 列表不为空
- ✅ 确保摄像机标签为 "MainCamera" 或手动指定
- ✅ 检查 `enableAutoSpawn` 是否为 true

### 性能问题
- ✅ 降低 `maxEnemyCount` 的值
- ✅ 增加 `spawnInterval` 间隔
- ✅ 使用对象池技术重用敌人对象

---

## 7️⃣ API 快速参考

### CardPoolManager
```csharp
CardPoolManager.Instance.GetRandomCards(4);      // 获取随机4张卡
CardPoolManager.Instance.GetRandomWeaponCard();   // 获取随机武器卡
CardPoolManager.Instance.GetRandomPropertyCard(); // 获取随机属性卡
```

### PropertyCardPicker
```csharp
// 自动监听 Flip_Card.OnPropertyCardConfirmed 事件
// 无需手动调用
```

### EnemySpawner
```csharp
EnemySpawner.SpawnEnemies(5);          // 立即生成5个敌人
EnemySpawner.ClearAllEnemies();        // 清空所有敌人
EnemySpawner.SetAutoSpawnEnabled(false); // 停止自动刷新
```

---

## ✅ 验证清单

- [ ] PropertyCard 已创建并配置
- [ ] CardPoolManager 已配置武器卡和属性卡
- [ ] Flip_Card UI 预制体已创建（武器卡和属性卡）
- [ ] WeaponCardPicker 和 PropertyCardPicker 已添加到UI管理器
- [ ] EnemySpawner 已配置敌人预制体和参数
- [ ] 摄像机已正确配置
- [ ] 所有预制体和资源都已拖入 Inspector
- [ ] 编译无错误

完成以上配置后，系统即可正常运行！🎉

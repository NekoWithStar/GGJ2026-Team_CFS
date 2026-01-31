# 自定义效果系统完整指南

## 概述
项目中新增了6种自定义效果，这些效果无法通过数值修饰符实现，需要通过`CustomEffectHandler`进行特殊处理。

## 6种自定义效果详细说明

### 1. 视野受限 (LimitedVision)
**功能**: 短暂减小摄像机的正交投影大小（缩小视野范围）

**配置参数**:
- `customEffectValue`: 视距缩小倍数（0-1之间）
  - 例如: 0.5 = 缩小到50%的视距
- `customEffectDuration`: 效果持续时间（秒）

**实现逻辑**:
1. 开始时平滑缩小摄像机正交大小
2. 持续指定时间
3. 逐渐恢复到原始视距
4. 使用协程处理动画过程

**代码位置**: `CustomEffectHandler.ApplyLimitedVision()`

**使用示例**:
```
创建PropertyCard资源
- 启用自定义效果
- 选择 LimitedVision
- customEffectValue = 0.5（缩小50%）
- customEffectDuration = 3（持续3秒）
```

---

### 2. 耳机损耗 (AudioDamage)
**功能**: 短暂降低游戏音频音量

**配置参数**:
- `customEffectValue`: 音量缩小倍数（0-1之间）
  - 例如: 0.3 = 音量降至30%
- `customEffectDuration`: 效果持续时间（秒）

**实现逻辑**:
1. 保存原始音量
2. 平滑降低AudioSource音量
3. 持续指定时间
4. 逐渐恢复到原始音量
5. 使用协程处理动画过程

**代码位置**: `CustomEffectHandler.ApplyAudioDamage()`

**使用示例**:
```
创建PropertyCard资源
- 启用自定义效果
- 选择 AudioDamage
- customEffectValue = 0.3（降至30%）
- customEffectDuration = 2（持续2秒）
```

---

### 3. 猫耳耳机 (CatEarHeadset)
**功能**: 从指定列表中随机选择并播放一个音频

**配置参数**:
- `randomAudioClips`: 音频列表
  - 必须至少包含1个AudioClip
  - 系统会从列表中随机选择一个播放

**实现逻辑**:
1. 检查randomAudioClips列表是否为空
2. 使用Random.Range()随机选择列表中的音频
3. 使用AudioSource.PlayOneShot()播放（不中断当前音乐）

**代码位置**: `CustomEffectHandler.ApplyCatEarHeadset()`

**使用示例**:
```
创建PropertyCard资源
- 启用自定义效果
- 选择 CatEarHeadset
- randomAudioClips列表中添加多个音频文件
- 例如：cat_meow_1, cat_meow_2, cat_meow_3等
```

**注意**: 这个效果不需要Duration参数，音频播放完自动停止

---

### 4. 失灵指南针 (BrokenCompass)
**功能**: 短暂颠倒玩家的运动方向

**配置参数**:
- `customEffectDuration`: 效果持续时间（秒）
  - 例如: 2 = 运动方向颠倒2秒

**实现逻辑**:
1. 设置`directionReversed`标志为true
2. 在`MovePlayer()`中，如果标志为true，将移动方向取反（-moveDir）
3. 持续指定时间
4. 恢复标志为false

**代码修改**:
- `PlayerControl.MovePlayer()`: 检查失灵指南针状态并反转方向
- `CustomEffectHandler.IsDirectionReversed()`: 提供状态查询

**使用示例**:
```
创建PropertyCard资源
- 启用自定义效果
- 选择 BrokenCompass
- customEffectDuration = 2（颠倒2秒）
```

**玩家体验**:
- 按W键会向后移动（S方向）
- 按A键会向右移动（D方向）
- 视觉效果与玩家直观相反

---

### 5. 以旧换新 (WeaponSwitch)
**功能**: 立即更换玩家当前手持的武器

**配置参数**:
- `replacementWeapon`: 目标Weapon ScriptableObject
  - 必须指定一个有效的Weapon资源

**实现逻辑**:
1. 检查replacementWeapon是否为空
2. 调用`PlayerControl.SwitchWeaponData()`
3. 该方法会更换武器数据，保持之前应用的属性卡加成
4. 如果是自动开火武器，自动启动开火

**代码位置**: 
- `CustomEffectHandler.ApplyWeaponSwitch()`
- `PlayerControl.SwitchWeaponData()`

**使用示例**:
```
创建PropertyCard资源
- 启用自定义效果
- 选择 WeaponSwitch
- replacementWeapon = 选择一个Weapon资源（如"激光枪"）
- 玩家选择此卡后，武器立即切换
```

**优势**:
- 不销毁重建武器对象，保持属性加成
- 支持自动开火武器
- 完全集成到卡牌系统

---

### 6. 敌人控制 (EnemyModifier)
**功能**: 调整所有敌人的移动速度和伤害值

**配置参数**:
- `customEffectValue`: 敌人速度倍数
  - 例如: 0.5 = 敌人速度减半
  - 例如: 1.5 = 敌人速度提升50%
- `customEffectValue2`: 敌人伤害倍数
  - 例如: 0.8 = 敌人伤害降至80%
  - 例如: 2 = 敌人伤害翻倍

**实现逻辑**:
1. 查找场景中所有EnemyControl对象
2. 保存每个敌人的原始速度和伤害值
3. 应用倍数修改
4. 在游戏结束时自动恢复原始状态
5. 通过`modifiedEnemies`字典追踪修改

**代码位置**: 
- `CustomEffectHandler.ApplyEnemyModifier()`
- `CustomEffectHandler.RestoreEnemyModifiers()`

**使用示例**:
```
创建PropertyCard资源 - 削弱敌人
- 启用自定义效果
- 选择 EnemyModifier
- customEffectValue = 0.7（敌人速度降至70%）
- customEffectValue2 = 0.5（敌人伤害降至50%）

创建PropertyCard资源 - 强化敌人
- 启用自定义效果
- 选择 EnemyModifier
- customEffectValue = 1.2（敌人速度提升20%）
- customEffectValue2 = 1.5（敌人伤害提升50%）
```

**生效时间**: 立即生效，影响所有敌人

---

## 集成流程

### 1. 属性卡配置（PropertyCard）
```csharp
// 在PropertyCard Inspector中：
✓ 启用 hasCustomEffect
✓ 选择 customEffectType
✓ 配置对应参数
✓ 添加属性修饰符（可选）
```

### 2. 应用流程
```csharp
// CardPoolManager.ApplyCard()
→ PlayerControl.ApplyPropertyCard()
→ PlayerPropertyManager.ApplyPropertyCard()
  → CustomEffectHandler.HandleCustomEffect()
    → 根据类型调用相应的处理方法
```

### 3. 清理流程
```csharp
// PlayerControl.Die()
→ CustomEffectHandler.ClearAllEffects()
  → 恢复摄像机视距
  → 恢复音频音量
  → 恢复敌人状态
  → 取消所有协程
```

---

## 调试检查清单

### 视野受限
- [ ] 摄像机正确缩小并恢复
- [ ] 缩小倍数符合设置
- [ ] 持续时间准确

### 耳机损耗
- [ ] 音量正确降低和恢复
- [ ] 不影响音乐播放（如需要）
- [ ] 持续时间准确

### 猫耳耳机
- [ ] 音频列表已配置
- [ ] 随机播放成功
- [ ] 不中断背景音乐

### 失灵指南针
- [ ] 方向正确颠倒
- [ ] WASD反向表现一致
- [ ] 持续时间准确
- [ ] 恢复后正常控制

### 以旧换新
- [ ] 武器数据正确更换
- [ ] 属性加成保持不变
- [ ] 自动开火武器正确启动

### 敌人控制
- [ ] 所有敌人都被修改
- [ ] 速度和伤害倍数准确
- [ ] 游戏结束后正确恢复
- [ ] 新生成的敌人不受影响

---

## 扩展建议

### 新增效果的步骤：
1. 在`CustomEffectType.cs`中添加新枚举值
2. 在`PropertyCard.cs`的`GetCustomEffectDisplayName()`中添加显示名称
3. 在`CustomEffectHandler.cs`中添加处理方法
4. 在`HandleCustomEffect()`的switch语句中添加case分支

### 参数不足的解决方案：
如需超过2个参数，建议：
- 使用`customEffectValue`和`customEffectValue2`的组合
- 或创建新的参数字段（如List<float>）
- 或从replacementWeapon中获取额外数据

---

## 完整性验证

✅ 已实现6种效果的完整功能
✅ 每种效果都有适当的参数配置
✅ 所有效果都集成到属性卡系统
✅ 都具有恢复/清理机制
✅ 提供了调试日志
✅ 包含了使用示例和检查清单

所有效果都已校对并确认功能完整！

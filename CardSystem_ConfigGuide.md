# 卡牌系统配置指南

本文档介绍如何在Unity项目中配置卡牌系统，实现抽卡获取属性/武器的效果。适用于策划人员，无需编程知识。

## 概述

卡牌系统包含两种卡牌：
- **武器卡**：提供新武器装备
- **属性卡**：提供属性加成（如移动速度、攻击力等）

系统通过卡池随机抽取卡牌，玩家翻转确认后应用效果。

## 1. 创建武器卡

### 步骤：
1. 在Unity中，右键点击Project窗口 → Create → Y_Survivor → Weapon
2. 命名武器卡（如 "PistolCard"）
3. 在Inspector中配置：
   - **Name**: 武器名称（如 "手枪"）
   - **Description**: 武器描述
   - **Icon**: 武器图标图片
   - **Weapon Prefab**: 武器预制体（拖拽Weapon prefab到此字段）
   - **其他参数**: 根据武器类型设置射速、伤害等

### 注意：
- 武器预制体需要有WeaponControl组件
- 确保Weapon prefab已正确设置

## 2. 创建属性卡

### 步骤：
1. 在Unity中，右键点击Project窗口 → Create → Y_Survivor → Property Card
2. 命名属性卡（如 "SpeedBoostCard"）
3. 在Inspector中配置：
   - **Card Name**: 卡牌名称（如 "速度提升"）
   - **Description**: 卡牌描述
   - **Icon**: 卡牌图标
   - **Property Type**: 选择属性类型（如 MoveSpeed, AttackDamage 等）
   - **Value**: 加成数值（如 50f 表示+50%或固定值，具体看属性类型）
   - **Duration**: 持续时间（秒，0表示永久）

### 属性类型说明：
- **MoveSpeed**: 移动速度加成（百分比）
- **MaxHealth**: 最大血量加成（固定值）
- **AttackDamage**: 攻击伤害加成（百分比）
- **CritChance**: 暴击率加成（百分比）
- **CritDamageMultiplier**: 暴击伤害倍数加成
- **MeleeAttackRange**: 近战攻击范围加成
- **LargeEnemyMoveSpeed**: 大型敌人移动速度（影响敌人）
- 其他类型请参考PropertyType枚举

## 3. 配置卡池

### 步骤：
1. 在场景中创建空对象，命名为"CardPoolManager"
2. 添加CardPoolManager组件
3. 在Inspector中配置：
   - **Weapon Cards**: 拖拽所有武器卡资产到此列表
   - **Property Cards**: 拖拽所有属性卡资产到此列表

### 注意：
- 卡池会合并武器卡和属性卡，抽卡时随机从总池中选择
- 可以调整列表顺序影响随机性（但系统使用统一随机）

## 4. 设置UI和事件

### 卡牌翻转UI：
1. 在场景中放置Flip_Card预制体
2. 确保Flip_Card上有Flip_Card脚本
3. 配置：
   - **Card Control Prefab**: 选择对应的卡牌控制预制体
     - 武器卡用WeaponCardControl
     - 属性卡用PropertyCardControl

### 卡牌选择器：
1. 在场景中创建空对象，添加相应Picker组件：
   - **WeaponCardPicker**: 处理武器卡确认
   - **PropertyCardPicker**: 处理属性卡确认
2. 在Picker的Inspector中：
   - 确保PlayerControl引用正确（自动查找或手动指定）
   - 对于PropertyCardPicker，确保EnemySpawner引用（如果需要影响敌人）

### 注意：
- Flip_Card会自动广播确认事件
- Picker会监听事件并应用卡牌效果

## 5. 运行时配置

### 抽卡流程：
1. 调用CardPoolManager.GetRandomCards(int count)获取随机卡牌
2. 为每个卡牌实例化对应的UI控制组件
3. 显示翻转UI，等待玩家确认
4. 确认后自动应用效果

### 手动测试：
- 在场景中添加测试按钮，调用抽卡方法
- 使用WeaponDebugger查看当前属性和卡牌来源

## 6. 常见问题

### Q: 卡牌效果不生效？
A: 检查PropertyManager是否正确附加到Player/Enemy，确认属性类型匹配。

### Q: 武器不装备？
A: 确保Weapon prefab有WeaponControl组件，PlayerControl有weaponAttachPoint。

### Q: 卡牌UI不显示？
A: 检查Flip_Card的Card Control Prefab设置，确认预制体有相应Control脚本。

### Q: 属性不叠加？
A: 属性系统支持叠加，检查PropertyManager的appliedCards列表。

## 7. Coin系统与卡牌选择

### Coin获取
- 敌人死亡时自动掉落金币预制体
- 玩家拾取金币后coin数值增加
- coin用于解锁卡牌选择功能

### 卡牌选择触发
- 当coin达到100时，自动触发卡牌选择
- 游戏暂停：玩家无法移动、武器停止开火、敌人AI暂停
- 音乐继续播放（不受Time.timeScale影响）
- 显示卡牌选择窗口，展示3张随机卡牌

### 卡牌选择流程
1. 从CardPoolManager获取随机卡牌
2. 实例化卡牌UI（PropertyCardControl或WeaponCardControl）
3. 玩家选择卡牌后，自动应用效果
4. 消费100 coin
5. 恢复游戏：玩家、武器、敌人AI恢复正常

### 配置要求
- 场景中添加CardSelectionManager组件
- 设置cardSelectionPanel（UI面板）
- 设置cardContainer（卡牌容器）
- 设置cardPrefab（卡牌UI预制体）
- 敌人预制体设置coinPrefab和dropCoin值
- 确保PlayerControl有coin变量（自动存在）

### 注意事项
- 卡牌选择期间Time.timeScale = 0，但音乐AudioSource应设置ignoreListenerPause=true
- 选择后coin自动扣除，不足时清零
- 可通过CardSelectionManager.ShowCardSelection()手动触发测试
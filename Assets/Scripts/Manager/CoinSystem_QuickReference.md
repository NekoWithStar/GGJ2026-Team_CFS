# 🪙 金币系统快速参考

## ⚡ 30秒快速了解

**问题：** 项目中 3 个地方硬编码了金币配置（10、30、5）  
**解决：** 创建 `CoinSystemConfig` 统一管理器  
**结果：** 修改一个地方，影响整个系统 ✅

---

## 📍 三个核心参数

```csharp
coinThresholdForCardSelection = 10    // 金币达到此值触发卡牌选择
coinCostPerCard = 30                  // 应用卡牌消耗的金币
coinDropPerEnemy = 5                  // 敌人掉落的金币数
```

---

## 🔗 代码位置映射

| 原始位置 | 原始代码 | 现在使用 |
|---------|---------|--------|
| PlayerControl.cs:779 | `if (coin >= 10)` | `ShouldTriggerCardSelection()` |
| CardPoolManager.cs:28 | `coinCostPerCard = 30` | `GetCoinCostPerCard()` |
| EnemyControl.cs:25 | `dropCoin = 5` | `GetCoinDropPerEnemy()` |

---

## 🎮 常用方法

```csharp
// 检查是否应触发卡牌选择
CoinSystemConfig.Instance.ShouldTriggerCardSelection(coin)

// 检查金币是否足够
CoinSystemConfig.Instance.HasEnoughCoinForCard(coin)

// 获取配置值
CoinSystemConfig.Instance.GetCoinThresholdForCardSelection()
CoinSystemConfig.Instance.GetCoinCostPerCard()
CoinSystemConfig.Instance.GetCoinDropPerEnemy()

// 计算消耗后的金币
CoinSystemConfig.Instance.GetCoinAfterCardApplication(coin)

// 计算掉落后的金币
CoinSystemConfig.Instance.GetCoinAfterEnemyDrop(coin)
```

---

## 🧪 测试方法

右键点击 CoinSystemTest 组件，选择：
- **测试完整金币流程** - 运行所有测试
- **模拟：玩家拾取金币** - 查看触发逻辑
- **模拟：完整卡牌选择流程** - 完整流程模拟

---

## ⚙️ Inspector 配置

创建 CoinSystemConfig 实例后，在 Inspector 中调整：
1. Coin Threshold For Card Selection
2. Coin Cost Per Card
3. Coin Drop Per Enemy
4. Debug Mode (启用日志)
5. Skip Coin Check (调试用)

---

## 🐛 调试技巧

### 启用详细日志
```csharp
CoinSystemConfig.Instance.ToggleDebugMode();
```

### 跳过金币检查进行测试
```csharp
CoinSystemConfig.Instance.ToggleSkipCoinCheck();
```

### 查看当前配置
```csharp
CoinSystemConfig.Instance.LogCoinSystemConfig();
// 或在 Inspector 中点击"打印金币系统配置"
```

### 查看完整流程说明
```csharp
Debug.Log(CoinSystemConfig.Instance.GetCoinSystemSummary());
```

---

## 📊 完整金币流程

```
敌人死亡
  ↓ DropCoin()
掉落 5 金币 ← coinDropPerEnemy
  ↓
玩家拾取
  ↓ PickupItem()
检查: coin >= 10 ? ← coinThresholdForCardSelection
  ├─ 是 → 触发卡牌选择
  └─ 否 → 继续游戏
  ↓
玩家选择卡牌
  ↓ ApplyCard()
检查: coin >= 30 ? ← coinCostPerCard
  ├─ 是 → 消耗30金币，应用卡牌
  └─ 否 → 显示金币不足
  ↓
游戏恢复
```

---

## 🎯 配置建议

### 简单难度
- 触发阈值: 5
- 卡牌消耗: 15
- 敌人掉落: 8

### 普通难度
- 触发阈值: 10
- 卡牌消耗: 30
- 敌人掉落: 5

### 困难难度
- 触发阈值: 15
- 卡牌消耗: 50
- 敌人掉落: 3

---

## ✅ 验证清单

- [x] CoinSystemConfig 创建 ✅
- [x] PlayerControl 集成 ✅
- [x] CardPoolManager 集成 ✅
- [x] EnemyControl 集成 ✅
- [x] 所有代码编译通过 ✅
- [x] 测试脚本创建 ✅
- [x] 文档完成 ✅

---

## 📚 相关文档

- `CoinSystem_Documentation.md` - 完整用户指南
- `CoinSystem_Integration_Report.md` - 项目整合报告
- 本文件 - 快速参考

---

## 🚀 立即开始

1. **在场景中创建 CoinSystemConfig**
   ```
   GameObject → Create Empty → 命名为 "CoinSystem"
   添加脚本 → CoinSystemConfig.cs
   ```

2. **调整 Inspector 中的参数**
   ```
   Coin Threshold For Card Selection: 10
   Coin Cost Per Card: 30
   Coin Drop Per Enemy: 5
   ```

3. **启用调试模式进行测试**
   ```
   Debug Mode: 启用
   运行游戏，观察 Console 中的日志
   ```

4. **使用测试脚本验证**
   ```
   添加 CoinSystemTest.cs
   右键选择测试选项
   ```

---

## 💡 常见问题

**Q: 可以不创建 CoinSystemConfig 吗？**  
A: 可以，但系统会使用默认的硬编码值。建议创建它以获得完整功能。

**Q: 如何在运行时修改配置？**  
A: 在 Inspector 中启用 Debug Mode，然后切换各个选项。

**Q: 调试模式会影响性能吗？**  
A: 有轻微影响（输出日志）。发布前请关闭。

**Q: 如何为不同难度应用不同配置？**  
A: 创建多个 Preset，或编写脚本在启动时切换配置。

---

## 🎉 现在你已经准备好了！

金币系统已完全集成和可控。你可以：
- 🎮 实时调整游戏平衡
- 🧪 快速测试不同配置
- 🔧 轻松调试金币流向
- 📊 理解完整的经济体系

**开始优化你的游戏吧！** 💰✨

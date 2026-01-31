# 金币升级系统使用指南

## 概述

金币升级系统已成功模块化，现在提供独立的、可配置的方法来处理金币检查、卡牌选择UI显示和升级流程。

## 新增方法

### `ProcessCoinUpgrade(int cardCount = -1, int customCoinCost = -1)`

完整的金币升级流程方法，支持自定义参数。

**参数：**
- `cardCount`: 显示的卡牌数量（可选，默认使用`cardsToShow`）
- `customCoinCost`: 自定义金币消耗（可选，默认使用`coinCostPerCard`）

**返回值：** `bool` - 是否成功触发升级

**示例：**
```csharp
// 使用默认设置
CardPoolManager.Instance.ProcessCoinUpgrade();

// 显示3张卡牌，消耗15金币
CardPoolManager.Instance.ProcessCoinUpgrade(3, 15);
```

### `ForceCoinUpgrade(int cardCount = -1)`

调试用方法，强制触发金币升级（跳过金币检查）。

**参数：**
- `cardCount`: 显示的卡牌数量（可选，默认使用`cardsToShow`）

**返回值：** `bool` - 是否成功触发升级

**示例：**
```csharp
// 强制升级，显示默认数量的卡牌
CardPoolManager.Instance.ForceCoinUpgrade();

// 强制升级，显示5张卡牌
CardPoolManager.Instance.ForceCoinUpgrade(5);
```

## 调试设置

在CardPoolManager的Inspector中可以配置以下调试选项：

- **Debug Mode**: 启用详细日志输出
- **Skip Coin Check**: 跳过金币检查（用于测试）

## 测试脚本

已创建`CoinUpgradeTest.cs`测试脚本，提供以下右键菜单选项：

- **测试正常金币升级**: 测试完整的金币升级流程
- **测试强制金币升级**: 测试跳过金币检查的升级流程
- **测试默认参数升级**: 测试使用默认参数的升级
- **显示调试设置**: 显示当前调试设置状态
- **切换调试模式**: 切换调试模式开关
- **切换跳过金币检查**: 切换跳过金币检查开关

## 使用步骤

1. 确保场景中有CardPoolManager、PlayerControl和CardSelectionManager组件
2. 在CardPoolManager中配置卡池和调试设置
3. 调用`ProcessCoinUpgrade()`方法触发升级
4. 玩家选择卡牌后，系统会自动消耗金币并恢复游戏

## 错误处理

系统包含完整的错误处理：
- 检查PlayerControl和CardSelectionManager是否存在
- 验证金币是否足够（可通过调试设置跳过）
- 异常捕获和游戏状态恢复

## 日志输出

启用调试模式后，会输出详细的流程日志：
- 💰 开始金币升级流程
- ✅ 金币升级流程启动成功
- ❌ 各种错误情况

## 向后兼容性

现有代码无需修改，PlayerControl的`TriggerCardSelection()`方法已更新为使用新的系统。
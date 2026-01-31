# 🪙 金币系统集中管理配置

## 问题分析

项目中存在多个位置的**硬编码金币配置**，导致修改金币相关的设定时需要修改多个地方：

### 📍 原始金币配置散落位置：

1. **PlayerControl.cs** (Line 779)
   ```csharp
   if (coin >= 10)  // ❌ 硬编码：10金币触发卡牌选择
   {
       TriggerCardSelection();
   }
   ```

2. **CardPoolManager.cs** (Line 28)
   ```csharp
   public int coinCostPerCard = 30;  // 卡牌消耗30金币
   ```

3. **EnemyControl.cs** (Line 25)
   ```csharp
   public int dropCoin = 5;  // 敌人掉落5金币
   ```

---

## ✅ 解决方案：CoinSystemConfig 统一管理器

### 🎯 新创建的配置文件
**文件位置：** `Assets/Scripts/Manager/CoinSystemConfig.cs`

### 📋 配置的三个核心参数

| 参数 | 默认值 | 说明 | 使用位置 |
|------|------|------|--------|
| `coinThresholdForCardSelection` | 10 | 金币达到此值时触发卡牌选择 | PlayerControl.PickupItem() |
| `coinCostPerCard` | 30 | 应用（选择）一张卡牌消耗的金币 | CardPoolManager, ApplyCard() |
| `coinDropPerEnemy` | 5 | 敌人死亡时掉落的金币数 | EnemyControl.DropCoin() |

---

## 🔧 核心方法

### 1. 检查是否应触发卡牌选择
```csharp
bool ShouldTriggerCardSelection(int currentCoin)
```
**用途：** 检查当前金币是否达到选择卡牌的阈值

**示例：**
```csharp
if (CoinSystemConfig.Instance.ShouldTriggerCardSelection(player.coin))
{
    player.TriggerCardSelection();
}
```

### 2. 检查是否有足够金币
```csharp
bool HasEnoughCoinForCard(int currentCoin)
```
**用途：** 检查是否有足够金币用于卡牌选择（支持调试模式跳过）

**示例：**
```csharp
if (!CoinSystemConfig.Instance.HasEnoughCoinForCard(player.coin))
{
    Debug.LogWarning("金币不足");
    return false;
}
```

### 3. 获取各种金币配置值
```csharp
int GetCoinThresholdForCardSelection()     // 获取触发阈值
int GetCoinCostPerCard()                   // 获取卡牌消耗
int GetCoinDropPerEnemy()                  // 获取敌人掉落
```

---

## 📝 集成位置汇总

### ✏️ PlayerControl.cs - 金币拾取处理

**修改前：**
```csharp
public void PickupItem(string type, int value)
{
    if (type == "Coin")
    {
        coin += value;
        if (coin >= 10)  // ❌ 硬编码
        {
            TriggerCardSelection();
        }
    }
}
```

**修改后：**
```csharp
public void PickupItem(string type, int value)
{
    if (type == "Coin")
    {
        coin += value;
        // ✅ 使用统一的金币配置
        if (CoinSystemConfig.Instance.ShouldTriggerCardSelection(coin))
        {
            TriggerCardSelection();
        }
    }
}
```

### ✏️ CardPoolManager.cs - 卡牌应用和金币检查

**修改位置1 - ApplyCard 方法：**
```csharp
// ✅ 使用配置中的金币消耗值
int requiredCoin = CoinSystemConfig.Instance != null 
    ? CoinSystemConfig.Instance.GetCoinCostPerCard() 
    : coinCostPerCard;

if (!CoinSystemConfig.Instance.HasEnoughCoinForCard(cachedPlayer.coin))
{
    Debug.LogWarning($"金币不足！需要 {requiredCoin}");
    return false;
}
```

**修改位置2 - ProcessCoinUpgrade 方法：**
```csharp
// ✅ 优先使用 CoinSystemConfig 中的配置
int actualCoinCost = customCoinCost;
if (customCoinCost <= 0)
{
    if (CoinSystemConfig.Instance != null)
    {
        actualCoinCost = CoinSystemConfig.Instance.GetCoinCostPerCard();
    }
    else
    {
        actualCoinCost = coinCostPerCard;
    }
}
```

### ✏️ EnemyControl.cs - 敌人掉落金币

**修改前：**
```csharp
public int dropCoin = 5;  // ❌ 硬编码

private void DropCoin()
{
    // 固定掉落5金币
    GameObject coin = Instantiate(coinPrefab, dropPos, Quaternion.identity);
}
```

**修改后：**
```csharp
public int dropCoin = 0;  // 默认为0，使用配置值

private void DropCoin()
{
    // ✅ 优先使用配置，否则使用本地设定
    int coinAmount = dropCoin > 0 
        ? dropCoin 
        : (CoinSystemConfig.Instance != null 
            ? CoinSystemConfig.Instance.GetCoinDropPerEnemy() 
            : 5);
    
    GameObject coin = Instantiate(coinPrefab, dropPos, Quaternion.identity);
}
```

---

## 🎮 使用 Inspector 调整

在 Unity Inspector 中找到 `CoinSystemConfig` 组件，可以直接修改：

1. **Coin Threshold For Card Selection** - 触发卡牌选择的金币数
2. **Coin Cost Per Card** - 单张卡牌的消耗金币
3. **Coin Drop Per Enemy** - 敌人掉落的金币数
4. **Debug Mode** - 启用调试日志
5. **Skip Coin Check** - 跳过金币检查（测试用）

---

## 🧪 调试功能

### Right-Click 菜单命令

在 Inspector 中右键点击 `CoinSystemConfig` 组件可以：

1. **打印金币系统配置** - 显示当前所有配置值
2. **切换调试模式** - 启用/禁用详细日志输出
3. **切换跳过金币检查** - 用于测试不消耗金币的情况

### 调试日志输出

启用调试模式后会输出：
```
╔════════════════════════════════════════════════════════════════════╗
║                     🪙 金币系统配置信息                            ║
╠════════════════════════════════════════════════════════════════════╣
║ 📌 卡牌选择触发阈值: 10
║ 💳 单张卡牌消耗金币: 30
║ 👾 敌人掉落金币数: 5
║ 🔧 调试模式: 启用
║ ⏭️ 跳过金币检查: 否
╚════════════════════════════════════════════════════════════════════╝
```

---

## 📊 金币流程图

```
敌人死亡
   ↓
掉落金币 (coinDropPerEnemy)
   ↓
玩家拾取金币
   ↓
检查: coin >= coinThresholdForCardSelection ?
   ├─ 是 → 触发卡牌选择UI
   └─ 否 → 继续游戏
   ↓
玩家选择卡牌
   ↓
检查: coin >= coinCostPerCard ?
   ├─ 是 → 消耗金币，应用卡牌效果
   └─ 否 → 显示金币不足提示
   ↓
游戏恢复
```

---

## 🔄 向后兼容性

### 旧代码仍然可用

即使没有 `CoinSystemConfig` 实例，代码仍会使用本地的默认值：

```csharp
// CardPoolManager.cs
int requiredCoin = CoinSystemConfig.Instance != null 
    ? CoinSystemConfig.Instance.GetCoinCostPerCard() 
    : coinCostPerCard;  // ← 如果没有配置，用本地值
```

---

## 📌 配置建议

### 对于不同游戏难度

| 难度 | 触发阈值 | 卡牌消耗 | 敌人掉落 |
|------|--------|--------|--------|
| 简单 | 5 | 15 | 8 |
| 普通 | 10 | 30 | 5 |
| 困难 | 15 | 50 | 3 |

---

## ⚠️ 注意事项

1. **必须创建一个 CoinSystemConfig 实例** 在场景中（通常在初始化场景）
2. **配置值应该是正数** - 系统会自动防止负数
3. **调试模式会输出很多日志** - 正式发布前记得关闭
4. **跳过金币检查仅用于测试** - 不要在发布版本中启用

---

## 🎯 总结

通过 `CoinSystemConfig` 统一管理器，我们实现了：

✅ **单一真相来源** - 所有金币配置都在一个地方  
✅ **易于维护** - 修改一个地方，影响整个系统  
✅ **灵活配置** - 可以在 Inspector 中实时调整  
✅ **调试友好** - 支持跳过检查和详细日志  
✅ **向后兼容** - 旧代码不会破裂  

现在你可以轻松地调整整个游戏的金币经济！🎮💰

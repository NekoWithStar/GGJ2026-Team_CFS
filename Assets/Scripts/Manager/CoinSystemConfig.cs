using UnityEngine;

/// <summary>
/// 金币系统配置管理器 - 统一管理所有与金币相关的配置
/// 确保整个项目中关于金币的设定是一致的、可控的
/// 
/// 现有金币相关的设定位置：
/// 1. PlayerControl.cs: 金币拾取时判定 if (coin >= 10) 触发卡牌选择
/// 2. CardPoolManager.cs: coinCostPerCard = 30 卡牌消耗金币数
/// 3. EnemyControl.cs: dropCoin = 5 敌人掉落金币数
/// </summary>
public class CoinSystemConfig : MonoBehaviour
{
    [Header("🎮 金币系统配置")]
    
    [Header("卡牌选择触发设置")]
    [Tooltip("拾取金币达到此值时，触发卡牌选择UI")]
    [SerializeField] private int coinThresholdForCardSelection = 10;
    
    [Header("卡牌消耗设置")]
    [Tooltip("应用（选择）一张卡牌需要消耗的金币数")]
    [SerializeField] private int coinCostPerCard = 30;
    [Tooltip("记录升级次数")]
    [SerializeField] private int countUpgrade = 0;
    [Tooltip("升级难度增加的间隔（每N次升级增加卡牌消耗）")]
    [SerializeField] private int upgradeIntervalForCost = 5;
    [Tooltip("每次升级难度增加的金币消耗数")]
    [SerializeField] private int coinCostIncreasePerUpgrade = 10;
    
    [Header("敌人掉落设置")]
    [Tooltip("敌人死亡时掉落的金币数")]
    [SerializeField] private int coinDropPerEnemy = 5;
    
    [Header("调试设置")]
    [Tooltip("启用金币系统调试模式")]
    [SerializeField] private bool debugMode = false;
    [Tooltip("跳过金币检查（用于测试）")]
    [SerializeField] private bool skipCoinCheck = false;

    // 单例
    public static CoinSystemConfig Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (debugMode)
        {
            LogCoinSystemConfig();
        }
    }

    /// <summary>
    /// 获取卡牌选择触发的金币阈值
    /// </summary>
    public int GetCoinThresholdForCardSelection()
    {
        return coinThresholdForCardSelection;
    }

    /// <summary>
    /// 获取单张卡牌的消耗金币数
    /// </summary>
    public int GetCoinCostPerCard()
    {
        return coinCostPerCard;
    }

    /// <summary>
    /// 获取敌人掉落的金币数
    /// </summary>
    public int GetCoinDropPerEnemy()
    {
        return coinDropPerEnemy;
    }

    /// <summary>
    /// 获取当前升级次数
    /// </summary>
    public int GetUpgradeCount()
    {
        return countUpgrade;
    }

    /// <summary>
    /// 增加升级次数，每升级5次自动增加卡牌消耗
    /// </summary>
    public void IncreaseUpgradeCount()
    {
        countUpgrade++;
        
        if (countUpgrade % upgradeIntervalForCost == 0)
        {
            coinCostPerCard += coinCostIncreasePerUpgrade;
            
            if (debugMode)
            {
                Debug.Log($"[CoinSystemConfig] 🎯 升级难度提升！升级次数: {countUpgrade}，卡牌消耗已增加到: {coinCostPerCard}");
            }
        }
        
        if (debugMode)
        {
            Debug.Log($"[CoinSystemConfig] ⬆️ 升级次数: {countUpgrade}");
        }
    }

    /// <summary>
    /// 检查是否应该触发卡牌选择
    /// </summary>
    public bool ShouldTriggerCardSelection(int currentCoin)
    {
        bool should = currentCoin >= coinThresholdForCardSelection;
        
        if (debugMode && should)
        {
            Debug.Log($"[CoinSystemConfig] ✅ 金币 {currentCoin} >= 阈值 {coinThresholdForCardSelection}，应该触发卡牌选择");
        }
        
        return should;
    }

    /// <summary>
    /// 检查是否有足够的金币用于卡牌选择
    /// </summary>
    public bool HasEnoughCoinForCard(int currentCoin)
    {
        if (skipCoinCheck)
        {
            if (debugMode)
            {
                Debug.LogWarning("[CoinSystemConfig] ⚠️ 调试模式：跳过金币检查");
            }
            return true;
        }

        bool enough = currentCoin >= coinCostPerCard;
        
        if (debugMode)
        {
            Debug.Log($"[CoinSystemConfig] 🔍 检查金币: {currentCoin}/{coinCostPerCard} = {(enough ? "足够" : "不足")}");
        }
        
        return enough;
    }

    /// <summary>
    /// 获取应用卡牌后的剩余金币
    /// </summary>
    public int GetCoinAfterCardApplication(int currentCoin)
    {
        int remaining = currentCoin - coinCostPerCard;
        
        if (debugMode)
        {
            Debug.Log($"[CoinSystemConfig] 💰 消耗卡牌: {currentCoin} - {coinCostPerCard} = {remaining}");
        }
        
        return remaining;
    }

    /// <summary>
    /// 获取敌人掉落金币后的总金币
    /// </summary>
    public int GetCoinAfterEnemyDrop(int currentCoin)
    {
        int total = currentCoin + coinDropPerEnemy;
        
        if (debugMode)
        {
            Debug.Log($"[CoinSystemConfig] 🪙 敌人掉落: {currentCoin} + {coinDropPerEnemy} = {total}");
        }
        
        return total;
    }

    /// <summary>
    /// 打印当前金币系统配置
    /// </summary>
    [ContextMenu("打印金币系统配置")]
    public void LogCoinSystemConfig()
    {
        Debug.Log($@"
╔════════════════════════════════════════════════════════════════════╗
║                     🪙 金币系统配置信息                            ║
╠════════════════════════════════════════════════════════════════════╣
║ 📌 卡牌选择触发阈值: {coinThresholdForCardSelection}
║ 💳 单张卡牌消耗金币: {coinCostPerCard}
║ 👾 敌人掉落金币数: {coinDropPerEnemy}
║ ⬆️ 升级次数: {countUpgrade}
║ 🎯 升级难度间隔: 每 {upgradeIntervalForCost} 次升级，卡牌消耗 +{coinCostIncreasePerUpgrade}
║ 🔧 调试模式: {(debugMode ? "启用" : "禁用")}
║ ⏭️ 跳过金币检查: {(skipCoinCheck ? "是" : "否")}
╚════════════════════════════════════════════════════════════════════╝
");
    }

    /// <summary>
    /// 设置金币系统调试模式
    /// </summary>
    [ContextMenu("切换调试模式")]
    public void ToggleDebugMode()
    {
        debugMode = !debugMode;
        Debug.Log($"[CoinSystemConfig] 调试模式已切换为: {debugMode}");
        LogCoinSystemConfig();
    }

    /// <summary>
    /// 切换跳过金币检查
    /// </summary>
    [ContextMenu("切换跳过金币检查")]
    public void ToggleSkipCoinCheck()
    {
        skipCoinCheck = !skipCoinCheck;
        Debug.Log($"[CoinSystemConfig] 跳过金币检查已切换为: {skipCoinCheck}");
    }

    /// <summary>
    /// 获取金币系统的完整摘要
    /// </summary>
    public string GetCoinSystemSummary()
    {
        return $@"
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  金币系统流程：
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  1️⃣  敌人死亡 → 掉落 {coinDropPerEnemy} 金币
  2️⃣  玩家拾取金币 → 检查是否达到 {coinThresholdForCardSelection} 金币阈值
  3️⃣  金币 >= {coinThresholdForCardSelection} → 触发卡牌选择UI
  4️⃣  玩家选择卡牌 → 检查金币是否 >= {coinCostPerCard}
  5️⃣  金币足够 → 消耗 {coinCostPerCard} 金币，应用卡牌效果，升级次数 +1
  6️⃣  升级次数达到 {upgradeIntervalForCost} 的倍数 → 卡牌消耗自动 +{coinCostIncreasePerUpgrade}（难度提升）
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
";
    }

    /// <summary>
    /// 重置金币系统配置到初始状态（用于场景重置）
    /// </summary>
    public void ResetToInitialState()
    {
        countUpgrade = 0;
        coinCostPerCard = 30; // 重置为初始值
        Debug.Log("[CoinSystemConfig] 金币系统已重置到初始状态");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 防止配置为负数或零
        if (coinThresholdForCardSelection < 0) coinThresholdForCardSelection = 0;
        if (coinCostPerCard < 0) coinCostPerCard = 0;
        if (coinDropPerEnemy < 0) coinDropPerEnemy = 0;
    }
#endif
}

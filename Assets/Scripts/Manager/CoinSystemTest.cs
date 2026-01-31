using UnityEngine;

/// <summary>
/// 金币系统完整测试脚本
/// 验证 CoinSystemConfig 的所有功能是否正常工作
/// </summary>
public class CoinSystemTest : MonoBehaviour
{
    [Header("测试参数")]
    [Tooltip("测试玩家的当前金币数")]
    public int testPlayerCoin = 10;

    /// <summary>
    /// 测试完整的金币流程
    /// </summary>
    [ContextMenu("测试完整金币流程")]
    public void TestCompleteFlow()
    {
        if (CoinSystemConfig.Instance == null)
        {
            Debug.LogError("[CoinSystemTest] ❌ CoinSystemConfig 未在场景中找到，请先创建该配置");
            return;
        }

        Debug.Log("[CoinSystemTest] 🧪 开始测试完整金币流程");
        Debug.Log(CoinSystemConfig.Instance.GetCoinSystemSummary());

        // 1. 打印配置信息
        TestPrintConfig();

        // 2. 测试卡牌选择触发
        TestCardSelectionTrigger();

        // 3. 测试金币检查
        TestCoinCheck();

        // 4. 测试金币消耗
        TestCoinConsumption();

        // 5. 测试敌人掉落
        TestEnemyDrop();

        Debug.Log("[CoinSystemTest] ✅ 测试完成");
    }

    /// <summary>
    /// 测试：打印配置
    /// </summary>
    [ContextMenu("测试：打印配置")]
    public void TestPrintConfig()
    {
        if (CoinSystemConfig.Instance == null)
        {
            Debug.LogError("[CoinSystemTest] ❌ CoinSystemConfig 未找到");
            return;
        }

        Debug.Log("[CoinSystemTest] 📋 打印当前配置");
        CoinSystemConfig.Instance.LogCoinSystemConfig();
    }

    /// <summary>
    /// 测试：卡牌选择触发条件
    /// </summary>
    [ContextMenu("测试：卡牌选择触发")]
    public void TestCardSelectionTrigger()
    {
        if (CoinSystemConfig.Instance == null)
        {
            Debug.LogError("[CoinSystemTest] ❌ CoinSystemConfig 未找到");
            return;
        }

        Debug.Log("[CoinSystemTest] 🎴 测试卡牌选择触发");
        int threshold = CoinSystemConfig.Instance.GetCoinThresholdForCardSelection();

        // 测试低于阈值
        bool shouldTrigger1 = CoinSystemConfig.Instance.ShouldTriggerCardSelection(threshold - 1);
        Debug.Log($"  • 金币 {threshold - 1} 应该触发: {shouldTrigger1} (期望: false)");

        // 测试等于阈值
        bool shouldTrigger2 = CoinSystemConfig.Instance.ShouldTriggerCardSelection(threshold);
        Debug.Log($"  • 金币 {threshold} 应该触发: {shouldTrigger2} (期望: true)");

        // 测试高于阈值
        bool shouldTrigger3 = CoinSystemConfig.Instance.ShouldTriggerCardSelection(threshold + 5);
        Debug.Log($"  • 金币 {threshold + 5} 应该触发: {shouldTrigger3} (期望: true)");

        bool allCorrect = (!shouldTrigger1) && shouldTrigger2 && shouldTrigger3;
        Debug.Log($"  ✓ 卡牌选择触发测试: {(allCorrect ? "✅ 通过" : "❌ 失败")}");
    }

    /// <summary>
    /// 测试：金币检查
    /// </summary>
    [ContextMenu("测试：金币检查")]
    public void TestCoinCheck()
    {
        if (CoinSystemConfig.Instance == null)
        {
            Debug.LogError("[CoinSystemTest] ❌ CoinSystemConfig 未找到");
            return;
        }

        Debug.Log("[CoinSystemTest] 💳 测试金币检查");
        int cost = CoinSystemConfig.Instance.GetCoinCostPerCard();

        // 测试金币不足
        bool enough1 = CoinSystemConfig.Instance.HasEnoughCoinForCard(cost - 1);
        Debug.Log($"  • 金币 {cost - 1} 足够支付 {cost}: {enough1} (期望: false)");

        // 测试金币等于消耗
        bool enough2 = CoinSystemConfig.Instance.HasEnoughCoinForCard(cost);
        Debug.Log($"  • 金币 {cost} 足够支付 {cost}: {enough2} (期望: true)");

        // 测试金币充足
        bool enough3 = CoinSystemConfig.Instance.HasEnoughCoinForCard(cost + 100);
        Debug.Log($"  • 金币 {cost + 100} 足够支付 {cost}: {enough3} (期望: true)");

        bool allCorrect = (!enough1) && enough2 && enough3;
        Debug.Log($"  ✓ 金币检查测试: {(allCorrect ? "✅ 通过" : "❌ 失败")}");
    }

    /// <summary>
    /// 测试：金币消耗计算
    /// </summary>
    [ContextMenu("测试：金币消耗")]
    public void TestCoinConsumption()
    {
        if (CoinSystemConfig.Instance == null)
        {
            Debug.LogError("[CoinSystemTest] ❌ CoinSystemConfig 未找到");
            return;
        }

        Debug.Log("[CoinSystemTest] 💰 测试金币消耗");
        int cost = CoinSystemConfig.Instance.GetCoinCostPerCard();
        int startCoin = 100;

        int afterConsume = CoinSystemConfig.Instance.GetCoinAfterCardApplication(startCoin);
        int expectedCoin = startCoin - cost;

        Debug.Log($"  • 初始金币: {startCoin}");
        Debug.Log($"  • 消耗金币: {cost}");
        Debug.Log($"  • 剩余金币: {afterConsume} (期望: {expectedCoin})");

        bool correct = afterConsume == expectedCoin;
        Debug.Log($"  ✓ 金币消耗测试: {(correct ? "✅ 通过" : "❌ 失败")}");
    }

    /// <summary>
    /// 测试：敌人掉落金币
    /// </summary>
    [ContextMenu("测试：敌人掉落")]
    public void TestEnemyDrop()
    {
        if (CoinSystemConfig.Instance == null)
        {
            Debug.LogError("[CoinSystemTest] ❌ CoinSystemConfig 未找到");
            return;
        }

        Debug.Log("[CoinSystemTest] 👾 测试敌人掉落");
        int dropAmount = CoinSystemConfig.Instance.GetCoinDropPerEnemy();
        int startCoin = 0;

        int afterDrop = CoinSystemConfig.Instance.GetCoinAfterEnemyDrop(startCoin);
        int expectedCoin = startCoin + dropAmount;

        Debug.Log($"  • 初始金币: {startCoin}");
        Debug.Log($"  • 敌人掉落: {dropAmount}");
        Debug.Log($"  • 总金币: {afterDrop} (期望: {expectedCoin})");

        bool correct = afterDrop == expectedCoin;
        Debug.Log($"  ✓ 敌人掉落测试: {(correct ? "✅ 通过" : "❌ 失败")}");
    }

    /// <summary>
    /// 测试：调试模式切换
    /// </summary>
    [ContextMenu("测试：切换调试模式")]
    public void TestDebugModeToggle()
    {
        if (CoinSystemConfig.Instance == null)
        {
            Debug.LogError("[CoinSystemTest] ❌ CoinSystemConfig 未找到");
            return;
        }

        Debug.Log("[CoinSystemTest] 🔧 测试调试模式切换");
        CoinSystemConfig.Instance.ToggleDebugMode();
        Debug.Log("  ✓ 调试模式已切换");
    }

    /// <summary>
    /// 测试：跳过金币检查切换
    /// </summary>
    [ContextMenu("测试：切换跳过金币检查")]
    public void TestSkipCoinCheckToggle()
    {
        if (CoinSystemConfig.Instance == null)
        {
            Debug.LogError("[CoinSystemTest] ❌ CoinSystemConfig 未找到");
            return;
        }

        Debug.Log("[CoinSystemTest] ⏭️ 测试跳过金币检查切换");
        CoinSystemConfig.Instance.ToggleSkipCoinCheck();
        Debug.Log("  ✓ 跳过金币检查已切换");
    }

    /// <summary>
    /// 模拟玩家拾取金币并检查是否应该触发卡牌选择
    /// </summary>
    [ContextMenu("模拟：玩家拾取金币")]
    public void SimulatePickupCoin()
    {
        if (CoinSystemConfig.Instance == null)
        {
            Debug.LogError("[CoinSystemTest] ❌ CoinSystemConfig 未找到");
            return;
        }

        Debug.Log($"[CoinSystemTest] 📦 模拟玩家拾取金币");
        int threshold = CoinSystemConfig.Instance.GetCoinThresholdForCardSelection();
        
        for (int i = 0; i <= threshold + 2; i++)
        {
            bool shouldTrigger = CoinSystemConfig.Instance.ShouldTriggerCardSelection(i);
            string status = shouldTrigger ? "✅ 触发卡牌选择" : "❌ 不触发";
            Debug.Log($"  • 金币: {i:D2} → {status}");
        }
    }

    /// <summary>
    /// 模拟完整的卡牌选择流程
    /// </summary>
    [ContextMenu("模拟：完整卡牌选择流程")]
    public void SimulateCardSelectionFlow()
    {
        if (CoinSystemConfig.Instance == null)
        {
            Debug.LogError("[CoinSystemTest] ❌ CoinSystemConfig 未找到");
            return;
        }

        Debug.Log("[CoinSystemTest] 🎮 模拟完整卡牌选择流程");
        
        int currentCoin = testPlayerCoin;
        int threshold = CoinSystemConfig.Instance.GetCoinThresholdForCardSelection();
        int cost = CoinSystemConfig.Instance.GetCoinCostPerCard();

        Debug.Log($"\n初始状态:");
        Debug.Log($"  当前金币: {currentCoin}");
        Debug.Log($"  触发阈值: {threshold}");
        Debug.Log($"  卡牌消耗: {cost}");

        Debug.Log($"\n步骤1: 检查是否触发卡牌选择");
        bool shouldTrigger = CoinSystemConfig.Instance.ShouldTriggerCardSelection(currentCoin);
        Debug.Log($"  结果: {(shouldTrigger ? "✅ 应该触发" : "❌ 不应该触发")}");

        if (!shouldTrigger)
        {
            Debug.Log($"\n需要更多金币。最少需要 {threshold} 金币");
            return;
        }

        Debug.Log($"\n步骤2: 检查是否有足够金币用于卡牌");
        bool hasEnough = CoinSystemConfig.Instance.HasEnoughCoinForCard(currentCoin);
        Debug.Log($"  结果: {(hasEnough ? "✅ 金币足够" : "❌ 金币不足")}");

        if (!hasEnough)
        {
            Debug.Log($"\n需要更多金币。需要 {cost} 金币，当前拥有 {currentCoin}");
            return;
        }

        Debug.Log($"\n步骤3: 玩家选择卡牌");
        Debug.Log($"  显示卡牌选择界面...");

        Debug.Log($"\n步骤4: 消耗金币");
        int remainingCoin = CoinSystemConfig.Instance.GetCoinAfterCardApplication(currentCoin);
        Debug.Log($"  消耗前: {currentCoin}");
        Debug.Log($"  消耗: {cost}");
        Debug.Log($"  消耗后: {remainingCoin}");

        Debug.Log($"\n✅ 流程完成！");
    }
}

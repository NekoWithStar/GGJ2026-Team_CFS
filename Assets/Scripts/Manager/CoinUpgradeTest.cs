using UnityEngine;

/// <summary>
/// 金币升级系统测试脚本
/// 用于验证ProcessCoinUpgrade和ForceCoinUpgrade方法的正确性
/// </summary>
public class CoinUpgradeTest : MonoBehaviour
{
    [Header("测试设置")]
    [Tooltip("测试时显示的卡牌数量")]
    public int testCardCount = 3;
    [Tooltip("测试时使用的自定义金币消耗")]
    public int testCoinCost = 5;

    /// <summary>
    /// 测试正常的金币升级流程
    /// </summary>
    [ContextMenu("测试正常金币升级")]
    public void TestNormalCoinUpgrade()
    {
        if (CardPoolManager.Instance == null)
        {
            Debug.LogError("[CoinUpgradeTest] ❌ CardPoolManager未找到");
            return;
        }

        Debug.Log("[CoinUpgradeTest] 🧪 开始测试正常金币升级流程");
        bool success = CardPoolManager.Instance.ProcessCoinUpgrade(testCardCount, testCoinCost);
        Debug.Log($"[CoinUpgradeTest] 测试结果: {(success ? "成功" : "失败")}");
    }

    /// <summary>
    /// 测试强制金币升级（跳过金币检查）
    /// </summary>
    [ContextMenu("测试强制金币升级")]
    public void TestForceCoinUpgrade()
    {
        if (CardPoolManager.Instance == null)
        {
            Debug.LogError("[CoinUpgradeTest] ❌ CardPoolManager未找到");
            return;
        }

        Debug.Log("[CoinUpgradeTest] 🧪 开始测试强制金币升级流程");
        bool success = CardPoolManager.Instance.ForceCoinUpgrade(testCardCount);
        Debug.Log($"[CoinUpgradeTest] 测试结果: {(success ? "成功" : "失败")}");
    }

    /// <summary>
    /// 测试使用默认参数的金币升级
    /// </summary>
    [ContextMenu("测试默认参数升级")]
    public void TestDefaultCoinUpgrade()
    {
        if (CardPoolManager.Instance == null)
        {
            Debug.LogError("[CoinUpgradeTest] ❌ CardPoolManager未找到");
            return;
        }

        Debug.Log("[CoinUpgradeTest] 🧪 开始测试默认参数金币升级流程");
        bool success = CardPoolManager.Instance.ProcessCoinUpgrade();
        Debug.Log($"[CoinUpgradeTest] 测试结果: {(success ? "成功" : "失败")}");
    }

    /// <summary>
    /// 显示当前调试设置状态
    /// </summary>
    [ContextMenu("显示调试设置")]
    public void ShowDebugSettings()
    {
        if (CardPoolManager.Instance == null)
        {
            Debug.LogError("[CoinUpgradeTest] ❌ CardPoolManager未找到");
            return;
        }

        Debug.Log($"[CoinUpgradeTest] 📊 当前调试设置:");
        Debug.Log($"  - 调试模式: {CardPoolManager.Instance.debugMode}");
        Debug.Log($"  - 跳过金币检查: {CardPoolManager.Instance.skipCoinCheck}");
        Debug.Log($"  - 默认卡牌数量: {CardPoolManager.Instance.cardsToShow}");
        Debug.Log($"  - 默认金币消耗: {CardPoolManager.Instance.coinCostPerCard}");
    }

    /// <summary>
    /// 切换调试模式
    /// </summary>
    [ContextMenu("切换调试模式")]
    public void ToggleDebugMode()
    {
        if (CardPoolManager.Instance == null)
        {
            Debug.LogError("[CoinUpgradeTest] ❌ CardPoolManager未找到");
            return;
        }

        CardPoolManager.Instance.debugMode = !CardPoolManager.Instance.debugMode;
        Debug.Log($"[CoinUpgradeTest] 🔄 调试模式已切换为: {CardPoolManager.Instance.debugMode}");
    }

    /// <summary>
    /// 切换跳过金币检查
    /// </summary>
    [ContextMenu("切换跳过金币检查")]
    public void ToggleSkipCoinCheck()
    {
        if (CardPoolManager.Instance == null)
        {
            Debug.LogError("[CoinUpgradeTest] ❌ CardPoolManager未找到");
            return;
        }

        CardPoolManager.Instance.skipCoinCheck = !CardPoolManager.Instance.skipCoinCheck;
        Debug.Log($"[CoinUpgradeTest] 🔄 跳过金币检查已切换为: {CardPoolManager.Instance.skipCoinCheck}");
    }
}
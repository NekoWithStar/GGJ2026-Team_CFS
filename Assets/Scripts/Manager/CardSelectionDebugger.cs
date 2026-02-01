using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 卡牌选择系统调试工具 - 用于诊断第二次重载时的卡牌选择窗口显示问题
/// </summary>
public class CardSelectionDebugger : MonoBehaviour
{
    private PlayerControl playerControl;
    private CardPoolManager cardPoolManager;
    private CardSelectionManager cardSelectionManager;

    private void Start()
    {
        playerControl = FindAnyObjectByType<PlayerControl>();
        cardPoolManager = FindAnyObjectByType<CardPoolManager>();
        cardSelectionManager = FindAnyObjectByType<CardSelectionManager>();

        Debug.Log("[CardSelectionDebugger] ========== 卡牌选择系统状态检查 ==========");
        Debug.Log($"[CardSelectionDebugger] PlayerControl: {(playerControl != null ? "✅ 找到" : "❌ 未找到")}");
        Debug.Log($"[CardSelectionDebugger] CardPoolManager: {(cardPoolManager != null ? "✅ 找到" : "❌ 未找到")}");
        Debug.Log($"[CardSelectionDebugger] CardSelectionManager: {(cardSelectionManager != null ? "✅ 找到" : "❌ 未找到")}");
        
        if (cardSelectionManager != null)
        {
            var panelField = cardSelectionManager.GetType().GetField("cardSelectionPanel", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var containerField = cardSelectionManager.GetType().GetField("cardContainer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var prefabField = cardSelectionManager.GetType().GetField("cardPrefab",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (panelField != null)
            {
                var panel = panelField.GetValue(cardSelectionManager) as GameObject;
                Debug.Log($"[CardSelectionDebugger] cardSelectionPanel: {(panel != null ? $"✅ {panel.name}" : "❌ null")}");
                if (panel != null)
                {
                    Debug.Log($"[CardSelectionDebugger]   - 激活状态: {panel.activeSelf}");
                    Debug.Log($"[CardSelectionDebugger]   - 根对象激活: {panel.activeInHierarchy}");
                }
            }
            
            if (containerField != null)
            {
                var container = containerField.GetValue(cardSelectionManager) as Transform;
                Debug.Log($"[CardSelectionDebugger] cardContainer: {(container != null ? $"✅ {container.name}" : "❌ null")}");
            }
            
            if (prefabField != null)
            {
                var prefab = prefabField.GetValue(cardSelectionManager) as GameObject;
                Debug.Log($"[CardSelectionDebugger] cardPrefab: {(prefab != null ? $"✅ {prefab.name}" : "❌ null")}");
            }
        }

        Debug.Log("[CardSelectionDebugger] ========================================");
    }

    private void Update()
    {
        // 按 T 键手动测试卡牌选择
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[CardSelectionDebugger] 📢 按下 T 键，手动测试 ProcessCoinUpgrade");
            if (cardPoolManager != null && playerControl != null)
            {
                // 给玩家一些金币用于测试
                playerControl.coin = 100;
                cardPoolManager.ProcessCoinUpgrade(3, 30);
            }
        }

        // 按 E 键隐藏卡牌选择窗口
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("[CardSelectionDebugger] 📢 按下 E 键，隐藏卡牌选择窗口并恢复游戏");
            if (cardSelectionManager != null)
            {
                cardSelectionManager.HideCardSelection();
                if (playerControl != null)
                {
                    playerControl.ResumeGame();
                }
            }
        }

        // 按 S 键打印当前状态
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("[CardSelectionDebugger] 📢 打印系统状态");
            if (playerControl != null)
            {
                Debug.Log($"  - PlayerControl.coin: {playerControl.coin}");
                Debug.Log($"  - Time.timeScale: {Time.timeScale}");
            }
            if (CardPoolManager.Instance != null)
            {
                Debug.Log($"  - CardPoolManager.debugMode: {CardPoolManager.Instance.debugMode}");
            }
        }
    }
}

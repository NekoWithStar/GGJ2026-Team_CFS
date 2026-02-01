using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Y_Survivor;

/// <summary>
/// 卡池管理器 - 统一管理武器卡和属性卡，集中处理卡牌应用、金币消耗、游戏恢复和UI更新
/// 支持从中随机选择指定数量的卡片供玩家选择
/// </summary>
public class CardPoolManager : MonoBehaviour
{
    [Header("卡池设置")]
    [Tooltip("所有可用的武器卡")]
    public List<Weapon> weaponCards = new List<Weapon>();
    
    [Tooltip("所有可用的属性卡")]
    public List<PropertyCard> propertyCards = new List<PropertyCard>();

    [Header("选择规则")]
    [Tooltip("每次升级显示的卡牌数量")]
    public int cardsToShow = 4;
    
    [Tooltip("玩家选择的卡牌数量")]
    public int cardsToSelect = 1;

    [Header("消耗设置")]
    [Tooltip("应用卡牌时消耗的金币数量")]
    public int coinCostPerCard = 30;

    [Header("调试设置")]
    [Tooltip("启用调试模式（显示详细日志）")]
    public bool debugMode = false;
    [Tooltip("跳过金币检查（用于测试）")]
    public bool skipCoinCheck = false;

    // UI 映射已回退到 PlayerControl（由 PlayerControl 负责更新 HUD）

    // 单例
    public static CardPoolManager Instance { get; private set; }
    private PlayerControl cachedPlayer;
    private CardSelectionManager cachedCardSelectionManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        cachedPlayer = FindAnyObjectByType<PlayerControl>();
        cachedCardSelectionManager = FindAnyObjectByType<CardSelectionManager>();
        Debug.Log($"[CardPoolManager] ✅ Awake - PlayerControl={cachedPlayer != null}, CardSelectionManager={cachedCardSelectionManager != null}");
    }

    /// <summary>
    /// 从卡池中随机选择指定数量的卡牌
    /// </summary>
    /// <param name="count">要选择的卡牌数量</param>
    /// <returns>随机选中的卡牌列表（可能包含武器卡和属性卡的混合）</returns>
    public List<ScriptableObject> GetRandomCards(int count = -1)
    {
        if (count <= 0) count = cardsToShow;

        // 合并所有卡牌到一个统一列表
        List<ScriptableObject> allCards = new List<ScriptableObject>();
        allCards.AddRange(weaponCards);
        allCards.AddRange(propertyCards);

        if (allCards.Count == 0)
        {
            Debug.LogError("[CardPoolManager] ❌ 卡池为空！请在 Inspector 中配置卡牌");
            return new List<ScriptableObject>();
        }

        // 随机打乱并取出前 count 张
        List<ScriptableObject> selectedCards = new List<ScriptableObject>();
        List<ScriptableObject> tempCards = new List<ScriptableObject>(allCards);

        for (int i = 0; i < count && tempCards.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, tempCards.Count);
            selectedCards.Add(tempCards[randomIndex]);
            tempCards.RemoveAt(randomIndex);
        }

        Debug.Log($"[CardPoolManager] 📋 从卡池中随机选择了 {selectedCards.Count} 张卡牌");
        return selectedCards;
    }

    /// <summary>
    /// 获取随机武器卡
    /// </summary>
    public Weapon GetRandomWeaponCard()
    {
        if (weaponCards.Count == 0)
        {
            Debug.LogError("[CardPoolManager] ❌ 武器卡池为空！");
            return null;
        }
        return weaponCards[Random.Range(0, weaponCards.Count)];
    }

    /// <summary>
    /// 获取随机属性卡
    /// </summary>
    public PropertyCard GetRandomPropertyCard()
    {
        if (propertyCards.Count == 0)
        {
            Debug.LogError("[CardPoolManager] ❌ 属性卡池为空！");
            return null;
        }
        return propertyCards[Random.Range(0, propertyCards.Count)];
    }

    /// <summary>
    /// 添加新的武器卡到卡池
    /// </summary>
    public void AddWeaponCard(Weapon weapon)
    {
        if (weapon != null && !weaponCards.Contains(weapon))
        {
            weaponCards.Add(weapon);
        }
    }

    /// <summary>
    /// 添加新的属性卡到卡池
    /// </summary>
    public void AddPropertyCard(PropertyCard card)
    {
        if (card != null && !propertyCards.Contains(card))
        {
            propertyCards.Add(card);
        }
    }

    /// <summary>
    /// 应用卡牌选择 - 集中处理卡牌应用、金币消耗、游戏恢复和UI更新
    /// 统一入口：被 CardSelectionManager 或其他模块调用
    /// </summary>
    /// <param name="card">要应用的卡牌（PropertyCard 或 Weapon）</param>
    /// <returns>是否应用成功</returns>
    public bool ApplyCard(ScriptableObject card)
    {
        Debug.Log($"[CardPoolManager] ▶ ApplyCard called for: {(card!=null?card.GetType().Name:"null")} ");

        if (card == null)
        {
            Debug.LogError("[CardPoolManager] ❌ 卡牌为空，无法应用");
            return false;
        }

        if (cachedPlayer == null)
            cachedPlayer = FindAnyObjectByType<PlayerControl>();

        if (cachedPlayer == null)
        {
            Debug.LogError("[CardPoolManager] ❌ PlayerControl未找到，无法应用卡牌");
            return false;
        }

        // 检查金币是否足够（使用统一的金币配置）
        int requiredCoin = CoinSystemConfig.Instance != null ? CoinSystemConfig.Instance.GetCoinCostPerCard() : coinCostPerCard;
        Debug.Log($"[CardPoolManager] 🔎 Player coin before apply: {cachedPlayer.coin} (need {requiredCoin})");
        if (CoinSystemConfig.Instance != null ? !CoinSystemConfig.Instance.HasEnoughCoinForCard(cachedPlayer.coin) : cachedPlayer.coin < coinCostPerCard)
        {
            Debug.LogWarning($"[CardPoolManager] ⚠️ 金币不足！需要 {requiredCoin}，当前拥有 {cachedPlayer.coin}");
            return false;
        }

        // 根据卡牌类型应用效果
        bool applySuccess = false;

        if (card is Weapon weapon)
        {
            cachedPlayer.ApplyWeaponCard(weapon);
            Debug.Log($"[CardPoolManager] ✅ 应用武器卡: {weapon.weaponName}");
            applySuccess = true;
        }
        else if (card is PropertyCard propertyCard)
        {
            cachedPlayer.ApplyPropertyCard(propertyCard);
            Debug.Log($"[CardPoolManager] ✅ 应用属性卡: {propertyCard.cardName}");
            applySuccess = true;
        }
        else
        {
            Debug.LogWarning($"[CardPoolManager] ⚠️ 未知卡牌类型: {card.GetType().Name}");
            return false;
        }

        // 如果应用成功，消耗金币（使用统一的金币配置）
        if (applySuccess)
        {
            int costAmount = CoinSystemConfig.Instance != null ? CoinSystemConfig.Instance.GetCoinCostPerCard() : coinCostPerCard;
            bool consumed = ConsumeCoin(costAmount);
            Debug.Log($"[CardPoolManager] 🔁 applySuccess={applySuccess} consumed={consumed}");
            
            // 消耗金币成功后，增加升级计数
            if (consumed && CoinSystemConfig.Instance != null)
            {
                CoinSystemConfig.Instance.IncreaseUpgradeCount();
                Debug.Log($"[CardPoolManager] ⬆️ 升级计数已增加");
            }
            
            ResumeGameplay();
            // HUD 更新由 PlayerControl 负责
            if (cachedPlayer != null)
            {
                cachedPlayer.UpdateHUD();
                Debug.Log($"[CardPoolManager] 🔔 Called cachedPlayer.UpdateHUD() - coin now {cachedPlayer.coin}");
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// 消耗指定数量的金币
    /// </summary>
    /// <param name="amount">要消耗的金币数量</param>
    /// <returns>是否消耗成功</returns>
    private bool ConsumeCoin(int amount)
    {
        if (cachedPlayer == null)
            cachedPlayer = FindAnyObjectByType<PlayerControl>();

        if (cachedPlayer == null)
        {
            Debug.LogError("[CardPoolManager] ❌ PlayerControl未找到，无法消耗金币");
            return false;
        }

        // 调用PlayerControl的ConsumeCoin方法来处理消耗和统计
        return cachedPlayer.ConsumeCoin(amount);
    }

    /// <summary>
    /// 恢复游戏（取消暂停）
    /// </summary>
    private void ResumeGameplay()
    {
        if (cachedPlayer == null)
            cachedPlayer = FindAnyObjectByType<PlayerControl>();

        if (cachedPlayer != null)
        {
            cachedPlayer.ResumeGame();
            Debug.Log("[CardPoolManager] ▶️ 游戏已恢复");
        }
        else
        {
            Debug.LogError("[CardPoolManager] ❌ 无法恢复游戏，PlayerControl未找到");
        }
    }

    /// <summary>
    /// 金币升级系统 - 完整的升级流程（可供调试和配置）
    /// 检查金币、显示卡牌选择UI、处理选择结果、消耗金币、恢复游戏
    /// </summary>
    /// <param name="cardCount">显示的卡牌数量（可选，默认使用cardsToShow）</param>
    /// <param name="customCoinCost">自定义金币消耗（可选，默认使用coinCostPerCard）</param>
    /// <returns>是否成功触发升级</returns>
    public bool ProcessCoinUpgrade(int cardCount = -1, int customCoinCost = -1)
    {
        Debug.Log($"[CardPoolManager] 💰 ProcessCoinUpgrade被调用 - cardCount={cardCount}, customCoinCost={customCoinCost}");
        
        // 使用默认值或自定义值
        int actualCardCount = cardCount > 0 ? cardCount : cardsToShow;
        
        // 优先使用 CoinSystemConfig 中的配置
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

        // 确保PlayerControl可用
        if (cachedPlayer == null)
        {
            cachedPlayer = FindAnyObjectByType<PlayerControl>();
            if (cachedPlayer == null)
            {
                Debug.LogError("[CardPoolManager] ❌ ProcessCoinUpgrade失败：PlayerControl未找到");
                return false;
            }
            Debug.Log("[CardPoolManager] ✅ PlayerControl已重新缓存");
        }

        // 检查金币是否足够（使用统一的金币配置）
        if (CoinSystemConfig.Instance != null)
        {
            if (!CoinSystemConfig.Instance.HasEnoughCoinForCard(cachedPlayer.coin))
            {
                Debug.LogWarning($"[CardPoolManager] ⚠️ 金币不足！需要 {actualCoinCost}，当前拥有 {cachedPlayer.coin}");
                return false;
            }
        }
        else if (cachedPlayer.coin < actualCoinCost)
        {
            Debug.LogWarning($"[CardPoolManager] ⚠️ 金币不足！需要 {actualCoinCost}，当前拥有 {cachedPlayer.coin}");
            return false;
        }

        if (debugMode)
        {
            Debug.Log($"[CardPoolManager] 💰 开始金币升级流程 - 金币: {cachedPlayer.coin}/{actualCoinCost}, 卡牌数量: {actualCardCount}");
        }

        // 获取或重新查找CardSelectionManager
        if (cachedCardSelectionManager == null)
        {
            cachedCardSelectionManager = FindAnyObjectByType<CardSelectionManager>();
            if (cachedCardSelectionManager == null)
            {
                Debug.LogError("[CardPoolManager] ❌ ProcessCoinUpgrade失败：CardSelectionManager未找到");
                return false;
            }
            Debug.Log("[CardPoolManager] ✅ CardSelectionManager已重新缓存");
        }

        // 触发卡牌选择
        try
        {
            // 暂停游戏（通过PlayerControl）
            cachedPlayer.PauseGameForCardSelection();
            Debug.Log("[CardPoolManager] ✅ 游戏已暂停");

            // 显示卡牌选择UI
            Debug.Log($"[CardPoolManager] 📢 调用ShowCardSelection({actualCardCount})");
            bool uiShown = cachedCardSelectionManager.ShowCardSelection(actualCardCount);
            if (!uiShown)
            {
                Debug.LogError("[CardPoolManager] ❌ 卡牌选择UI未能显示，恢复游戏");
                ResumeGameplay();
                return false;
            }
            Debug.Log("[CardPoolManager] ✅ 卡牌选择UI已显示");

            if (debugMode)
            {
                Debug.Log($"[CardPoolManager] ✅ 金币升级流程启动成功 - 等待玩家选择卡牌");
            }
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CardPoolManager] ❌ ProcessCoinUpgrade异常: {e.Message}\n{e.StackTrace}");
            // 如果出现异常，尝试恢复游戏
            ResumeGameplay();
            return false;
        }
    }

    /// <summary>
    /// 调试用方法 - 强制触发金币升级（不检查金币）
    /// </summary>
    public bool ForceCoinUpgrade(int cardCount = -1)
    {
        if (debugMode)
        {
            Debug.LogWarning("[CardPoolManager] 🔧 调试模式：强制触发金币升级（跳过金币检查）");
        }

        int actualCardCount = cardCount > 0 ? cardCount : cardsToShow;

        if (cachedPlayer == null)
        {
            cachedPlayer = FindAnyObjectByType<PlayerControl>();
        }

        var cardSelectionManager = FindAnyObjectByType<CardSelectionManager>();
        if (cardSelectionManager == null)
        {
            Debug.LogError("[CardPoolManager] ❌ ForceCoinUpgrade失败：CardSelectionManager未找到");
            return false;
        }

        try
        {
            cachedPlayer.PauseGameForCardSelection();
            bool uiShown = cardSelectionManager.ShowCardSelection(actualCardCount);
            if (!uiShown)
            {
                Debug.LogError("[CardPoolManager] ❌ 卡牌选择UI未能显示，恢复游戏");
                ResumeGameplay();
                return false;
            }
            if (debugMode)
            {
                Debug.Log($"[CardPoolManager] ✅ 强制金币升级启动成功");
            }
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CardPoolManager] ❌ ForceCoinUpgrade异常: {e.Message}");
            ResumeGameplay();
            return false;
        }
    }
}

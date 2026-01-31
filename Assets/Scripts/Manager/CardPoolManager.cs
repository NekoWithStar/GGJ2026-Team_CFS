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
    public int coinCostPerCard = 10;

    // UI 映射已回退到 PlayerControl（由 PlayerControl 负责更新 HUD）

    // 单例
    public static CardPoolManager Instance { get; private set; }
    private PlayerControl cachedPlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        cachedPlayer = FindAnyObjectByType<PlayerControl>();
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

        // 检查金币是否足够
        if (cachedPlayer.coin < coinCostPerCard)
        {
            Debug.LogWarning($"[CardPoolManager] ⚠️ 金币不足！需要 {coinCostPerCard}，当前拥有 {cachedPlayer.coin}");
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

        // 如果应用成功，消耗金币
        if (applySuccess)
        {
            ConsumeCoin(coinCostPerCard);
            ResumeGameplay();
            // HUD 更新由 PlayerControl 负责
            if (cachedPlayer != null)
                cachedPlayer.UpdateHUD();
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

        if (cachedPlayer.coin < amount)
        {
            Debug.LogWarning($"[CardPoolManager] ⚠️ 金币不足！需要 {amount}，当前拥有 {cachedPlayer.coin}");
            return false;
        }

        cachedPlayer.coin -= amount;
        if (cachedPlayer.coin < 0) cachedPlayer.coin = 0;

        Debug.Log($"[CardPoolManager] 💰 消耗 {amount} 金币，剩余: {cachedPlayer.coin}");
        return true;
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
}

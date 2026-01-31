using System.Collections.Generic;
using UnityEngine;
using Y_Survivor;

/// <summary>
/// 卡池管理器 - 统一管理武器卡和属性卡
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

    // 单例
    public static CardPoolManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
}

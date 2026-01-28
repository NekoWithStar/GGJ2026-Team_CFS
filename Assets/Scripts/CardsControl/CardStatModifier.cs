using System.Collections.Generic;
using UnityEngine;

// 原子化卡牌属性修改器（最小侵入，集中修改 ScriptableObject Card）
public static class CardStatModifier
{
    // 覆盖 rank（保证非负）
    public static void SetRank(Card card, int rank)
    {
        if (card == null) return;
        card.cardRank = Mathf.Max(0, rank);
    }

    // 批量设置 rank
    public static void SetRankForList(IEnumerable<Card> cards, int rank)
    {
        if (cards == null) return;
        foreach (var c in cards)
        {
            SetRank(c, rank);
        }
    }

    // 增减 rank（可为负 delta）
    public static void AddRank(Card card, int delta)
    {
        if (card == null) return;
        card.cardRank = Mathf.Max(0, card.cardRank + delta);
    }

    // 覆盖 cost（保证非负）
    public static void SetCost(Card card, int cost)
    {
        if (card == null) return;
        card.cardCost = Mathf.Max(0, cost);
    }

    // 批量设置 cost
    public static void SetCostForList(IEnumerable<Card> cards, int cost)
    {
        if (cards == null) return;
        foreach (var c in cards)
        {
            SetCost(c, cost);
        }
    }

    // 增减 cost（可为负 delta，但最终不小于0）
    public static void AddCost(Card card, int delta)
    {
        if (card == null) return;
        card.cardCost = Mathf.Max(0, card.cardCost + delta);
    }
}
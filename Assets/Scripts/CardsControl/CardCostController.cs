using System.Collections.Generic;
using UnityEngine;

// 管理可销耗的资源池并按 Card.cardCost 循环消耗卡牌，增加 cardRank，并同步 Face_Move UI。
// 最小侵入：不会修改 Card 类，严格使用 card.cardCost / card.cardRank。
public class CardCostController : MonoBehaviour
{
    [Header("外部可销耗的资源池（若不需要可置为大值或忽略）")]
    public int availableCost = 10;

    [Header("默认每次消费增加的 rank（可在调用时覆盖）")]
    public int defaultRankIncrease = 1;

    [Header("场景中的 Face_Move（用于同步滚动条）")]
    public Face_Move faceMove; // 在 Inspector 指向场景里的 Face_Move 实例（可为空，但若为空不会同步 UI）

    // 尝试消费单张卡（严格使用 card.cardCost）
    // rankIncrease 如果传 -1 则使用 defaultRankIncrease
    // 返回 true 表示成功消费并已增加 rank；false 表示失败
    public bool TryConsumeCard(Card card, int rankIncrease = -1)
    {
        if (card == null)
        {
            Debug.LogWarning("TryConsumeCard: card 为 null");
            return false;
        }

        int cost = Mathf.Max(0, card.cardCost);

        if (cost > availableCost)
        {
            Debug.Log("无法使用：可用 cost 不足");
            return false;
        }

        // 使用卡牌自身的 cost 扣费
        availableCost -= cost;

        int inc = (rankIncrease < 0) ? defaultRankIncrease : rankIncrease;
        if (inc != 0)
        {
            CardStatModifier.AddRank(card, inc);
        }

        // 同步 UI（若有绑定）
        SyncFaceMoveForCard(card);

        return true;
    }

    // 批量循环消费：按传入顺序尝试对每张卡执行消费（使用卡牌自身 cardCost）
    // 返回被成功消费的卡牌列表（为空表示无成功）
    public List<Card> ConsumeCardsLoop(IEnumerable<Card> cards, int rankIncreasePerCard = -1)
    {
        var consumed = new List<Card>();
        if (cards == null) return consumed;

        foreach (var c in cards)
        {
            if (c == null) continue;

            // 尝试消费当前卡，若成功则加入 consumed 并继续；若失败（cost不足）就跳过该卡继续循环
            if (TryConsumeCard(c, rankIncreasePerCard))
            {
                consumed.Add(c);
            }
        }

        return consumed;
    }

    // 将卡牌的当前 rank 值转换为 0..1 并设置到 Face_Move 的对应 scrollbar（会触发 Face_Move 的回调）
    void SyncFaceMoveForCard(Card card)
    {
        if (card == null) return;

        if (faceMove == null)
        {
            // 不强制绑定 faceMove，但给出提示以便调试
            Debug.Log("CardCostController: faceMove 未绑定，跳过 UI 同步");
            return;
        }

        int max = Mathf.Max(1, faceMove.maxTendencyValue); // 防止除0
        float normalized = Mathf.Clamp01(card.cardRank / (float)max);

        switch (card.cardType)
        {
            case Card.CARD_TYPE.Reason:
                if (faceMove.scrollBar_Ego != null) faceMove.scrollBar_Ego.value = normalized;
                break;
            case Card.CARD_TYPE.Feel:
                if (faceMove.scrollBar_Id != null) faceMove.scrollBar_Id.value = normalized;
                break;
            case Card.CARD_TYPE.Dream:
                if (faceMove.scrollBar_Superego != null) faceMove.scrollBar_Superego.value = normalized;
                break;
        }
        // 直接设置 scrollbar.value 会触发 Face_Move 的 onValueChanged，进而更新文本与写回列表卡牌（保持复用逻辑）
    }

    // 便捷方法：用 Face_Move 绑定的卡组按顺序循环消费（示例）
    public List<Card> ConsumeAllReasonCards(int rankIncreasePerCard = -1)
    {
        if (faceMove == null) return new List<Card>();
        return ConsumeCardsLoop(faceMove.reasonCards, rankIncreasePerCard);
    }
}
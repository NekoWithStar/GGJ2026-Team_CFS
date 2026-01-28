using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardControl : MonoBehaviour
{
    public Card card_data; // 卡牌数据

    // 卡牌元素控制
    public Image picture; // 卡牌图片
    public Text card_name; // 卡牌名称文本
    public Text describe; // 描述文本
    public Text cost; // 费用文本
    public Text rank; // 倾向等级文本
    public GameObject back; // 卡牌背面图片

    private void Awake()
    {
        picture = card_data.cardPicture;
        describe.text = card_data.cardDescription;
        card_name.text = card_data.cardName;
        cost.text = card_data.cardCost.ToString();
        rank.text = card_data.cardRank.ToString();
        back = card_data.back;
    }

    private void OnEnable()
    {
        SimpleScrollbar.OnRankChanged += HandleRankChanged;
    }

    private void OnDisable()
    {
        SimpleScrollbar.OnRankChanged -= HandleRankChanged;
    }

    // 当滚动条广播该类型 rank 改变时，更新 ScriptableObject 与 UI（最小入侵）
    private void HandleRankChanged(Card.CARD_TYPE type, int newRank)
    {
        if (card_data == null) return;
        if (card_data.cardType != type) return;

        // 更新数据 并同步 UI 文本
        card_data.cardRank = newRank;
        if (rank != null) rank.text = newRank.ToString();
    }
}

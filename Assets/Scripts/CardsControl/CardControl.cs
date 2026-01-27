using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardControl : MonoBehaviour
{
    public Card card_data; // 卡牌数据

    // 卡牌元素控制
    public SpriteRenderer picture; // 卡牌图片
    public Text card_name; // 卡牌名称文本
    public Text describe; // 描述文本
    public Text cost; // 费用文本
    public Text rank; // 倾向等级文本

    private void Awake()
    {
        picture.sprite = card_data.cardPicture;
        describe.text = card_data.cardDescription;
        card_name.text = card_data.cardName;
        cost.text = card_data.cardCost.ToString();
        rank.text = card_data.cardRank.ToString();
    }
}

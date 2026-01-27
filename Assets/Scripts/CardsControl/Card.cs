using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName =  "Card")]
public class Card : ScriptableObject
{
    // 卡牌类型属性
    public enum CARD_TYPE
    {
        Reason, //理性组
        Feel,   //感受组
        Dream   //梦想组
    }

    public CARD_TYPE cardType; // 卡牌类型
    public string cardName;   // 卡牌名称
    public int cardCost;     // 卡牌费用
    public int cardRank;     // 卡牌倾向等级
    public string cardDescription; // 卡牌描述
    public Sprite cardPicture; // 卡牌图片
}

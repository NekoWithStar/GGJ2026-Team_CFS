using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Y_Survivor;

/// <summary>
/// 卡牌选择管理器：当coin足够时弹出卡牌选择窗口
/// </summary>
public class CardSelectionManager : MonoBehaviour
{
    [Header("UI配置")]
    public GameObject cardSelectionPanel; // 选择面板
    public Transform cardContainer; // 卡牌容器
    public GameObject cardPrefab; // 卡牌UI预制体（Flip_Card或简化版）
    public Button confirmButton; // 确认按钮（可选）

    [Header("选择配置")]
    public int cardsToShow = 3; // 显示卡牌数量

    private List<GameObject> currentCards = new List<GameObject>();
    private CardPoolManager cardPool;
    private PlayerControl player;

    private void Awake()
    {
        // Use newer APIs to avoid deprecated FindObjectOfType
        cardPool = FindAnyObjectByType<CardPoolManager>();
        player = FindAnyObjectByType<PlayerControl>();

        if (cardSelectionPanel != null)
        {
            cardSelectionPanel.SetActive(false);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmSelection);
        }
    }

    /// <summary>
    /// 显示卡牌选择
    /// </summary>
    public void ShowCardSelection(int count = 3)
    {
        if (cardPool == null || cardSelectionPanel == null) return;

        cardsToShow = count;

        // 获取随机卡牌
        var cards = cardPool.GetRandomCards(cardsToShow);
        if (cards.Count == 0) return;

        // 清空旧卡牌
        ClearCurrentCards();

        // 生成新卡牌UI
        foreach (var card in cards)
        {
            GameObject cardUI = Instantiate(cardPrefab, cardContainer);
            // 配置卡牌UI（假设cardPrefab有相应脚本）
            var cardControl = cardUI.GetComponent<PropertyCardControl>();
            if (cardControl != null && card is PropertyCard propertyCard)
            {
                cardControl.SetupCard(propertyCard);
            }
            else
            {
                var weaponControl = cardUI.GetComponent<WeaponCardControl>();
                if (weaponControl != null && card is Weapon weapon)
                {
                    weaponControl.SetupCard(weapon);
                }
            }

            currentCards.Add(cardUI);
        }

        // 显示面板
        cardSelectionPanel.SetActive(true);
    }

    /// <summary>
    /// 隐藏卡牌选择
    /// </summary>
    public void HideCardSelection()
    {
        cardSelectionPanel.SetActive(false);
        ClearCurrentCards();
    }

    /// <summary>
    /// 确认选择（消费coin并恢复游戏）
    /// </summary>
    private void OnConfirmSelection()
    {
        // 消费coin
        if (player != null)
        {
            player.coin -= 100;
            if (player.coin < 0) player.coin = 0;
            Debug.Log($"消费100金币，剩余金币: {player.coin}");
        }

        // 恢复游戏
        if (player != null)
        {
            player.ResumeGame();
        }

        HideCardSelection();
    }

    /// <summary>
    /// 清空当前卡牌
    /// </summary>
    private void ClearCurrentCards()
    {
        foreach (var card in currentCards)
        {
            Destroy(card);
        }
        currentCards.Clear();
    }
}
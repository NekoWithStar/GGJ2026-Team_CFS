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
    public Transform cardContainer; // 卡牌容器（需要设置合适的尺寸和锚点）
    public GameObject cardPrefab; // 卡牌UI预制体（Flip_Card或简化版）

    [Header("确认按钮（可选）")]
    public Button confirmButton1; // 确认第一张卡牌的按钮
    public Button confirmButton2; // 确认第二张卡牌的按钮
    public Button confirmButton3; // 确认第三张卡牌的按钮
    public Button confirmButton4; // 确认第四张卡牌的按钮
    public Button cancelButton; // 取消按钮

    [Header("布局配置")]
    public List<Transform> cardPositions; // 指定卡牌位置点（可选，如果为空则使用自动布局）
    public float cardSpacing = 200f; // 卡牌之间的间距（自动布局时使用）
    public float cardWidth = 150f; // 卡牌宽度（用于计算布局）

    [Header("选择配置")]
    public int cardsToShow = 3; // 显示卡牌数量

    private List<GameObject> currentCards = new List<GameObject>();
    private List<ScriptableObject> currentCardData = new List<ScriptableObject>(); // 存储当前显示的卡牌数据
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

        // 监听卡牌确认事件
        Flip_Card.OnCardConfirmed += OnCardSelected;
        Flip_Card.OnWeaponConfirmed += OnWeaponSelected;
        Flip_Card.OnPropertyCardConfirmed += OnPropertyCardSelected;

        // 设置确认按钮监听器
        SetupConfirmButtons();
    }

    /// <summary>
    /// 设置确认按钮的监听器
    /// </summary>
    private void SetupConfirmButtons()
    {
        if (confirmButton1 != null)
            confirmButton1.onClick.AddListener(() => ConfirmCardByIndex(0));
        if (confirmButton2 != null)
            confirmButton2.onClick.AddListener(() => ConfirmCardByIndex(1));
        if (confirmButton3 != null)
            confirmButton3.onClick.AddListener(() => ConfirmCardByIndex(2));
        if (confirmButton4 != null)
            confirmButton4.onClick.AddListener(() => ConfirmCardByIndex(3));
        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelSelection);
    }

    /// <summary>
    /// 取消卡牌选择
    /// </summary>
    private void CancelSelection()
    {
        Debug.Log("[CardSelectionManager] ❌ 取消卡牌选择");
        HideCardSelection();
        // 恢复游戏但不消耗金币
        if (player != null)
        {
            player.ResumeGame();
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
        currentCardData.Clear();

        // 生成新卡牌UI并排列
        for (int i = 0; i < cards.Count; i++)
        {
            GameObject cardUI = Instantiate(cardPrefab, cardContainer);
            
            // 设置位置
            if (cardPositions != null && i < cardPositions.Count && cardPositions[i] != null)
            {
                // 使用指定位置点
                cardUI.transform.position = cardPositions[i].position;
                cardUI.transform.rotation = cardPositions[i].rotation;
            }
            else
            {
                // 自动布局（回退方案）
                RectTransform rectTransform = cardUI.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    float totalWidth = (cards.Count - 1) * cardSpacing;
                    float startX = -totalWidth / 2f;
                    rectTransform.anchoredPosition = new Vector2(startX + i * cardSpacing, 0f);
                }
            }
            
            // 设置Flip_Card为选择模式
            var flipCard = cardUI.GetComponent<Flip_Card>();
            if (flipCard != null)
            {
                flipCard.secondClickIsConfirm = true;
                Debug.Log($"[CardSelectionManager] ✅ 设置卡牌 {i+1} 的 Flip_Card.secondClickIsConfirm = true");
            }
            else
            {
                Debug.LogError($"[CardSelectionManager] ❌ 卡牌 {i+1} 缺少 Flip_Card 组件！");
            }
            
            // 配置卡牌UI - 同时查找两个组件，根据卡牌类型决定使用哪个
            PropertyCardControl propertyControl = cardUI.GetComponent<PropertyCardControl>();
            WeaponCardControl weaponControl = cardUI.GetComponent<WeaponCardControl>();

            if (cards[i] is PropertyCard propertyCard)
            {
                if (propertyControl != null)
                {
                    propertyControl.SetupCard(propertyCard);
                    currentCardData.Add(propertyCard);
                    Debug.Log($"[CardSelectionManager] ✅ 配置属性卡: {propertyCard.cardName}");
                }
                else
                {
                    Debug.LogError($"[CardSelectionManager] ❌ 属性卡 {propertyCard.cardName} 缺少 PropertyCardControl 组件");
                }
            }
            else if (cards[i] is Weapon weapon)
            {
                if (weaponControl != null)
                {
                    weaponControl.SetupCard(weapon);
                    currentCardData.Add(weapon);
                    Debug.Log($"[CardSelectionManager] ✅ 配置武器卡: {weapon.weaponName}");
                }
                else
                {
                    Debug.LogError($"[CardSelectionManager] ❌ 武器卡 {weapon.weaponName} 缺少 WeaponCardControl 组件");
                }
            }
            else
            {
                currentCardData.Add(cards[i]);
                Debug.LogWarning($"[CardSelectionManager] ⚠️ 未知卡牌类型: {cards[i].GetType().Name}");
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
    /// 手动确认选择第一张卡牌（临时解决方案，绕过Flip_Card确认问题）
    /// </summary>
    public void ConfirmFirstCard()
    {
        if (currentCardData.Count > 0 && currentCardData[0] != null)
        {
            Debug.Log($"[CardSelectionManager] 🎯 手动确认第一张卡牌: {currentCardData[0].GetType().Name}");
            ApplyCardEffect(currentCardData[0]);
        }
        else
        {
            Debug.LogWarning("[CardSelectionManager] ⚠️ 没有可确认的卡牌");
        }
    }

    /// <summary>
    /// 手动确认选择指定索引的卡牌
    /// </summary>
    public void ConfirmCardByIndex(int index)
    {
        if (index >= 0 && index < currentCardData.Count && currentCardData[index] != null)
        {
            Debug.Log($"[CardSelectionManager] 🎯 手动确认第{index+1}张卡牌: {currentCardData[index].GetType().Name}");
            ApplyCardEffect(currentCardData[index]);
        }
        else
        {
            Debug.LogWarning($"[CardSelectionManager] ⚠️ 无效的卡牌索引: {index}");
        }
    }

    /// <summary>
    /// 当普通卡牌被选择时
    /// </summary>
    public void OnCardSelected(Card card)
    {
        ApplyCardEffect(card);
    }

    /// <summary>
    /// 当武器卡牌被选择时
    /// </summary>
    private void OnWeaponSelected(Weapon weapon)
    {
        ApplyCardEffect(weapon);
    }

    /// <summary>
    /// 当属性卡牌被选择时
    /// </summary>
    private void OnPropertyCardSelected(Y_Survivor.PropertyCard propertyCard)
    {
        ApplyCardEffect(propertyCard);
    }

    /// <summary>
    /// 应用卡牌效果 - 现在委托给 CardPoolManager 统一处理
    /// </summary>
    private void ApplyCardEffect(ScriptableObject card)
    {
        if (cardPool == null)
        {
            Debug.LogError("[CardSelectionManager] CardPoolManager未找到，无法应用卡牌");
            HideCardSelection();
            return;
        }

        // 委托给 CardPoolManager.ApplyCard() - 它会处理：
        // 1. 检查金币是否足够
        // 2. 应用卡牌效果
        // 3. 消耗金币
        // 4. 恢复游戏
        // 5. 更新UI
        bool success = cardPool.ApplyCard(card);
        
        if (success)
        {
            Debug.Log($"[CardSelectionManager] ✅ 卡牌应用成功: {(card is PropertyCard pc ? pc.cardName : card is Weapon w ? w.weaponName : "Unknown")}");
        }
        else
        {
            Debug.LogWarning("[CardSelectionManager] ⚠️ 卡牌应用失败（可能金币不足）");
        }

        HideCardSelection();
    }

    private void Update()
    {
        // 卡牌选择期间的快捷键
        if (cardSelectionPanel != null && cardSelectionPanel.activeSelf)
        {
            // 数字键1-3确认对应卡牌
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                ConfirmCardByIndex(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                ConfirmCardByIndex(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                ConfirmCardByIndex(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                ConfirmCardByIndex(3);
            }
            // ESC键取消选择
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[CardSelectionManager] ❌ 取消卡牌选择");
                HideCardSelection();
                // 恢复游戏但不消耗金币
                if (player != null)
                {
                    player.ResumeGame();
                }
            }
        }
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
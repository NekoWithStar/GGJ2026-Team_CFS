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

    [Header("布局配置")]
    public List<Transform> cardPositions; // 指定卡牌位置点（可选，如果为空则使用自动布局）
    public float cardSpacing = 200f; // 卡牌之间的间距（自动布局时使用）
    public float cardWidth = 150f; // 卡牌宽度（用于计算布局）

    [Header("位置标记绑定")]
    [Tooltip("自动绑定场景中带此 Tag 的位置标记（可选）")]
    public string cardSlotTag = "CardSlot";

    [Tooltip("自动绑定场景中名称以此开头的标记（可选）")]
    public string cardSlotNamePrefix = "CardSlot_";

    [Header("选择配置")]
    public int cardsToShow = 3; // 显示卡牌数量

    private List<GameObject> currentCards = new List<GameObject>();
    private List<ScriptableObject> currentCardData = new List<ScriptableObject>(); // 存储当前显示的卡牌数据
    private CardPoolManager cardPool;
    private PlayerControl player;
    private bool isInitialized = false;

    private void Awake()
    {
        InitializeManager();
    }

    /// <summary>
    /// 初始化管理器 - 可以从 Awake 或 ShowCardSelection 中调用
    /// </summary>
    private void InitializeManager()
    {
        if (isInitialized)
        {
            Debug.Log("[CardSelectionManager] ⚠️ 已经初始化过了，跳过重复初始化");
            return;
        }

        // Use newer APIs to avoid deprecated FindObjectOfType
        cardPool = FindAnyObjectByType<CardPoolManager>();
        player = FindAnyObjectByType<PlayerControl>();

        Debug.Log($"[CardSelectionManager] 📢 初始化 - cardPool={cardPool != null}, player={player != null}, cardSelectionPanel={cardSelectionPanel != null}");

        // 尝试解析 cardSelectionPanel（包含未激活对象）
        if (!TryResolveCardSelectionPanel())
        {
            Debug.LogError("[CardSelectionManager] ❌ 无法找到cardSelectionPanel！请在Inspector中手动赋值或确保场景中存在名为'CardSelectionPanel'的对象");
        }

        if (cardSelectionPanel != null)
        {
            cardSelectionPanel.SetActive(false);
            Debug.Log("[CardSelectionManager] ✅ cardSelectionPanel初始化为非活动状态");
        }
        else
        {
            Debug.LogError("[CardSelectionManager] ❌ cardSelectionPanel为null，无法禁用！");
        }

        // 如果 cardContainer 也没有赋值，尝试自动查找
        TryResolveCardContainer();

        // 自动绑定位置标记（如果未手动设置）
        ResolveCardPositions();

        // 监听卡牌确认事件
        Flip_Card.OnCardConfirmed += OnCardSelected;
        Flip_Card.OnWeaponConfirmed += OnWeaponSelected;
        Flip_Card.OnPropertyCardConfirmed += OnPropertyCardSelected;
        Debug.Log("[CardSelectionManager] ✅ 已注册卡牌确认事件监听");
        
        isInitialized = true;
    }

    /// <summary>
    /// 显示卡牌选择
    /// </summary>
    public bool ShowCardSelection(int count = 3)
    {
        // 确保已初始化（如果 Awake 没有执行）
        if (!isInitialized)
        {
            Debug.LogWarning("[CardSelectionManager] ⚠️ ShowCardSelection 被调用时还未初始化，现在执行初始化...");
            InitializeManager();
        }

        Debug.Log($"[CardSelectionManager] 📢 ShowCardSelection被调用 - count={count}, cardPool={cardPool != null}, cardSelectionPanel={cardSelectionPanel != null}");

        // 每次显示前尝试刷新位置标记（避免重载后引用丢失）
        ResolveCardPositions();
        
        if (cardPool == null)
        {
            Debug.LogError("[CardSelectionManager] ❌ cardPool为null！无法显示卡牌选择");
            return false;
        }
        
        // 二次检查 cardSelectionPanel（包含未激活对象）
        if (!TryResolveCardSelectionPanel())
        {
            Debug.LogError("[CardSelectionManager] ❌ cardSelectionPanel为null！无法显示卡牌选择窗口。请确保场景中存在名为'CardSelectionPanel'的GameObject，或在Inspector中手动赋值");
            return false;
        }

        // 确保 cardContainer 可用
        if (cardContainer == null)
        {
            Debug.LogWarning("[CardSelectionManager] ⚠️ cardContainer为null，尝试重新查找或使用cardSelectionPanel作为容器");
            TryResolveCardContainer();
        }
        if (cardContainer == null)
        {
            Debug.LogError("[CardSelectionManager] ❌ cardContainer为null！无法生成卡牌UI");
            return false;
        }

        cardsToShow = count;

        // 获取随机卡牌
        var cards = cardPool.GetRandomCards(cardsToShow);
        if (cards.Count == 0) return false;

        // 清空旧卡牌
        ClearCurrentCards();
        currentCardData.Clear();

        // 预检查：是否使用预设位置
        bool usePresetPositions = cardPositions != null && cardPositions.Count >= cards.Count;
        if (usePresetPositions)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (cardPositions[i] == null)
                {
                    usePresetPositions = false;
                    break;
                }
            }
        }

        // 如果容器存在布局组件，则优先交给布局组件处理
        var layoutGroup = cardContainer != null ? cardContainer.GetComponent<UnityEngine.UI.LayoutGroup>() : null;
        if (layoutGroup != null && !usePresetPositions)
        {
            if (layoutGroup is HorizontalLayoutGroup h)
            {
                h.spacing = cardSpacing;
            }
            if (layoutGroup is VerticalLayoutGroup v)
            {
                v.spacing = cardSpacing;
            }
        }

        // 生成新卡牌UI并排列
        for (int i = 0; i < cards.Count; i++)
        {
            GameObject cardUI = Instantiate(cardPrefab, cardContainer);
            // 确保Transform重置，避免第二次堆叠在一起
            RectTransform cardRect = cardUI.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                cardRect.localScale = Vector3.one;
                cardRect.localRotation = Quaternion.identity;
                cardRect.anchoredPosition = Vector2.zero;
                cardRect.anchoredPosition3D = Vector3.zero;
            }
            
            // 设置位置
            if (usePresetPositions && i < cardPositions.Count)
            {
                // 使用指定位置点
                var preset = cardPositions[i];
                var presetRect = preset.GetComponent<RectTransform>();
                if (cardRect != null && presetRect != null)
                {
                    cardRect.anchoredPosition = presetRect.anchoredPosition;
                    cardRect.localRotation = presetRect.localRotation;
                    cardRect.localScale = presetRect.localScale;
                }
                else
                {
                    cardUI.transform.localPosition = preset.localPosition;
                    cardUI.transform.localRotation = preset.localRotation;
                    cardUI.transform.localScale = preset.localScale;
                }
            }
            else
            {
                // 自动布局（回退方案）
                if (layoutGroup == null)
                {
                    RectTransform rectTransform = cardUI.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        float totalWidth = (cards.Count - 1) * cardSpacing;
                        float startX = -totalWidth / 2f;
                        rectTransform.anchoredPosition = new Vector2(startX + i * cardSpacing, 0f);
                    }
                }
                else
                {
                    // 交给布局组件时，保持零偏移
                    if (cardRect != null)
                    {
                        cardRect.anchoredPosition = Vector2.zero;
                    }
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
                    // 查找场景中现有的 WeaponCardControl 实例
                    WeaponCardControl[] sceneWeaponControls = FindObjectsByType<WeaponCardControl>(FindObjectsSortMode.None);
                    if (sceneWeaponControls.Length > 0)
                    {
                        // 使用第一个找到的 WeaponCardControl
                        weaponControl = sceneWeaponControls[0];
                        weaponControl.SetupCard(weapon);
                        currentCardData.Add(weapon);
                        Debug.Log($"[CardSelectionManager] ✅ 使用场景中的 WeaponCardControl 配置武器卡: {weapon.weaponName}");
                    }
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
        // 强制刷新布局，避免再次显示时重叠
        RectTransform containerRect = cardContainer as RectTransform;
        if (containerRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
        }
        Debug.Log($"[CardSelectionManager] ✅ 卡牌选择面板已显示 - 生成了 {currentCards.Count} 张卡牌");
        return true;
    }

    private void ResolveCardPositions()
    {
        // 如果已有手动配置且有效，则不覆盖
        if (cardPositions != null && cardPositions.Count > 0)
        {
            bool hasValid = false;
            foreach (var t in cardPositions)
            {
                if (t != null)
                {
                    hasValid = true;
                    break;
                }
            }
            if (hasValid) return;
        }

        var resolved = new List<Transform>();

        // 1) 按 Tag 绑定（需要在 Unity 中创建并应用 Tag）
        if (!string.IsNullOrEmpty(cardSlotTag))
        {
            try
            {
                var tagged = GameObject.FindGameObjectsWithTag(cardSlotTag);
                if (tagged != null && tagged.Length > 0)
                {
                    foreach (var go in tagged)
                    {
                        resolved.Add(go.transform);
                    }
                }
            }
            catch
            {
                // Tag 未定义会抛异常，忽略即可
            }
        }

        // 2) 按名称前缀绑定
        if (resolved.Count == 0 && !string.IsNullOrEmpty(cardSlotNamePrefix))
        {
            var allTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var t in allTransforms)
            {
                if (t.name.StartsWith(cardSlotNamePrefix))
                {
                    resolved.Add(t);
                }
            }
        }

        // 排序：按名称确保稳定顺序（CardSlot_0,1,2,3）
        if (resolved.Count > 1)
        {
            resolved.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        }

        if (resolved.Count > 0)
        {
            cardPositions = resolved;
            Debug.Log($"[CardSelectionManager] ✅ 已自动绑定 {cardPositions.Count} 个位置标记");
        }
    }

    private bool TryResolveCardSelectionPanel()
    {
        if (cardSelectionPanel != null) return true;

        Debug.LogWarning("[CardSelectionManager] ⚠️ cardSelectionPanel为null，尝试解析（包含未激活对象）...");

        // 1) 直接查找（仅激活）
        cardSelectionPanel = GameObject.Find("CardSelectionPanel");
        if (cardSelectionPanel != null)
        {
            Debug.Log("[CardSelectionManager] ✅ 通过GameObject.Find找到CardSelectionPanel");
            return true;
        }

        // 2) 查找所有 Transform（包含未激活）并匹配名称
        Transform[] allTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform t in allTransforms)
        {
            if (t.gameObject.name == "CardSelectionPanel")
            {
                cardSelectionPanel = t.gameObject;
                Debug.Log("[CardSelectionManager] ✅ 在未激活对象中找到CardSelectionPanel");
                return true;
            }
        }

        // 3) 如果有 cardContainer，向上查找包含 Selection/Card 的父级
        if (cardContainer != null)
        {
            Transform parent = cardContainer.parent;
            while (parent != null)
            {
                if (parent.gameObject.name.Contains("Selection") || parent.gameObject.name.Contains("Card"))
                {
                    cardSelectionPanel = parent.gameObject;
                    Debug.Log($"[CardSelectionManager] ✅ 通过cardContainer父级找到CardSelectionPanel: {parent.gameObject.name}");
                    return true;
                }
                parent = parent.parent;
            }
        }

        // 4) 退化：查找包含关键字的 Canvas（包含未激活）
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            if (canvas.gameObject.name.Contains("Selection") || canvas.gameObject.name.Contains("Card"))
            {
                cardSelectionPanel = canvas.gameObject;
                Debug.Log($"[CardSelectionManager] ✅ 在Canvas中找到CardSelectionPanel: {canvas.gameObject.name}");
                return true;
            }
        }

        return false;
    }

    private void TryResolveCardContainer()
    {
        if (cardContainer != null) return;

        if (cardSelectionPanel == null) return;

        Debug.LogWarning("[CardSelectionManager] ⚠️ cardContainer未在Inspector中赋值，尝试在cardSelectionPanel中查找");

        // 优先找名为 CardContainer/Container 的子物体
        Transform[] allChildren = cardSelectionPanel.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child.gameObject.name.Contains("CardContainer") || child.gameObject.name.Contains("Container"))
            {
                cardContainer = child;
                Debug.Log($"[CardSelectionManager] ✅ 通过名称找到cardContainer: {child.name}");
                return;
            }
        }

        // 再尝试找带布局组件的对象
        foreach (Transform child in allChildren)
        {
            if (child.GetComponent<HorizontalLayoutGroup>() != null
                || child.GetComponent<VerticalLayoutGroup>() != null
                || child.GetComponent<GridLayoutGroup>() != null)
            {
                cardContainer = child;
                Debug.Log($"[CardSelectionManager] ✅ 通过布局组件找到cardContainer: {child.name}");
                return;
            }
        }

        // 最后退化：使用面板本身
        cardContainer = cardSelectionPanel.transform;
        Debug.Log("[CardSelectionManager] ✅ 使用cardSelectionPanel作为cardContainer");
    }

    /// <summary>
    /// 隐藏卡牌选择
    /// </summary>
    public void HideCardSelection()
    {
        Debug.Log("[CardSelectionManager] 📢 HideCardSelection被调用");
        cardSelectionPanel.SetActive(false);
        ClearCurrentCards();
    }

    /// <summary>
    /// 当普通卡牌被选择时
    /// </summary>
    public void OnCardSelected(Card card)
    {
        Debug.Log($"[CardSelectionManager] 📢 OnCardSelected 事件被触发: {card.cardName}");
        ApplyCardEffect(card);
    }

    /// <summary>
    /// 当武器卡牌被选择时
    /// </summary>
    private void OnWeaponSelected(Weapon weapon)
    {
        Debug.Log($"[CardSelectionManager] 📢 OnWeaponSelected 事件被触发: {weapon.weaponName}");
        ApplyCardEffect(weapon);
    }

    /// <summary>
    /// 当属性卡牌被选择时
    /// </summary>
    private void OnPropertyCardSelected(Y_Survivor.PropertyCard propertyCard)
    {
        Debug.Log($"[CardSelectionManager] 📢 OnPropertyCardSelected 事件被触发: {propertyCard.cardName}");
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

    private void OnDestroy()
    {
        Debug.Log("[CardSelectionManager] 📢 OnDestroy - 清理事件监听");
        // 清空卡牌
        ClearCurrentCards();
        currentCardData.Clear();
        
        // 取消事件监听
        Flip_Card.OnCardConfirmed -= OnCardSelected;
        Flip_Card.OnWeaponConfirmed -= OnWeaponSelected;
        Flip_Card.OnPropertyCardConfirmed -= OnPropertyCardSelected;
        Debug.Log("[CardSelectionManager] ✅ 已注销卡牌确认事件监听");
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
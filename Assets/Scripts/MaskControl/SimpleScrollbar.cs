using System;
using UnityEngine;
using UnityEngine.UI;

public class SimpleScrollbar : MonoBehaviour
{
    // 广播：当某一类倾向的 rank 改变时通知订阅者 (type, newRank)
    public static event Action<Card.CARD_TYPE, int> OnRankChanged;

    [Header("自我倾向（理性）")]
    public Scrollbar scrollBar_Ego;    // 自我滚动条
    public Text text_EgoValue;         // 自我数值文本
    private int _egoValue;             // 自我倾向值

    [Header("本我倾向（感受）")]
    public Scrollbar scrollBar_Id;     // 本我滚动条
    public Text text_IdValue;          // 本我数值文本
    private int _idValue;              // 本我倾向值

    [Header("超我倾向（理想）")]
    public Scrollbar scrollBar_Superego; // 超我滚动条
    public Text text_SuperegoValue;     // 超我数值文本
    private int _superegoValue;         // 超我倾向值

    [Header("配置")]
    public int maxTendencyValue = 30;  // 倾向值最大值（统一归一到0~30）

    [Header("动画")]
    [Tooltip("确认卡牌时滚动条增加的平滑动画时长（秒）")]
    public float confirmTweenDuration = 0.5f;

    // 抑制 OnValueChanged 中的“写回卡牌”逻辑（用于程序化更新滚动条，不应覆盖卡牌原有 rank）
    private bool suppressOnValueChanged = false;

    void OnEnable()
    {
        // 订阅卡牌确认广播
        Flip_Card.OnCardConfirmed += HandleCardConfirmed;
    }

    void OnDisable()
    {
        Flip_Card.OnCardConfirmed -= HandleCardConfirmed;
    }

    void Start()
    {
        // 初始化默认 rank 为 0（不从场景聚合）
        _egoValue = 0;
        _idValue = 0;
        _superegoValue = 0;

        if (maxTendencyValue <= 0) maxTendencyValue = 30;

        // 初始化 UI（在添加监听器前设置值，避免触发写回）
        if (scrollBar_Ego != null) scrollBar_Ego.value = 0f;
        if (scrollBar_Id != null) scrollBar_Id.value = 0f;
        if (scrollBar_Superego != null) scrollBar_Superego.value = 0f;

        UpdateEgoText(_egoValue);
        UpdateIdText(_idValue);
        UpdateSuperegoText(_superegoValue);

        // 绑定用户交互监听器（此后用户拖动会触发写回卡牌的逻辑）
        InitScrollbars();
    }

    #region 初始化滚动条（独立绑定）
    void InitScrollbars()
    {
        // 每个滚动条只绑定自己的更新方法，彼此无关联
        if (scrollBar_Ego != null) scrollBar_Ego.onValueChanged.AddListener(OnEgoScrollChanged);
        if (scrollBar_Id != null) scrollBar_Id.onValueChanged.AddListener(OnIdScrollChanged);
        if (scrollBar_Superego != null) scrollBar_Superego.onValueChanged.AddListener(OnSuperegoScrollChanged);
    }
    #endregion
    #region 处理卡牌确认（Flip_Card 广播） - 使用 DOTween 平滑增加
    void HandleCardConfirmed(Card card)
    {
        if (card == null) return;

        switch (card.cardType)
        {
            case Card.CARD_TYPE.Reason:
                _egoValue = Mathf.Clamp(_egoValue + card.cardRank, 0, maxTendencyValue);
                // 程序化更新滚动条时抑制写回卡牌的逻辑
                suppressOnValueChanged = true;
                if (scrollBar_Ego != null) scrollBar_Ego.value = Mathf.Clamp01((float)_egoValue / maxTendencyValue);
                UpdateEgoText(_egoValue);
                suppressOnValueChanged = false;
                break;

            case Card.CARD_TYPE.Feel:
                _idValue = Mathf.Clamp(_idValue + card.cardRank, 0, maxTendencyValue);
                suppressOnValueChanged = true;
                if (scrollBar_Id != null) scrollBar_Id.value = Mathf.Clamp01((float)_idValue / maxTendencyValue);
                UpdateIdText(_idValue);
                suppressOnValueChanged = false;
                break;

            case Card.CARD_TYPE.Dream:
                _superegoValue = Mathf.Clamp(_superegoValue + card.cardRank, 0, maxTendencyValue);
                suppressOnValueChanged = true;
                if (scrollBar_Superego != null) scrollBar_Superego.value = Mathf.Clamp01((float)_superegoValue / maxTendencyValue);
                UpdateSuperegoText(_superegoValue);
                suppressOnValueChanged = false;
                break;
        }
        // 不把程序化变化写回到各卡牌（避免覆盖卡牌数据）
        Debug.Log($"Confirmed card '{card.cardName}' added {card.cardRank} to {card.cardType}, new values: Ego={_egoValue}, Id={_idValue}, Superego={_superegoValue}");
    }
    #endregion

    #region 三个滚动条的独立逻辑（核心归一映射）
    /// <summary>
    /// 自我滚动条值变化（用户交互时）
    /// </summary>
    public void OnEgoScrollChanged(float scrollValue)
    {
        // 归一映射：0~1 → 0~maxTendencyValue
        _egoValue = Mathf.RoundToInt(scrollValue * maxTendencyValue);
        _egoValue = Mathf.Clamp(_egoValue, 0, maxTendencyValue);
        // 只更新自己的文本
        UpdateEgoText(_egoValue);

        // 如果是程序化触发（例如确认卡牌时设置 value），则不把值写回卡牌
        if (suppressOnValueChanged) return;

        // 广播变化给订阅者（CardControl 将会把 card_data.cardRank 更新为该值）
        OnRankChanged?.Invoke(Card.CARD_TYPE.Reason, _egoValue);

        // 可选：解锁提示（保留核心规则，无多余逻辑）
        if (_egoValue >= 10) Debug.Log("自我倾向≥10，解锁理性卡Rank2");
        if (_egoValue >= 24) Debug.Log("自我倾向≥24，解锁理性卡Rank3");
    }

    /// <summary>
    /// 本我滚动条值变化（用户交互时）
    /// </summary>
    public void OnIdScrollChanged(float scrollValue)
    {
        _idValue = Mathf.RoundToInt(scrollValue * maxTendencyValue);
        _idValue = Mathf.Clamp(_idValue, 0, maxTendencyValue);
        UpdateIdText(_idValue);

        if (suppressOnValueChanged) return;

        OnRankChanged?.Invoke(Card.CARD_TYPE.Feel, _idValue);
        if (_idValue >= 10) Debug.Log("本我倾向≥10，解锁感受卡Rank2");
        if (_idValue >= 24) Debug.Log("本我倾向≥24，解锁感受卡Rank3");
    }

    /// <summary>
    /// 超我滚动条值变化（用户交互时）
    /// </summary>
    public void OnSuperegoScrollChanged(float scrollValue)
    {
        _superegoValue = Mathf.RoundToInt(scrollValue * maxTendencyValue);
        _superegoValue = Mathf.Clamp(_superegoValue, 0, maxTendencyValue);
        UpdateSuperegoText(_superegoValue);

        if (suppressOnValueChanged) return;

        OnRankChanged?.Invoke(Card.CARD_TYPE.Dream, _superegoValue);
        if (_superegoValue >= 10) Debug.Log("超我倾向≥10，解锁理想卡Rank2");
        if (_superegoValue >= 24) Debug.Log("超我倾向≥24，解锁理想卡Rank3");
    }
    #endregion

    #region 独立更新文本（极简）
    void UpdateEgoText(int value)
    {
        if (text_EgoValue != null) text_EgoValue.text = $"Reason：{value}";
    }

    void UpdateIdText(int value)
    {
        if (text_IdValue != null) text_IdValue.text = $"Feel：{value}";
    }

    void UpdateSuperegoText(int value)
    {
        if (text_SuperegoValue != null) text_SuperegoValue.text = $"Dream：{value}";
    }
    #endregion
}
using System;
using UnityEngine;
using UnityEngine.UI;

// 仅保留三个独立滚动条的核心逻辑，无任何多余功能
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

    void Start()
    {
        // 初始化：绑定每个滚动条的事件
        InitScrollbars();
        // 初始数值设为0
        UpdateEgoText(0);
        UpdateIdText(0);
        UpdateSuperegoText(0);
    }

    #region 1. 初始化滚动条（独立绑定）
    void InitScrollbars()
    {
        // 每个滚动条只绑定自己的更新方法，彼此无关联
        if (scrollBar_Ego != null) scrollBar_Ego.onValueChanged.AddListener(OnEgoScrollChanged);
        if (scrollBar_Id != null) scrollBar_Id.onValueChanged.AddListener(OnIdScrollChanged);
        if (scrollBar_Superego != null) scrollBar_Superego.onValueChanged.AddListener(OnSuperegoScrollChanged);
    }
    #endregion

    #region 2. 三个滚动条的独立逻辑（核心归一映射）
    /// <summary>
    /// 自我滚动条值变化
    /// </summary>
    public void OnEgoScrollChanged(float scrollValue)
    {
        // 归一映射：0~1 → 0~maxTendencyValue
        _egoValue = Mathf.RoundToInt(scrollValue * maxTendencyValue);
        _egoValue = Mathf.Clamp(_egoValue, 0, maxTendencyValue);
        // 只更新自己的文本
        UpdateEgoText(_egoValue);
        // 广播变化
        OnRankChanged?.Invoke(Card.CARD_TYPE.Reason, _egoValue);
        // 可选：解锁提示（保留核心规则，无多余逻辑）
        if (_egoValue >= 10) Debug.Log("自我倾向≥10，解锁理性卡Rank2");
        if (_egoValue >= 24) Debug.Log("自我倾向≥24，解锁理性卡Rank3");
    }

    /// <summary>
    /// 本我滚动条值变化
    /// </summary>
    public void OnIdScrollChanged(float scrollValue)
    {
        _idValue = Mathf.RoundToInt(scrollValue * maxTendencyValue);
        _idValue = Mathf.Clamp(_idValue, 0, maxTendencyValue);
        UpdateIdText(_idValue);
        OnRankChanged?.Invoke(Card.CARD_TYPE.Feel, _idValue);
        if (_idValue >= 10) Debug.Log("本我倾向≥10，解锁感受卡Rank2");
        if (_idValue >= 24) Debug.Log("本我倾向≥24，解锁感受卡Rank3");
    }

    /// <summary>
    /// 超我滚动条值变化
    /// </summary>
    public void OnSuperegoScrollChanged(float scrollValue)
    {
        _superegoValue = Mathf.RoundToInt(scrollValue * maxTendencyValue);
        _superegoValue = Mathf.Clamp(_superegoValue, 0, maxTendencyValue);
        UpdateSuperegoText(_superegoValue);
        OnRankChanged?.Invoke(Card.CARD_TYPE.Dream, _superegoValue);
        if (_superegoValue >= 10) Debug.Log("超我倾向≥10，解锁理想卡Rank2");
        if (_superegoValue >= 24) Debug.Log("超我倾向≥24，解锁理想卡Rank3");
    }
    #endregion

    #region 3. 独立更新文本（极简）
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
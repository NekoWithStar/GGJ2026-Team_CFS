using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 面具/倾向控制（保留原文件名 Face_Move，最小改动对接 CardStatModifier）
public class Face_Move : MonoBehaviour
{
    [Header("自我（Reason）")]
    public Scrollbar scrollBar_Ego;
    public Text text_EgoValue;
    public List<Card> reasonCards = new List<Card>();

    [Header("本我（Feel）")]
    public Scrollbar scrollBar_Id;
    public Text text_IdValue;
    public List<Card> feelCards = new List<Card>();

    [Header("超我（Dream）")]
    public Scrollbar scrollBar_Superego;
    public Text text_SuperegoValue;
    public List<Card> dreamCards = new List<Card>();

    [Header("配置")]
    [Tooltip("最大倾向值，范围 0..maxTendencyValue（numberOfSteps 会被设置为 maxTendencyValue+1）")]
    public int maxTendencyValue = 30;

    [Tooltip("是否把滑动值写回 Card.cardRank")]
    public bool applyRankToCards = true;

    [Tooltip("是否同时根据 rank 自动设置 cost（newCost = costBase + rank * costPerRank）")]
    public bool updateCostWithRank = false;
    public int costBase = 0;
    public int costPerRank = 0;

    void Start()
    {
        ConfigureScrollbar(scrollBar_Ego);
        ConfigureScrollbar(scrollBar_Id);
        ConfigureScrollbar(scrollBar_Superego);

        BindScrollbars();
        InitializeUIFromCards();
    }

    void ConfigureScrollbar(Scrollbar sb)
    {
        if (sb == null) return;
        // Unity 要求 numberOfSteps > 1 才生效；我们希望包含 0，所以用 maxTendencyValue + 1
        sb.numberOfSteps = Mathf.Max(2, maxTendencyValue + 1);
    }

    void BindScrollbars()
    {
        if (scrollBar_Ego != null)
        {
            scrollBar_Ego.onValueChanged.RemoveAllListeners();
            scrollBar_Ego.onValueChanged.AddListener(OnEgoScrollChanged);
        }
        if (scrollBar_Id != null)
        {
            scrollBar_Id.onValueChanged.RemoveAllListeners();
            scrollBar_Id.onValueChanged.AddListener(OnIdScrollChanged);
        }
        if (scrollBar_Superego != null)
        {
            scrollBar_Superego.onValueChanged.RemoveAllListeners();
            scrollBar_Superego.onValueChanged.AddListener(OnSuperegoScrollChanged);
        }
    }

    void InitializeUIFromCards()
    {
        // 如果有绑定卡牌，以第一张卡牌的 rank 初始化 scrollbar 与文本；否则为0
        if (scrollBar_Ego != null)
        {
            int initRank = (reasonCards != null && reasonCards.Count > 0) ? reasonCards[0].cardRank : 0;
            SetScrollbarValueWithoutNotify(scrollBar_Ego, RankToScrollbar(initRank));
            UpdateEgoText(initRank);
        }

        if (scrollBar_Id != null)
        {
            int initRank = (feelCards != null && feelCards.Count > 0) ? feelCards[0].cardRank : 0;
            SetScrollbarValueWithoutNotify(scrollBar_Id, RankToScrollbar(initRank));
            UpdateIdText(initRank);
        }

        if (scrollBar_Superego != null)
        {
            int initRank = (dreamCards != null && dreamCards.Count > 0) ? dreamCards[0].cardRank : 0;
            SetScrollbarValueWithoutNotify(scrollBar_Superego, RankToScrollbar(initRank));
            UpdateSuperegoText(initRank);
        }
    }

    float RankToScrollbar(int rank)
    {
        if (maxTendencyValue <= 0) return 0f;
        return Mathf.Clamp01(rank / (float)maxTendencyValue);
    }

    // 设置 scrollbar 值但不触发回调（移除监听 -> 设置 -> 重新绑定）
    void SetScrollbarValueWithoutNotify(Scrollbar sb, float value)
    {
        if (sb == null) return;
        sb.onValueChanged.RemoveAllListeners();
        sb.value = value;
        BindScrollbars();
    }

    // 回调：将 0..1 映射为 0..maxTendencyValue 的离散整数
    int MapScrollbarToRank(float v) => Mathf.RoundToInt(v * maxTendencyValue);

    public void OnEgoScrollChanged(float scrollValue)
    {
        int mapped = MapScrollbarToRank(scrollValue);
        UpdateEgoText(mapped);

        if (applyRankToCards)
        {
            CardStatModifier.SetRankForList(reasonCards, mapped);
        }

        if (updateCostWithRank)
        {
            int newCost = costBase + mapped * costPerRank;
            CardStatModifier.SetCostForList(reasonCards, newCost);
        }
    }

    public void OnIdScrollChanged(float scrollValue)
    {
        int mapped = MapScrollbarToRank(scrollValue);
        UpdateIdText(mapped);

        if (applyRankToCards)
        {
            CardStatModifier.SetRankForList(feelCards, mapped);
        }

        if (updateCostWithRank)
        {
            int newCost = costBase + mapped * costPerRank;
            CardStatModifier.SetCostForList(feelCards, newCost);
        }
    }

    public void OnSuperegoScrollChanged(float scrollValue)
    {
        int mapped = MapScrollbarToRank(scrollValue);
        UpdateSuperegoText(mapped);

        if (applyRankToCards)
        {
            CardStatModifier.SetRankForList(dreamCards, mapped);
        }

        if (updateCostWithRank)
        {
            int newCost = costBase + mapped * costPerRank;
            CardStatModifier.SetCostForList(dreamCards, newCost);
        }
    }

    // 简洁文本更新
    void UpdateEgoText(int v) => text_EgoValue.text = $"Reason：{v}";
    void UpdateIdText(int v) => text_IdValue.text = $"Feel：{v}";
    void UpdateSuperegoText(int v) => text_SuperegoValue.text = $"Dream：{v}";
}
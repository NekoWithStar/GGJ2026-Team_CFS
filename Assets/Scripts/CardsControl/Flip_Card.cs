using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class Flip_Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // 当卡牌被“确认”时广播该卡牌的数据（Card ScriptableObject）
    public static event Action<Card> OnCardConfirmed;

    [Header("卡牌正反面 (Canvas UI 下的 GameObject)")]
    public GameObject frontFace; // 正面（包含 CardControl 等 UI 元素）
    public GameObject backFace; // 背面（默认显示）

    [Header("交互设置")]
    public float hoverScale = 1.08f; // 鼠标悬停放大倍率
    public float scaleSpeed = 10f; // 缩放速度（越大越快）
    public float flipDuration = 0.4f; // 翻转总时长（秒）

    [Header("确认设置")]
    [Tooltip("如果为 true，则当卡牌正面朝上时再次点击视为确认：不会把卡牌翻回去。")]
    public bool secondClickIsConfirm = false;
    [Tooltip("当 secondClickIsConfirm 为 true 且用户在正面再次点击时触发的事件（可在 Inspector 中绑定）。")]
    public UnityEvent onConfirm;

    private bool isFaceDown = true; // 默认背面朝上
    private bool isAnimating = false;
    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    private void Start()
    {
        originalScale = transform.localScale;
        // 初始状态：背面可见，正面隐藏
        if (frontFace != null) frontFace.SetActive(!isFaceDown);
        if (backFace != null) backFace.SetActive(isFaceDown);
        // 保证初始旋转为 0 或 180，避免累积旋转问题
        transform.localEulerAngles = new Vector3(0f, isFaceDown ? 0f : 180f, 0f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(originalScale * hoverScale));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(originalScale));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isAnimating) return;

        // 如果启用了“再次点击为确认”且当前为正面朝上，则把再次点击视为确认而不是翻回去
        if (!isFaceDown && secondClickIsConfirm)
        {
            // 先触发 inspector 绑定的 UnityEvent
            onConfirm?.Invoke();

            // 查找 CardControl（通常在正面的子对象上），并广播被确认的 Card（如果存在）
            CardControl cc = GetComponentInChildren<CardControl>();
            if (cc != null && cc.card_data != null)
            {
                OnCardConfirmed?.Invoke(cc.card_data);
            }

            return;
        }

        StartCoroutine(FlipCoroutine());
    }

    private IEnumerator ScaleTo(Vector3 target)
    {
        Vector3 start = transform.localScale;
        float t = 0f;
        float duration = 1f / Mathf.Max(0.0001f, scaleSpeed);
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            transform.localScale = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        transform.localScale = target;
        scaleCoroutine = null;
    }

    private IEnumerator FlipCoroutine()
    {
        isAnimating = true;

        // 统一起始角度（避免累计）
        float startAngle = isFaceDown ? 0f : 180f;
        float endAngle = startAngle + 180f;

        float elapsed = 0f;
        bool swapped = false;

        // 在翻转过程中禁用交互（可根据需要添加 CanvasGroup 禁用射线）
        while (elapsed < flipDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float frac = Mathf.Clamp01(elapsed / flipDuration);
            float angle = Mathf.Lerp(startAngle, endAngle, frac);
            transform.localEulerAngles = new Vector3(0f, angle, 0f);

            // 翻转到中点（90度偏移）时交换正反面显示
            if (!swapped && Mathf.Abs(Mathf.DeltaAngle(startAngle, angle)) >= 90f)
            {
                SwapFaces();
                swapped = true;
            }

            yield return null;
        }

        // 确保结束角度规范到 0..360
        float finalY = endAngle % 360f;
        transform.localEulerAngles = new Vector3(0f, finalY, 0f);

        // 翻面状态取反
        isFaceDown = !isFaceDown;
        isAnimating = false;
    }

    private void SwapFaces()
    {
        if (frontFace != null) frontFace.SetActive(!frontFace.activeSelf);
        if (backFace != null) backFace.SetActive(!backFace.activeSelf);
    }
}

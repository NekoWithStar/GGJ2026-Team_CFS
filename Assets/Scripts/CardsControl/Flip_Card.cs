using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// 卡牌翻转交互（兼容 CardControl 与 WeaponCardControl）
/// - 鼠标悬停放大、点击翻面、支持 secondClickIsConfirm 触发确认事件
/// - 当确认时会广播对应类型的静态事件：OnCardConfirmed / OnWeaponConfirmed
/// </summary>
public class Flip_Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // 当卡牌被“确认”时广播该卡牌的数据（Card ScriptableObject）
    public static event Action<Card> OnCardConfirmed;
    // 当武器被“确认”时广播该武器的数据（Weapon ScriptableObject）
    public static event Action<Weapon> OnWeaponConfirmed;    // 当属性卡被"确认"时广播该属性卡的数据（PropertyCard ScriptableObject）
    public static event Action<Y_Survivor.PropertyCard> OnPropertyCardConfirmed;
    [Header("卡牌正反面 (Canvas UI 下的 GameObject)")]
    public GameObject frontFace; // 正面（包含 CardControl / WeaponCardControl 等 UI 元素）
    public GameObject backFace; // 背面（默认显示）
    [Tooltip("如果启用，将把当前 Inspector 上的 front/back 引用互换（把原来的背面视为新的正面）")]
    public bool swapFrontBackDefinition = true;

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
    // 供外部查询当前卡牌是否为正面朝上（用于全局点击代理等）
    public bool IsFaceUp => !isFaceDown;
    private bool isAnimating = false;
    private Vector3 originalScale;
    private Coroutine scaleCoroutine;
    private Coroutine faceCameraCoroutine;
    private bool isConfirmed = false;

    // 标记：当对象被禁用后再次启用时是否应重置为背面
    private bool _resetToBackOnEnable = true;

    private void Awake()
    {
        // 提前保存初始缩放，避免 OnEnable 在 Start 之前访问到未初始化的 originalScale
        originalScale = transform.localScale;
        // 如果需要，在启动时互换 front/back 的引用（不需要在 Inspector 中手动修改）
        if (swapFrontBackDefinition)
        {
            var tmp = frontFace;
            frontFace = backFace;
            backFace = tmp;
        }
    }

    private void OnEnable()
    {
        // 如果之前被禁用过（视为“关闭”），则在重新启用时重置为背面状态
        if (_resetToBackOnEnable)
        {
            ResetToBack();
            _resetToBackOnEnable = false; // 重置标记，避免重复重置
        }
    }

    private void OnDisable()
    {
        // 记录为“已关闭”状态，等待下次启用时重置为背面
        _resetToBackOnEnable = true;
    }

    private void Start()
    {
        // 初始状态：背面可见，正面隐藏
        if (frontFace != null) frontFace.SetActive(!isFaceDown);
        if (backFace != null) backFace.SetActive(isFaceDown);
        // 保证初始旋转为 0 或 180，避免累积旋转问题
        transform.localEulerAngles = new Vector3(0f, isFaceDown ? 0f : 180f, 0f);
    }

    /// <summary>
    /// 将卡牌强制重置为背面状态（停止动画、复位旋转、显示背面、还原缩放）
    /// </summary>
    public void ResetToBack()
    {
        // 停止缩放协程
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }

        // 停止翻转动画状态
        isAnimating = false;

        // 强制背面朝上
        isFaceDown = true;

        // 复位旋转与缩放 — 面向摄像机（保持 Y 角度为 0）
        transform.localScale = originalScale;
        // 将卡牌旋转为正面/背面朝向屏幕（清除 X,Z 旋转），并将 Y 设为 0
        transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        transform.localScale = originalScale;

        // 显示/隐藏正反面
        if (frontFace != null) frontFace.SetActive(false);
        if (backFace != null) backFace.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isConfirmed) return; // 已确认的卡牌不再响应悬停翻面/缩放
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(originalScale * hoverScale));
        // 开始面向摄像机（持续保持 X/Z 角度为 0，保留 Y 旋转以支持翻转动画）
        if (faceCameraCoroutine != null) StopCoroutine(faceCameraCoroutine);
        faceCameraCoroutine = StartCoroutine(FaceCameraCoroutine());
        // 鼠标进入时若背面朝上则翻到正面
        if (!isConfirmed && isFaceDown && !isAnimating)
        {
            StartCoroutine(FlipCoroutine());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isConfirmed) return; // 已确认的不翻回
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(originalScale));
        if (faceCameraCoroutine != null)
        {
            StopCoroutine(faceCameraCoroutine);
            faceCameraCoroutine = null;
        }
        // 鼠标离开时如果正面朝上且未启用 secondClickIsConfirm，则翻回背面
        if (!isConfirmed && !isFaceDown && !isAnimating && !secondClickIsConfirm)
        {
            StartCoroutine(FlipCoroutine());
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[Flip_Card] 🖱️ 卡牌被点击 - isFaceDown: {isFaceDown}, secondClickIsConfirm: {secondClickIsConfirm}, isAnimating: {isAnimating}");

        if (isAnimating) return;

        // 如果当前为正面朝上，且处于卡牌选择界面，则把点击视为确认（平替方案）
        if (!isFaceDown)
        {
            bool selectionPanelOpen = false;
            var csm = FindAnyObjectByType<CardSelectionManager>();
            if (csm != null && csm.cardSelectionPanel != null)
            {
                selectionPanelOpen = csm.cardSelectionPanel.activeInHierarchy;
            }

            if (secondClickIsConfirm || selectionPanelOpen)
            {
                Debug.Log($"[Flip_Card] ✅ 触发确认事件 (secondClickIsConfirm={secondClickIsConfirm}, selectionPanelOpen={selectionPanelOpen})");
                Confirm();
                return;
            }
        }

        // 如果是背面朝上，开始翻转到正面
        if (isFaceDown)
        {
            Debug.Log($"[Flip_Card] 🔄 从背面翻转到正面");
            StartCoroutine(FlipCoroutine());
        }
        else
        {
            Debug.Log($"[Flip_Card] 🔄 从正面翻转回背面");
            StartCoroutine(FlipCoroutine());
        }
    }

    /// <summary>
    /// 公开方法：确认当前卡片（供按钮等UI元素调用）
    /// </summary>
    public void Confirm()
    {
        Debug.Log("[Flip_Card] 🎯 Confirm() 方法被调用");

        // 停止交互相关协程（但延后设置 isConfirmed，直到找到并广播确认）
        if (faceCameraCoroutine != null)
        {
            StopCoroutine(faceCameraCoroutine);
            faceCameraCoroutine = null;
        }
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }

        onConfirm?.Invoke();

        // 为兼容可能被 swap 的 front/back 引用，优先在 frontFace 下查找控件，再在 backFace 下查找，最后回退到全局查找
        CardControl cc = null;
        WeaponCardControl wc = null;
        PropertyCardControl pcc = null;

        Debug.Log($"[Flip_Card] 🔍 开始搜索控件...\n  frontFace: {(frontFace != null ? frontFace.name : "NULL")}\n  backFace: {(backFace != null ? backFace.name : "NULL")}");

        if (frontFace != null)
        {
            cc = frontFace.GetComponentInChildren<CardControl>();
            wc = frontFace.GetComponentInChildren<WeaponCardControl>();
            pcc = frontFace.GetComponentInChildren<PropertyCardControl>();
            Debug.Log($"[Flip_Card] 📦 frontFace 搜索结果：CC={cc != null}, WC={wc != null}, PCC={pcc != null}");
        }

        if ((cc == null && wc == null && pcc == null) && backFace != null)
        {
            cc = backFace.GetComponentInChildren<CardControl>();
            wc = backFace.GetComponentInChildren<WeaponCardControl>();
            pcc = backFace.GetComponentInChildren<PropertyCardControl>();
            Debug.Log($"[Flip_Card] 📦 backFace 搜索结果：CC={cc != null}, WC={wc != null}, PCC={pcc != null}");
        }

        if (cc == null && wc == null && pcc == null)
        {
            cc = GetComponentInChildren<CardControl>();
            wc = GetComponentInChildren<WeaponCardControl>();
            pcc = GetComponentInChildren<PropertyCardControl>();
            Debug.Log($"[Flip_Card] 📦 全局搜索结果：CC={cc != null}, WC={wc != null}, PCC={pcc != null}");
        }

        if (cc != null && cc.card_data != null)
        {
            OnCardConfirmed?.Invoke(cc.card_data);
            isConfirmed = true;
            Debug.Log($"[Flip_Card] ✅ 确认普通卡片: {cc.card_data.cardName}");
            return;
        }

        if (wc != null && wc.weapon_data != null)
        {
            OnWeaponConfirmed?.Invoke(wc.weapon_data);
            isConfirmed = true;
            Debug.Log($"[Flip_Card] ✅ 确认武器卡片: {wc.weapon_data.weaponName}");
            return;
        }

        if (pcc != null && pcc.propertyCard != null)
        {
            OnPropertyCardConfirmed?.Invoke(pcc.propertyCard);
            isConfirmed = true;
            Debug.Log($"[Flip_Card] ✅ 确认属性卡片: {pcc.propertyCard.cardName}");
            return;
        }

        // 详细诊断：列出找到的控件但数据为 null
        if (cc != null)
            Debug.LogWarning("[Flip_Card] ⚠️ 找到 CardControl 但 card_data 为 NULL");
        if (wc != null)
            Debug.LogWarning("[Flip_Card] ⚠️ 找到 WeaponCardControl 但 weapon_data 为 NULL");
        if (pcc != null)
            Debug.LogWarning("[Flip_Card] ⚠️ 找到 PropertyCardControl 但 propertyCard 为 NULL");

        Debug.LogWarning("[Flip_Card] ⚠️ Confirm() 被调用，但未找到有效的 CardControl、WeaponCardControl 或 PropertyCardControl");
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

    private IEnumerator FaceCameraCoroutine()
    {
        // 保持卡牌面向摄像机（只锁定 X/Z 旋转，保留 Y 以配合翻转）
        while (true)
        {
            // 获取当前 Y 角度
            float y = transform.localEulerAngles.y;
            transform.localEulerAngles = new Vector3(0f, y, 0f);
            yield return null;
        }
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

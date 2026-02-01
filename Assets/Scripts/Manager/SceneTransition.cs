using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景切换淡入/淡出管理器
/// 自动创建一个覆盖全屏的 Canvas+Image 用于淡入淡出效果，且在场景间保持单例
/// 使用： SceneTransition.Instance.TransitionToScene("SceneName", 0.6f);
/// </summary>
public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Header("Transition Settings")]
    public Color fadeColor = Color.black;
    public float defaultFadeDuration = 0.5f;

    private Canvas transitionCanvas;
    private CanvasGroup canvasGroup;
    private Image fadeImage;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetupCanvas();
    }

    private void SetupCanvas()
    {
        // 如果已经有设置过就返回
        if (transitionCanvas != null) return;

        GameObject go = new GameObject("SceneTransitionCanvas");
        go.transform.SetParent(transform, false);

        transitionCanvas = go.AddComponent<Canvas>();
        transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        transitionCanvas.sortingOrder = 1000;

        canvasGroup = go.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 0f;

        // Image
        GameObject imgGO = new GameObject("FadeImage");
        imgGO.transform.SetParent(go.transform, false);
        fadeImage = imgGO.AddComponent<Image>();
        fadeImage.color = fadeColor;

        RectTransform rt = imgGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// 开始带淡入淡出的场景切换
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="fadeDuration"></param>
    public void TransitionToScene(string sceneName, float fadeDuration = -1f)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("[SceneTransition] 已在过渡中，忽略新的请求");
            return;
        }

        if (fadeDuration <= 0f) fadeDuration = defaultFadeDuration;
        StartCoroutine(DoTransition(sceneName, fadeDuration));
    }

    private IEnumerator DoTransition(string sceneName, float fadeDuration)
    {
        isTransitioning = true;

        // Fade out
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        // 开始异步加载场景（使用完全限定名以避免与自定义 SceneManager 冲突）
        AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // 等待加载完成到90%
        while (op.progress < 0.9f)
        {
            yield return null;
        }

        // 激活场景
        op.allowSceneActivation = true;
        while (!op.isDone)
        {
            yield return null;
        }

        // 给一帧时间让新场景初始化
        yield return null;

        // Fade in
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

        isTransitioning = false;
    }

    /// <summary>
    /// 取消当前过渡并销毁该单例（用于在强制重载场景时清理可能阻塞的异步加载）
    /// </summary>
    public void CancelAndDestroy()
    {
        if (isTransitioning)
        {
            StopAllCoroutines();
        }
        isTransitioning = false;

        // 尝试销毁过渡画布
        if (transitionCanvas != null)
        {
            Destroy(transitionCanvas.gameObject);
            transitionCanvas = null;
        }

        // 最后销毁自身对象
        Destroy(gameObject);
        Instance = null;
        Debug.Log("[SceneTransition] CancelAndDestroy called - transition aborted and instance destroyed");
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        canvasGroup.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}

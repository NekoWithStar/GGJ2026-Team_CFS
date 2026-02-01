using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

    [Header("Debug Shortcuts")]
    [Tooltip("按 R 键时等待多少秒再重载当前场景（0 = 立即重载）")]
    public float reloadDelay = 0f;

    [Tooltip("当延迟重载时，是否在等待期间暂停游戏（通过 Time.timeScale=0）。等待使用实时秒数，不受 timeScale 影响。）")]
    public bool pauseDuringReload = true;

    private bool isReloading = false;

    private void Update()
    {
        // 按 R 键重载当前场景（方便开发调试）
        if (Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            if (reloadDelay > 0f)
            {
                StartCoroutine(ReloadAfterDelay(reloadDelay));
            }
            else
            {
                ReloadCurrentScene();
            }
        }
    }

    private IEnumerator ReloadAfterDelay(float delaySeconds)
    {
        isReloading = true;

        if (pauseDuringReload)
        {
            Time.timeScale = 0f;
        }

        yield return new WaitForSecondsRealtime(delaySeconds);

        if (pauseDuringReload)
        {
            Time.timeScale = 1f;
        }

        ReloadCurrentScene();
        isReloading = false;
    }

    /// <summary>
    /// 切换到指定场景
    /// </summary>
    /// <param name="sceneName">目标场景名称</param>
    public void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            // 优先使用 SceneTransition 进行带淡入/淡出效果的切换
            if (SceneTransition.Instance != null)
            {
                SceneTransition.Instance.TransitionToScene(sceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
            Debug.Log($"[SceneManager] 切换到场景: {sceneName}");
        }
        else
        {
            Debug.LogWarning("[SceneManager] 场景名称不能为空");
        }
    }

    /// <summary>
    /// 重载当前场景
    /// </summary>
    public void ReloadCurrentScene()
    {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        // 在重载之前，取消任何可能正在进行的 SceneTransition（避免异步加载卡住）
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.CancelAndDestroy();
        }

        // 确保恢复游戏时间（防止暂停状态保留）
        Time.timeScale = 1f;

        // 直接同步加载当前场景，避免使用异步过渡时发生卡住（某些情况下 LoadSceneAsync 的 allowSceneActivation=false
        // 会导致 progress 无法推进到 0.9，从而卡住重载流程）。
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
        Debug.Log($"[SceneManager] 重载当前场景（绕过过渡，已取消过渡）: {currentSceneName}");
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[SceneManager] 退出游戏");
        Application.Quit();
        
        // 在编辑器中停止播放
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}

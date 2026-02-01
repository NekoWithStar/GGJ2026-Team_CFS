using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

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
        // 直接同步加载当前场景，避免使用异步过渡时发生卡住（某些情况下 LoadSceneAsync 的 allowSceneActivation=false
        // 会导致 progress 无法推进到 0.9，从而卡住重载流程）。
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
        Debug.Log($"[SceneManager] 重载当前场景（绕过过渡）: {currentSceneName}");
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

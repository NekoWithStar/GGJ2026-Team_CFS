using System.Collections;
using UnityEngine;

public class Eat_Corn : MonoBehaviour
{
    [Header("碰撞处理设置")]
    [Tooltip("被碰撞并需要关闭/销毁的目标 Tag")]
    public string targetTag = "Corn";
    [Tooltip("是否在碰撞时将目标对象 SetActive(false)")]
    public bool deactivateOnCollision = true;
    [Tooltip("是否在碰撞时 Destroy 目标对象（优先于 deactivateOnCollision）")]
    public bool destroyOnCollision = false;
    [Tooltip("开启调试日志")]
    public bool debugLogs = false;

    void OnValidate()
    {
        // 保持设置一致：如果两者都勾选，优先 Destroy
        if (deactivateOnCollision && destroyOnCollision)
        {
            deactivateOnCollision = false;
            if (debugLogs) Debug.Log("[Eat_Corn] destroyOnCollision 启用，deactivateOnCollision 已自动关闭。", this);
        }
    }



    // 2D 触发（IsTrigger）
    void OnTriggerEnter2D(Collider2D other)
    {
        TryHandleCollision(other.gameObject);
    }

    void TryHandleCollision(GameObject other)
    {
        if (other == null) return;
        if (string.IsNullOrEmpty(targetTag)) return;

        if (!other.CompareTag(targetTag)) return;

        if (debugLogs) Debug.Log($"[Eat_Corn] 碰撞到目标 Tag={targetTag}，处理对象={other.name}", this);

        if (destroyOnCollision)
        {
            Destroy(other);
            if (debugLogs) Debug.Log($"[Eat_Corn] 已销毁对象 {other.name}", this);
            return;
        }

        if (deactivateOnCollision)
        {
            other.SetActive(false);
            if (debugLogs) Debug.Log($"[Eat_Corn] 已禁用对象 {other.name}", this);
        }
    }
}

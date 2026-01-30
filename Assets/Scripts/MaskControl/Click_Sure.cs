using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Click_Sure : MonoBehaviour
{
    public enum CloseAction
    {
        SetInactive,
        Destroy
    }

    [Tooltip("选择关闭卡牌的方式：隐藏还是销毁。")]
    public CloseAction closeAction = CloseAction.SetInactive;

    [Tooltip("是否包含未激活（inactive）的卡牌。")]
    public bool includeInactive = true;

    void OnEnable()
    {
        Flip_Card.OnCardConfirmed += OnCardConfirmed;
    }

    void OnDisable()
    {
        Flip_Card.OnCardConfirmed -= OnCardConfirmed;
    }

    /// <summary>
    /// 当有卡牌被确认选择后，关闭（或销毁）当前场景中所有 Flip_Card 对应的 GameObject。
    /// 使用 Resources.FindObjectsOfTypeAll 来同时查找 inactive 对象，并根据 scene.isLoaded 过滤场景内对象，
    /// 避免影响项目内的字体/资源资产或 prefab 资产。
    /// </summary>
    void OnCardConfirmed(Card confirmedCard)
    {
        try
        {
            var all = Resources.FindObjectsOfTypeAll<Flip_Card>();
            int closedCount = 0;

            foreach (var f in all)
            {
                if (f == null || f.gameObject == null) continue;

                // 只处理已存在于某个已加载场景中的实例（排除资产 / 未入场景的 prefab）
                if (!f.gameObject.scene.isLoaded) continue;

                // 如果不想处理 inactive 的对象，则跳过
                if (!includeInactive && !f.gameObject.activeInHierarchy) continue;

                if (closeAction == CloseAction.SetInactive)
                {
                    f.gameObject.SetActive(false);
                }
                else
                {
                    UnityEngine.Object.Destroy(f.gameObject);
                }

                closedCount++;
            }

            Debug.Log($"Click_Sure: confirmed '{(confirmedCard != null ? confirmedCard.cardName : "null")}', closed {closedCount} Flip_Card(s).");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Click_Sure: error while closing cards: {ex}");
        }
    }
}

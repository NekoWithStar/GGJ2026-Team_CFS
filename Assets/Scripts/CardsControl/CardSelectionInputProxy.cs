using UnityEngine;

/// <summary>
/// 全局输入代理：当卡牌选择面板打开时，任意屏幕左键点击将确认第一个处于正面朝上的Flip_Card。
/// 用于场景层（非按钮事件）点击无法传递到卡牌时的平替方案。
/// </summary>
public class CardSelectionInputProxy : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var csm = FindAnyObjectByType<CardSelectionManager>();
            if (csm == null) return;
            if (csm.cardSelectionPanel == null) return;
            if (!csm.cardSelectionPanel.activeInHierarchy) return;

            // 查找所有 Flip_Card，优先确认第一个处于正面朝上的卡牌
            var all = Resources.FindObjectsOfTypeAll<Flip_Card>();
            foreach (var fc in all)
            {
                if (fc == null) continue;
                if (!fc.gameObject.activeInHierarchy) continue;
                if (fc.IsFaceUp)
                {
                    Debug.Log($"[CardSelectionInputProxy] 全局点击确认 Flip_Card: {fc.gameObject.name}");
                    fc.Confirm();
                    // 只确认第一个
                    break;
                }
            }
        }
    }
}

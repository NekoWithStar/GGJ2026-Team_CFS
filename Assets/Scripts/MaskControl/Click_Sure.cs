using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Click_Sure : MonoBehaviour
{
    void OnEnable()
    {
        Flip_Card.OnCardConfirmed += OnCardConfirmed;
    }

    void OnDisable()
    {
        Flip_Card.OnCardConfirmed -= OnCardConfirmed;
    }

    // 当有卡牌被确认选择后，关闭（隐藏）当前场景中所有卡牌（含 Flip_Card 所在的 GameObject）
    void OnCardConfirmed(Card confirmedCard)
    {
        // 查找所有 Flip_Card 并关闭其根 GameObject（保守做法：只 SetActive(false)）
        Flip_Card[] all = UnityEngine.Object.FindObjectsByType<Flip_Card>(FindObjectsSortMode.None);
        foreach (var f in all)
        {
            if (f == null) continue;
            // 关闭整个卡牌对象，避免残留交互
            f.gameObject.SetActive(false);
        }

        Debug.Log($"Click_Sure: confirmed '{(confirmedCard != null ? confirmedCard.cardName : "null")}', closed {all.Length} cards.");
    }
}

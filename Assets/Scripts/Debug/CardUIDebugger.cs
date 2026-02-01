using UnityEngine;
using Y_Survivor;

/// <summary>
/// 诊断卡牌UI结构和Flip_Card配置
/// 在Console调用 CardUIDebugger.DiagnoseAllFlipCards() 查看当前场景的所有Flip_Card
/// </summary>
public class CardUIDebugger : MonoBehaviour
{
    [ContextMenu("诊断所有Flip_Card")]
    public void DiagnoseAllFlipCards()
    {
        var allFlipCards = Resources.FindObjectsOfTypeAll<Flip_Card>();
        Debug.Log($"[CardUIDebugger] 🔍 找到 {allFlipCards.Length} 个 Flip_Card");
        
        for (int i = 0; i < allFlipCards.Length; i++)
        {
            DiagnoseFlipCard(allFlipCards[i], i);
        }
    }

    public static void DiagnoseFlipCard(Flip_Card flipCard, int index = 0)
    {
        if (flipCard == null)
        {
            Debug.LogWarning("[CardUIDebugger] ❌ Flip_Card 为 null");
            return;
        }

        Debug.Log($"\n========== 诊断 Flip_Card #{index} ==========");
        Debug.Log($"GameObject: {flipCard.gameObject.name}");
        Debug.Log($"活跃: {flipCard.gameObject.activeSelf} / {flipCard.gameObject.activeInHierarchy}");
        Debug.Log($"secondClickIsConfirm: {flipCard.secondClickIsConfirm}");

        // 检查 frontFace 和 backFace
        var frontFace = flipCard.GetType().GetField("frontFace", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(flipCard) as GameObject;
        var backFace = flipCard.GetType().GetField("backFace", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(flipCard) as GameObject;

        Debug.Log($"frontFace: {(frontFace != null ? frontFace.name : "❌ NULL")}");
        Debug.Log($"backFace: {(backFace != null ? backFace.name : "❌ NULL")}");

        // 检查控件及其数据
        if (frontFace != null)
        {
            DebugCardControlsInGameObject(frontFace, "frontFace");
        }
        if (backFace != null)
        {
            DebugCardControlsInGameObject(backFace, "backFace");
        }

        // 全局查找
        Debug.Log($"\n全局查找（Flip_Card 下的所有子对象）:");
        DebugCardControlsInGameObject(flipCard.gameObject, "Flip_Card整体");
        
        Debug.Log($"========== 诊断结束 ==========\n");
    }

    private static void DebugCardControlsInGameObject(GameObject root, string location)
    {
        var cc = root.GetComponentInChildren<CardControl>();
        var wc = root.GetComponentInChildren<WeaponCardControl>();
        var pcc = root.GetComponentInChildren<PropertyCardControl>();

        Debug.Log($"  [{location}] CardControl: {(cc != null ? $"✅ {cc.gameObject.name}" : "❌ NULL")}");
        if (cc != null && cc.card_data != null)
        {
            Debug.Log($"    -> card_data: ✅ {cc.card_data.cardName}");
        }
        else if (cc != null)
        {
            Debug.Log($"    -> card_data: ❌ NULL");
        }

        Debug.Log($"  [{location}] WeaponCardControl: {(wc != null ? $"✅ {wc.gameObject.name}" : "❌ NULL")}");
        if (wc != null && wc.weapon_data != null)
        {
            Debug.Log($"    -> weapon_data: ✅ {wc.weapon_data.weaponName}");
        }
        else if (wc != null)
        {
            Debug.Log($"    -> weapon_data: ❌ NULL");
        }

        Debug.Log($"  [{location}] PropertyCardControl: {(pcc != null ? $"✅ {pcc.gameObject.name}" : "❌ NULL")}");
        if (pcc != null && pcc.propertyCard != null)
        {
            Debug.Log($"    -> propertyCard: ✅ {pcc.propertyCard.cardName}");
        }
        else if (pcc != null)
        {
            Debug.Log($"    -> propertyCard: ❌ NULL");
        }
    }
}

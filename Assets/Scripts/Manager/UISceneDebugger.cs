using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// UI 场景结构诊断工具 - 帮助定位卡牌选择UI的具体位置
/// 按 U 键打印所有 Canvas 和它们的子对象
/// </summary>
public class UISceneDebugger : MonoBehaviour
{
    private void Update()
    {
        // 按 U 键打印所有 UI 元素
        if (Input.GetKeyDown(KeyCode.U))
        {
            PrintAllCanvases();
        }
    }

    private void PrintAllCanvases()
    {
        Debug.Log("[UISceneDebugger] ========== 场景中的所有Canvas ==========");
        
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Debug.Log($"找到 {canvases.Length} 个 Canvas");

        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            Debug.Log($"\n[Canvas {i}] {canvas.gameObject.name}");
            Debug.Log($"  - 激活状态: {canvas.gameObject.activeSelf}");
            Debug.Log($"  - 根对象激活: {canvas.gameObject.activeInHierarchy}");
            Debug.Log($"  - 渲染模式: {canvas.renderMode}");
            Debug.Log($"  - 排序顺序: {canvas.sortingOrder}");
            Debug.Log($"  - 子对象数: {canvas.gameObject.transform.childCount}");

            PrintChildObjects(canvas.gameObject.transform, 1);
        }

        // 也打印所有没有被激活的 Canvas
        Debug.Log("\n========== 未激活的Canvas和UI元素 ==========");
        PrintInactiveUIElements();

        Debug.Log("\n========== 查找特定UI元素 ==========");
        FindSpecificUIElements();
    }

    private void PrintChildObjects(Transform parent, int depth)
    {
        string indent = new string(' ', depth * 2);
        
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            string status = child.gameObject.activeSelf ? "✅" : "❌";
            Debug.Log($"{indent}├─ [{status}] {child.gameObject.name}");
            
            // 如果这个对象看起来像卡牌相关的，打印更多信息
            if (child.gameObject.name.Contains("Card") || child.gameObject.name.Contains("Selection"))
            {
                Debug.Log($"{indent}   └─ 🎯  潜在的卡牌相关对象！");
                RectTransform rect = child.GetComponent<RectTransform>();
                if (rect != null)
                {
                    Debug.Log($"{indent}   └─ 大小: {rect.rect.width}x{rect.rect.height}");
                }
            }
            
            // 递归打印子对象
            if (child.childCount > 0 && depth < 4)
            {
                PrintChildObjects(child, depth + 1);
            }
        }
    }

    private void PrintInactiveUIElements()
    {
        GraphicRaycaster[] raycasters = FindObjectsByType<GraphicRaycaster>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"找到 {raycasters.Length} 个 GraphicRaycaster (包括未激活的)");

        foreach (GraphicRaycaster raycaster in raycasters)
        {
            string status = raycaster.gameObject.activeSelf ? "✅ 活动" : "❌ 未激活";
            Debug.Log($"  - {status}: {raycaster.gameObject.name}");
        }
    }

    private void FindSpecificUIElements()
    {
        // 查找所有包含特定关键字的对象
        string[] keywords = { "Card", "Selection", "Panel", "Upgrade", "Choice" };
        
        foreach (string keyword in keywords)
        {
            Transform[] allTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            List<Transform> matches = new List<Transform>();
            
            foreach (Transform t in allTransforms)
            {
                if (t.gameObject.name.Contains(keyword))
                {
                    matches.Add(t);
                }
            }

            if (matches.Count > 0)
            {
                Debug.Log($"\n🔍 包含 '{keyword}' 的对象:");
                foreach (Transform match in matches)
                {
                    string status = match.gameObject.activeSelf ? "✅" : "❌";
                    string path = GetGameObjectPath(match.gameObject);
                    Debug.Log($"  {status} {path}");
                }
            }
        }
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        
        while (parent != null)
        {
            path = parent.gameObject.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }

    private void Start()
    {
        Debug.Log("[UISceneDebugger] 📢 按 U 键打印场景中所有UI元素的结构");
    }
}

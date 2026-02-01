using UnityEngine;

/// <summary>
/// CardSelectionPanel 位置诊断工具
/// 按 C 键打印 CardSelectionPanel 的完整信息
/// </summary>
public class CardSelectionPanelFinder : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            FindCardSelectionPanel();
        }
    }

    private void FindCardSelectionPanel()
    {
        Debug.Log("[CardSelectionPanelFinder] ========== 搜索 CardSelectionPanel ==========");
        
        // 方法 1: 直接查找
        GameObject panel = GameObject.Find("CardSelectionPanel");
        if (panel != null)
        {
            PrintGameObjectInfo("通过 GameObject.Find('CardSelectionPanel') 找到", panel);
            return;
        }

        Debug.Log("[CardSelectionPanelFinder] ⚠️ 通过 GameObject.Find 未找到，继续搜索...");

        // 方法 2: 查找所有包含"Card"的对象
        Transform[] allTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform t in allTransforms)
        {
            if (t.gameObject.name.Contains("Card") && t.gameObject.name.Contains("Selection"))
            {
                PrintGameObjectInfo($"通过关键字搜索找到", t.gameObject);
                return;
            }
        }

        Debug.Log("[CardSelectionPanelFinder] ⚠️ 仍未找到包含'Card'和'Selection'的对象");

        // 方法 3: 打印所有包含"Card"的对象
        Debug.Log("[CardSelectionPanelFinder] 📋 打印所有包含'Card'的对象:");
        foreach (Transform t in allTransforms)
        {
            if (t.gameObject.name.Contains("Card"))
            {
                string status = t.gameObject.activeSelf ? "✅" : "❌";
                string path = GetGameObjectPath(t.gameObject);
                Debug.Log($"  {status} {path}");
            }
        }

        Debug.Log("[CardSelectionPanelFinder] 📋 打印所有包含'Selection'的对象:");
        foreach (Transform t in allTransforms)
        {
            if (t.gameObject.name.Contains("Selection"))
            {
                string status = t.gameObject.activeSelf ? "✅" : "❌";
                string path = GetGameObjectPath(t.gameObject);
                Debug.Log($"  {status} {path}");
            }
        }

        // 方法 4: 打印所有 Canvas 及其子对象
        Debug.Log("[CardSelectionPanelFinder] 📋 所有 Canvas:");
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            string status = canvas.gameObject.activeSelf ? "✅" : "❌";
            Debug.Log($"  {status} Canvas: {canvas.gameObject.name}");
            PrintChildObjects(canvas.gameObject.transform, 2);
        }
    }

    private void PrintGameObjectInfo(string source, GameObject obj)
    {
        Debug.Log($"[CardSelectionPanelFinder] ✅ {source}");
        Debug.Log($"  - 路径: {GetGameObjectPath(obj)}");
        Debug.Log($"  - 激活状态: {obj.activeSelf}");
        Debug.Log($"  - 根对象激活: {obj.activeInHierarchy}");
        Debug.Log($"  - 子对象数: {obj.transform.childCount}");
        
        Canvas canvas = obj.GetComponent<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"  - Canvas 渲染模式: {canvas.renderMode}");
            Debug.Log($"  - Canvas 排序顺序: {canvas.sortingOrder}");
        }

        // 打印子对象
        if (obj.transform.childCount > 0)
        {
            Debug.Log("  - 子对象列表:");
            PrintChildObjects(obj.transform, 2);
        }
    }

    private void PrintChildObjects(Transform parent, int depth)
    {
        string indent = new string(' ', depth * 2);
        
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            string status = child.gameObject.activeSelf ? "✅" : "❌";
            Debug.Log($"{indent}├─ {status} {child.gameObject.name}");
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
        Debug.Log("[CardSelectionPanelFinder] 📢 按 C 键搜索 CardSelectionPanel");
    }
}

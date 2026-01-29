using UnityEngine;

/// <summary>
/// 2D拾取脚本：金币/血包，玩家触碰即拾取
/// </summary>
public class PickupControl : MonoBehaviour
{
    [Header("拾取配置")]
    public string pickupType = "Coin"; // 拾取类型：Coin/Hp
    public int pickupValue = 5;        // 拾取数值
    private SpriteRenderer sr;
    private Collider2D col;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    // 2D触发器检测：玩家触碰即拾取
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 调用玩家拾取方法
            other.GetComponent<PlayerControl>().PickupItem(pickupType, pickupValue);
            // 隐藏并禁用碰撞（避免重复拾取）
            sr.enabled = false;
            col.enabled = false;
            // 延迟销毁
            Destroy(gameObject, 0.1f);
        }
    }
}
using UnityEngine;

/// <summary>
/// 2D拾取脚本：金币/血包，玩家触碰即拾取
/// </summary>
public class PickupControl : MonoBehaviour
{
    [Header("拾取配置")]
    public string pickupType = "Coin"; // 拾取类型：Coin/Hp
    public int pickupValue = 5;        // 拾取数值
    
    [Header("自动销毁配置")]
    [Tooltip("金币最长存在时间（秒）")]
    public float maxLifetime = 6f;
    
    [Tooltip("距离玩家超过此范围时自动销毁（单位：米）")]
    public float maxDistanceFromPlayer = 15f;
    
    private SpriteRenderer sr;
    private Collider2D col;
    private Camera mainCamera;
    private Transform playerTransform;
    private bool isPickedUp = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        mainCamera = Camera.main;
        
        // 查找玩家对象
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("[PickupControl] 未找到玩家对象，无法进行距离检测");
        }
        
        // 启动自动销毁协程
        StartCoroutine(AutoDestroyCoroutine());
    }

    // 2D触发器检测：玩家触碰即拾取
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isPickedUp)
        {
            isPickedUp = true;
            // 调用玩家拾取方法
            other.GetComponent<PlayerControl>().PickupItem(pickupType, pickupValue);
            // 隐藏并禁用碰撞（避免重复拾取）
            sr.enabled = false;
            col.enabled = false;
            // 延迟销毁
            Destroy(gameObject, 0.1f);
        }
    }

    /// <summary>
    /// 自动销毁协程：指定时间后检查距离玩家是否超过设定范围，如果超过则销毁
    /// </summary>
    private System.Collections.IEnumerator AutoDestroyCoroutine()
    {
        yield return new WaitForSeconds(maxLifetime);
        
        // 如果已经被拾取，不需要检查
        if (isPickedUp)
        {
            yield break;
        }
        
        // 检查距离玩家是否超过设定范围
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            if (distanceToPlayer > maxDistanceFromPlayer)
            {
                // 距离玩家过远，销毁金币
                Destroy(gameObject);
            }
        }
        else
        {
            // 如果找不到玩家，为了安全起见也销毁金币
            Debug.LogWarning("[PickupControl] 无法获取玩家位置，销毁金币以避免内存泄漏");
            Destroy(gameObject);
        }
    }
}
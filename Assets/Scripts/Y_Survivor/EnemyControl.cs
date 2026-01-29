using UnityEngine;

/// <summary>
/// 2D类幸存者敌人核心控制：自动追击玩家+近距离攻击+受击死亡+掉落金币
/// </summary>
public class Enemy2DController : MonoBehaviour
{
    [Header("移动&追击配置")]
    [Tooltip("敌人移动速度，建议比玩家慢1-2，如8-10")]
    public float moveSpeed = 8f;
    [Tooltip("追击范围：超出该范围不追玩家，建议15-20")]
    public float chaseRange = 18f;

    [Header("攻击配置")]
    [Tooltip("攻击范围：进入该范围触发攻击，建议1.5-2")]
    public float attackRange = 2f;
    [Tooltip("攻击冷却时间（秒），避免连续攻击，建议1-1.5")]
    public float attackCD = 1f;
    public int attackDamage = 5; // 攻击力

    [Header("敌人基础属性")]
    public int maxHp = 30;   // 最大血量
    public int currentHp;    // 当前血量
    [Tooltip("死亡掉落金币数量")]
    public int dropCoin = 5;

    [Header("掉落配置")]
    public GameObject coinPrefab; // 金币预制体（后续创建，挂拾取脚本）
    public float dropOffset = 0.5f; // 掉落偏移量（避免卡在敌人位置）

    private Rigidbody2D rb;
    private Transform player;      // 玩家Transform（追击目标）
    private float lastAttackTime;  // 上一次攻击时间（计算冷却）
    private bool isDead = false;   // 死亡标记

    #region 初始化
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // 找到玩家（玩家对象标签需设为Player，避免Find失败）
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Start()
    {
        currentHp = maxHp;
        lastAttackTime = -attackCD; // 初始无冷却，可直接攻击
    }
    #endregion

    #region 帧更新：追击+攻击（核心逻辑）
    private void FixedUpdate()
    {
        if (isDead || player == null)
        {
            rb.velocity = Vector2.zero; // 死亡/无玩家时停止移动
            return;
        }

        // 计算敌人到玩家的距离
        float distanceToPlayer = Vector2.Distance(rb.position, player.position);

        // 1. 超出追击范围：停止移动
        if (distanceToPlayer > chaseRange)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 2. 在攻击范围：触发攻击（带冷却）
        if (distanceToPlayer <= attackRange && Time.time - lastAttackTime >= attackCD)
        {
            AttackPlayer();
            return;
        }

        // 3. 在追击范围但超出攻击范围：追击玩家
        ChasePlayer(distanceToPlayer);
    }
    #endregion

    #region 核心行为：追击+攻击
    /// <summary>
    /// 追击玩家（2D平动，面朝玩家）
    /// </summary>
    private void ChasePlayer(float distance)
    {
        // 1. 计算追击方向（归一化，避免斜向加速）
        Vector2 chaseDir = (player.position - transform.position).normalized;
        // 2. 刚体移动（与玩家同写法，保证物理顺滑）
        rb.velocity = chaseDir * moveSpeed * Time.fixedDeltaTime;
        // 3. 敌人面朝玩家（与玩家面朝鼠标同逻辑）
        float angle = Mathf.Atan2(chaseDir.y, chaseDir.x) * Mathf.Rad2Deg;
        rb.rotation = angle;
    }

    /// <summary>
    /// 攻击玩家
    /// </summary>
    private void AttackPlayer()
    {
        lastAttackTime = Time.time; // 更新攻击时间
        // 调用玩家受击方法
        player.GetComponent<PlayerControl>().TakeDamage(attackDamage);
        // 后续可加：攻击特效、攻击音效、屏幕抖动等
        Debug.Log("敌人攻击！玩家受到" + attackDamage + "点伤害");
    }
    #endregion

    #region 受击+死亡+掉落
    /// <summary>
    /// 受击方法（玩家攻击时调用）
    /// </summary>
    /// <param name="damage">受到的伤害值</param>
    public void TakeDamage(int damage)
    {
        if (isDead) return; // 死亡后不接受伤害

        currentHp = Mathf.Max(currentHp - damage, 0);
        if (currentHp <= 0)
        {
            Die();
        }
        // 后续可加：受击特效、受击音效等
    }

    /// <summary>
    /// 死亡方法：停止行为+掉落金币+销毁自身
    /// </summary>
    private void Die()
    {
        isDead = true;
        rb.velocity = Vector2.zero; // 停止移动
        DropCoin(); // 掉落金币
        // 延迟销毁（给掉落特效留时间，可选）
        Destroy(gameObject, 0.2f);
        // 后续可加：死亡特效、死亡音效、通知波次管理器怪物死亡等
        Debug.Log("敌人死亡，掉落" + dropCoin + "金币");
    }

    /// <summary>
    /// 死亡掉落金币
    /// </summary>
    private void DropCoin()
    {
        if (coinPrefab == null) return;

        // 计算掉落位置（敌人位置+随机偏移，避免卡在原地）
        Vector2 dropPos = new Vector2(
            transform.position.x + Random.Range(-dropOffset, dropOffset),
            transform.position.y + Random.Range(-dropOffset, dropOffset)
        );
        // 生成金币预制体
        GameObject coin = Instantiate(coinPrefab, dropPos, Quaternion.identity);
        // 给金币赋值掉落数量（后续拾取脚本用）
        coin.GetComponent<PickupControl>().pickupValue = dropCoin;
    }
    #endregion

    // 2D触发器检测（可选，后续可扩展：碰到陷阱受击等）
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 示例：碰到玩家的攻击触发器受击
        // if (other.CompareTag("PlayerAttack"))
        // {
        //     TakeDamage(other.GetComponent<PlayerAttack2D>().attackDamage);
        // }
    }
}
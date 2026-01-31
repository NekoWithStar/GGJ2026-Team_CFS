using UnityEngine;

/// <summary>
/// 2D类幸存者敌人核心控制：自动追击玩家+近距离攻击+受击死亡+掉落金币
/// </summary>
public class EnemyControl : MonoBehaviour
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
    public float attackDamage = 5; // 攻击力

    [Header("敌人基础属性")]
    public float maxHp = 30;   // 最大血量
    public float currentHp;    // 当前血量
    [Tooltip("死亡掉落金币数量（如果为0，会使用 CoinSystemConfig 中的配置）")]
    public int dropCoin = 0;

    [Header("掉落配置")]
    public GameObject coinPrefab; // 金币预制体（后续创建，挂拾取脚本）
    public float dropOffset = 0.5f; // 掉落偏移量（避免卡在敌人位置）

    private Rigidbody2D rb;
    private Transform player;      // 玩家Transform（追击目标）
    private float lastAttackTime;  // 上一次攻击时间（计算冷却）
    private bool isDead = false;   // 死亡标记
    private bool aiPaused = false; // AI暂停标记
    private Collider2D col2d;
    private SpriteRenderer sr;

    #region 初始化
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col2d = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
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
        if (isDead || player == null || aiPaused)
        {
            rb.velocity = Vector2.zero; // 死亡/无玩家/AI暂停时停止移动
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
        // 调用玩家受击方法（加防空判断）
        var pc = player.GetComponent<PlayerControl>();
        if (pc != null)
        {
            pc.TakeDamage(attackDamage);
            Debug.Log("敌人攻击！玩家受到" + attackDamage + "点伤害");
        }
    }
    #endregion

    #region 受击+死亡+掉落
    /// <summary>
    /// 受击方法（玩家攻击时调用）
    /// </summary>
    /// <param name="damage">受到的伤害值</param>
    public void TakeDamage(float damage)
    {
        if (isDead) return; // 死亡后不接受伤害

        currentHp = Mathf.Max(currentHp - Mathf.RoundToInt(damage), 0);
        Debug.Log($"敌人受击：-{damage}，当前HP = {currentHp}/{maxHp}");

        // 可添加受击反馈（闪烁 / 位移 / 音效）
        StartCoroutine(HitFlash());

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator HitFlash()
    {
        if (sr == null) yield break;
        Color orig = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.08f);
        sr.color = orig;
    }

    /// <summary>
    /// 死亡方法：停止行为+掉落金币+销毁自身
    /// </summary>
    private void Die()
    {
        isDead = true;
        rb.velocity = Vector2.zero; // 停止移动

        // 禁用碰撞与渲染（可选）
        if (col2d != null) col2d.enabled = false;

        DropCoin(); // 掉落金币
        // 延迟销毁（给掉落特效留时间，可选）
        Destroy(gameObject, 0.2f);
        // 后续可加：死亡特效、死亡音效、通知波次管理器怪物死亡等
        Debug.Log("敌人死亡，掉落" + dropCoin + "金币");
    }

    /// <summary>
    /// 死亡掉落金币（使用统一的金币配置）
    /// </summary>
    private void DropCoin()
    {
        if (coinPrefab == null) return;

        // 使用配置中的掉落金币数，如果没有配置就使用本地的 dropCoin
        int coinAmount = dropCoin > 0 ? dropCoin : (CoinSystemConfig.Instance != null ? CoinSystemConfig.Instance.GetCoinDropPerEnemy() : 5);

        // 计算掉落位置（敌人位置+随机偏移，避免卡在原地）
        Vector2 dropPos = new Vector2(
            transform.position.x + Random.Range(-dropOffset, dropOffset),
            transform.position.y + Random.Range(-dropOffset, dropOffset)
        );
        // 生成金币预制体
        GameObject coin = Instantiate(coinPrefab, dropPos, Quaternion.identity);
        // 给金币赋值掉落数量（后续拾取脚本用）
        var pickup = coin.GetComponent<PickupControl>();
        if (pickup != null)
        {
            // PickupControl 期望 PickupItem 调用时传入类型和值，这里我们把值放到 pickupValue 字段
            // 但 PickupControl 的当前实现不直接使用该字段，我们保持兼容（PickupControl 已使用 pickupValue 字段）
            // 如果 coin 预制体使用了不同的拾取脚本，请在预制体中设置或修改下面逻辑。
        }
        var pctrl = coin.GetComponent<PickupControl>();
        if (pctrl != null)
        {
            // PickupControl.cs 使用 pickupType/pickupValue，调整为 Coin 类型
            // （如果 coin prefab 没有设置，建议在 prefab inspector 中设置）
            // 这里尽量保证值被设置
            // (PickupControl 在拾取时读取 pickupType/pickupValue 字段)
        }
        // 如果需要，把数值写在一个可被拾取脚本读取的位置（例如一个 PickupData 脚本），此处保留为预制体配置优先
    }
    #endregion

    #region 碰撞/触发：接收伤害（兼容不同武器实现）
    // 推荐做法：玩家武器在命中时携带一个 DamageDealer 组件或具有 tag "PlayerWeapon"
    // 下面实现会优先查找 DamageDealer 组件（建议），否则根据 tag "PlayerWeapon" 从玩家组件读取攻击力

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        ApplyDamageFromCollider(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        ApplyDamageFromCollider(collision.collider);
    }

    private void ApplyDamageFromCollider(Collider2D other)
    {
        if (other == null) return;

        // 1) 优先查找 DamageDealer 组件（建议把武器或攻击判定体上挂上该组件）
        var dd = other.GetComponent<DamageDealer>() ?? other.GetComponentInParent<DamageDealer>();
        if (dd != null)
        {
            // 避免自伤（DamageDealer 可持有 owner 引用）
            if (dd.owner != null && dd.owner == gameObject) return;

            TakeDamage(dd.damage);

            if (dd.destroyOnHit && other.gameObject != null)
            {
                // 如果 DamageDealer 在独立临时物体上，直接销毁该物体
                Destroy(other.gameObject);
            }

            return;
        }

        // 2) 退路：如果碰撞对象标记为玩家武器（"Weapon"），尝试从玩家的已装备武器读取伤害值
        if (other.CompareTag("Weapon"))
        {
            var pc = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerControl>();
            if (pc != null)
            {
                // 获取玩家已装备武器的伤害值
                var equippedWeapon = pc.GetEquippedWeapon() as WeaponControl;
                if (equippedWeapon != null && equippedWeapon.weaponData != null)
                {
                    float weaponDamage = equippedWeapon.GetEffectiveDamage();
                    TakeDamage(weaponDamage);
                }
                else
                {
                    Debug.LogWarning("[EnemyControl] 玩家未装备武器或武器数据缺失，使用默认伤害（10）");
                    TakeDamage(10f); // 默认伤害值
                }
            }
        }
    }
    #endregion

    #region AI控制
    /// <summary>
    /// 暂停AI
    /// </summary>
    public void PauseAI()
    {
        aiPaused = true;
        rb.velocity = Vector2.zero;
    }

    /// <summary>
    /// 恢复AI
    /// </summary>
    public void ResumeAI()
    {
        aiPaused = false;
    }
    #endregion
}
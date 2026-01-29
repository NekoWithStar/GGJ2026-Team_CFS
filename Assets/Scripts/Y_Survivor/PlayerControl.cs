using UnityEngine;

/// <summary>
/// 2D类幸存者玩家核心控制：WASD移动+鼠标朝向+基础属性/状态 + 近战攻击示例
/// </summary>
public class PlayerControl : MonoBehaviour
{
    [Header("移动配置")]
    [Tooltip("玩家移动速度，类幸存者建议8-12")]
    public float moveSpeed = 10f;
    [Tooltip("是否限制移动（如死亡/升级时）")]
    public bool canMove = true;

    [Header("玩家基础属性")]
    public int maxHp = 100;    // 最大血量
    public int currentHp;     // 当前血量
    public int attack = 10;   // 攻击力（后续攻击用）
    public int coin = 0;      // 金币（后续升级用）

    [Header("攻击配置")]
    [Tooltip("近战攻击范围（单位：世界坐标）")]
    public float attackRange = 2f;
    [Tooltip("攻击冷却（秒）")]
    public float attackCD = 0.6f;
    [Tooltip("检测敌人的层（在Inspector中设置为Enemy层）")]
    public LayerMask enemyLayer;

    private Rigidbody2D rb;       // 2D刚体（核心移动组件）
    private Vector2 moveDir;      // 移动方向
    private Camera mainCam;       // 主相机（用于鼠标朝向计算）
    private float lastAttackTime; // 上一次攻击时间
    private Vector2 lastLookDir;  // 缓存朝向（用于攻击方向计算/调试）

    #region 初始化
    private void Awake()
    {
        // 获取核心组件，避免频繁Find（性能优化+简洁）
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
    }

    private void Start()
    {
        // 初始化血量
        currentHp = maxHp;
        // 初始化攻击冷却为可立刻攻击状态
        lastAttackTime = -attackCD;
    }
    #endregion

    #region 帧更新：移动+朝向（核心逻辑）
    private void Update()
    {
        if (!canMove) return; // 不能移动则直接返回

        // 1. 获取WASD输入（二维向量，自动归一化避免斜向加速）
        GetMoveInput();
        // 2. 计算鼠标朝向，让玩家始终面朝鼠标
        LookAtMouse();

        // 3. 攻击输入（左键为示例）
        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }
    }

    // 固定帧更新：物理相关逻辑（Unity推荐，避免帧率波动导致移动卡顿）
    private void FixedUpdate()
    {
        if (!canMove)
        {
            rb.velocity = Vector2.zero; // 不能移动时清空速度，防止飘移
            return;
        }
        // 4. 刚体移动（2D物理标准写法，顺滑无穿模）
        MovePlayer();
    }
    #endregion

    #region 核心操作：移动+朝向+攻击
    /// <summary>
    /// 获取WASD移动输入
    /// </summary>
    private void GetMoveInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveDir = new Vector2(horizontal, vertical).normalized; // 斜向移动不加速
    }

    /// <summary>
    /// 刚体移动（FixedUpdate中执行，2D物理标准写法）
    /// </summary>
    private void MovePlayer()
    {
        // 给刚体赋值速度，结合移动方向和速度，Time.fixedDeltaTime是固定帧时间
        rb.velocity = moveDir * moveSpeed * Time.fixedDeltaTime;
    }

    /// <summary>
    /// 玩家面朝鼠标方向（2D核心写法，基于世界坐标计算）
    /// </summary>
    private void LookAtMouse()
    {
        // 1. 将鼠标屏幕坐标转为世界坐标（2D需指定Z轴，与玩家同层）
        Vector2 mouseWorldPos = mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCam.orthographicSize));
        // 2. 计算玩家到鼠标的方向向量
        Vector2 lookDir = mouseWorldPos - rb.position;
        lastLookDir = lookDir;
        // 3. 计算方向向量的角度（弧度转角度，2D绕Z轴旋转）
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        // 4. 给玩家设置旋转角度（面朝鼠标）
        rb.rotation = angle;
    }

    /// <summary>
    /// 检查冷却并执行攻击（近战示例）
    /// </summary>
    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCD) return; // 冷却中
        lastAttackTime = Time.time;
        PerformMeleeAttack();
    }

    /// <summary>
    /// 近战攻击实现：以玩家面向方向前方为中心，使用OverlapCircle检测敌人并造成伤害
    /// </summary>
    private void PerformMeleeAttack()
    {
        // 攻击中心位于玩家位置沿朝向的前方（避免从玩家中间检测）
        Vector2 dir = lastLookDir.normalized;
        if (dir == Vector2.zero) dir = Vector2.right; // 默认向右，防止除零
        Vector2 attackCenter = rb.position + dir * (attackRange * 0.5f);

        // 检测敌人并造成伤害
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, attackRange, enemyLayer);
        if (hits.Length == 0)
        {
            Debug.Log("玩家攻击，未命中敌人。");
            return;
        }

        int hitCount = 0;
        foreach (var col in hits)
        {
            // 尝试调用敌人的受击接口
            var enemy = col.GetComponent<Enemy2DController>();
            if (enemy != null)
            {
                enemy.TakeDamage(attack);
                hitCount++;
            }
        }

        Debug.Log($"玩家攻击，命中敌人数量：{hitCount}，造成攻击力：{attack}");
        // 后续可加：攻击动画、击退、特效、音效等
    }
    #endregion

    #region 基础状态方法（后续扩展直接补逻辑，无需改核心） 
    /// <summary>
    /// 受击方法（敌人攻击时调用）
    /// </summary>
    /// <param name="damage">受到的伤害值</param>
    public void TakeDamage(int damage)
    {
        currentHp = Mathf.Max(currentHp - damage, 0); // 血量不小于0
        if (currentHp <= 0)
        {
            Die(); // 血量为0则死亡
        }
        // 后续可加：受击特效、无敌帧、屏幕抖动等
    }

    /// <summary>
    /// 死亡方法
    /// </summary>
    private void Die()
    {
        canMove = false; // 死亡后禁止移动
        // 后续可加：死亡特效、游戏结束UI、销毁玩家等
        Debug.Log("玩家死亡！");
    }

    /// <summary>
    /// 拾取道具方法（金币/血包，拾取脚本调用）
    /// </summary>
    /// <param name="type">道具类型：Coin/Hp</param>
    /// <param name="value">道具数值</param>
    public void PickupItem(string type, int value)
    {
        switch (type)
        {
            case "Coin":
                coin += value;
                Debug.Log("拾取金币：" + value + "，当前金币：" + coin);
                break;
            case "Hp":
                currentHp = Mathf.Min(currentHp + value, maxHp); // 血量不超过最大值
                Debug.Log("拾取血包：" + value + "，当前血量：" + currentHp);
                break;
        }
        // 后续可加：拾取特效、拾取音效等
    }
    #endregion

    
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 仅在编辑器中绘制攻击检测范围，便于调试
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb == null) return;

        Vector2 dir = lastLookDir.normalized;
        if (dir == Vector2.zero) dir = Vector2.right;
        Vector2 center = rb.position + dir * (attackRange * 0.5f);

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        Gizmos.DrawSphere(center, 0.05f);
        UnityEditor.Handles.color = new Color(1f, 0.2f, 0.2f, 0.15f);
        UnityEditor.Handles.DrawSolidDisc(center, Vector3.forward, attackRange);
    }
#endif
}
using UnityEngine;

/// <summary>
/// 2D类幸存者玩家核心控制：WASD移动+鼠标朝向+基础属性/状态
/// 外置武器支持：可在检视面板指定预制体或运行时通过脚本更换，武器脚本可实现 IWeapon 接口以接收使用调用
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

    [Header("外置武器（可选）")]
    [Tooltip("在Inspector指定外置武器预制体，启动时会实例化并挂载到 weaponAttachPoint")]
    public GameObject externalWeaponPrefab;
    [Tooltip("武器挂点（为空则使用玩家物体Transform作为挂点）")]
    public Transform weaponAttachPoint;

    [Header("武器输入")]
    [Tooltip("发射/使用武器按键，默认鼠标左键")]
    public KeyCode fireKey = KeyCode.Mouse0;
    [Tooltip("是否按住持续开火（对支持冷却的远程武器有效）")]
    public bool holdToFire = false;

    private Rigidbody2D rb;       // 2D刚体（核心移动组件）
    private Vector2 moveDir;      // 移动方向
    private Camera mainCam;       // 主相机（用于鼠标朝向计算）

    // 外置武器实例与接口引用（可在运行时通过 API 更换）
    private GameObject externalWeaponInstance;
    private IWeapon externalWeaponScript;

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

        // 如果在Inspector中指定了武器预制体，自动装备
        if (externalWeaponPrefab != null)
        {
            EquipExternalWeapon(externalWeaponPrefab);
        }

        // 如果没有指定挂点，则默认使用玩家自身Transform
        if (weaponAttachPoint == null)
        {
            weaponAttachPoint = transform;
        }
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

        // 3. 攻击输入：直接在 PlayerControl 中处理已装备武器的触发
        var equipped = GetEquippedWeapon();
        if (equipped != null)
        {
            bool doFire = holdToFire ? Input.GetKey(fireKey) : Input.GetKeyDown(fireKey);
            if (doFire)
            {
                UseEquippedWeapon();
            }
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
        // 3. 刚体移动（2D物理标准写法，顺滑无穿模）
        MovePlayer();
    }
    #endregion

    #region 核心操作：移动+朝向+武器挂载API
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
        // 3. 计算方向向量的角度（弧度转角度，2D绕Z轴旋转）
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        // 4. 给玩家设置旋转角度（面朝鼠标）
        rb.rotation = angle;
    }

    /// <summary>
    /// 将武器预制体实例化并挂载到 weaponAttachPoint（若已有则替换）
    /// 如果实例上存在实现 IWeapon 的组件，会缓存引用便于调用
    /// </summary>
    /// <param name="weaponPrefab">武器预制体</param>
    public void EquipExternalWeapon(GameObject weaponPrefab)
    {
        if (weaponPrefab == null) return;

        // 卸载旧武器
        if (externalWeaponInstance != null)
        {
            Destroy(externalWeaponInstance);
            externalWeaponInstance = null;
            externalWeaponScript = null;
        }

        // 实例化并挂到挂点
        externalWeaponInstance = Instantiate(weaponPrefab, weaponAttachPoint.position, weaponAttachPoint.rotation, weaponAttachPoint);
        externalWeaponInstance.transform.localPosition = Vector3.zero;
        externalWeaponInstance.transform.localRotation = Quaternion.identity;

        // 查找实现 IWeapon 的脚本（若有）
        externalWeaponScript = externalWeaponInstance.GetComponentInChildren<IWeapon>();
    }

    /// <summary>
    /// 卸下当前外置武器（销毁实例）
    /// </summary>
    public void UnequipExternalWeapon()
    {
        if (externalWeaponInstance != null)
        {
            Destroy(externalWeaponInstance);
            externalWeaponInstance = null;
            externalWeaponScript = null;
        }
    }

    /// <summary>
    /// 使用已装备武器（由外部调用或动画事件触发）。
    /// 武器脚本需实现 IWeapon 接口以响应 Use 调用；否则不会产生效果。
    /// </summary>
    public void UseEquippedWeapon()
    {
        if (externalWeaponScript != null)
        {
            externalWeaponScript.Use(gameObject);
        }
    }

    /// <summary>
    /// 返回当前装备的 IWeapon（方便外部脚本控制）
    /// </summary>
    public IWeapon GetEquippedWeapon()
    {
        return externalWeaponScript;
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
}

/// <summary>
/// 武器行为接口（可由外置武器脚本实现），Use 方法接收使用者（玩家）对象
/// 这样可以将具体攻击/发射/冷却等逻辑放在武器脚本中，PlayerControl 只负责挂载与调用
/// </summary>
public interface IWeapon
{
    /// <summary>
    /// 使用武器（例如近战挥砍、发射子弹等）
    /// </summary>
    /// <param name="user">发起使用的物体（通常为玩家）</param>
    void Use(GameObject user);
}
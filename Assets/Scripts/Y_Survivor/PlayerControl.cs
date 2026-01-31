using UnityEngine;
using Y_Survivor;

/// <summary>
/// 2D类幸存者玩家核心控制：WASD移动+鼠标朝向+基础属性/状态
/// 外置武器支持：可在检视面板指定预制体或运行时通过脚本更换，武器脚本可实现 IWeapon 接口以接收使用调用
/// </summary>
public class PlayerControl : MonoBehaviour
{
    [Header("移动配置")]
    [Tooltip("是否限制移动（如死亡/升级时）")]
    public bool canMove = true;

    [Header("玩家基础属性")]
    private int maxHp = 100;    // 最大血量（改为 private，从 PropertyManager 获取）
    private float baseMoveSpeed = 100f; // 基础移动速度（改为 private，从 PropertyManager 获取）
    public int currentHp;     // 当前血量
    public int coin = 0;      // 金币（后续升级用）

    [Header("外置武器（可选）")]
    [Tooltip("在Inspector指定外置武器预制体，启动时会实例化并挂载到 weaponAttachPoint")]
    public GameObject externalWeaponPrefab;
    [Tooltip("武器挂点（为空则使用玩家物体Transform作为挂点）")]
    public Transform weaponAttachPoint;

    [Header("武器输入")]
    [Tooltip("发射/使用武器按键，默认鼠标左键")]
    public KeyCode fireKey = KeyCode.Mouse0;

    private Rigidbody2D rb;       // 2D刚体（核心移动组件）
    private Vector2 moveDir;      // 移动方向
    private Camera mainCam;       // 主相机（用于鼠标朝向计算）
    private PlayerPropertyManager playerPropertyManager; // 玩家属性管理器（血量、移动速度等）

    // 外置武器实例与接口引用（可在运行时通过 API 更换）
    private GameObject externalWeaponInstance;
    private IWeapon externalWeaponScript;
    
    /// <summary>
    /// 获取当前装备的外置武器实例（只读）
    /// </summary>
    public GameObject ExternalWeaponInstance => externalWeaponInstance;

    #region 初始化
    private void Awake()
    {
        // 获取核心组件，避免频繁Find（性能优化+简洁）
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
        playerPropertyManager = GetComponent<PlayerPropertyManager>();
        
        if (playerPropertyManager == null)
        {
            Debug.LogWarning("[PlayerControl] 玩家未挂载 PlayerPropertyManager，属性修饰系统将不可用");
        }
    }

    private void Start()
    {
        // 初始化血量
        currentHp = maxHp;
        
        // 如果有PropertyManager，将其基础值同步到当前值
        if (playerPropertyManager != null)
        {
            playerPropertyManager.SetCurrentHealth(currentHp);
        }

        // 如果没有指定挂点，则默认使用玩家自身Transform（必须先初始化）
        if (weaponAttachPoint == null)
        {
            weaponAttachPoint = transform;
        }

        // 优先检查 weaponAttachPoint 下是否已经存在武器（场景中预先挂载）
        if (weaponAttachPoint.childCount > 0)
        {
            // 遍历子物体，查找带有 WeaponControl 组件的武器
            foreach (Transform child in weaponAttachPoint)
            {
                var weaponControl = child.GetComponentInChildren<WeaponControl>();
                if (weaponControl != null)
                {
                    externalWeaponInstance = child.gameObject;
                    externalWeaponScript = weaponControl as IWeapon;
                    
                    Debug.Log($"[PlayerControl] ✅ 检测到场景中已存在的武器: {child.name}" +
                             $"\n  - WeaponControl: 已找到" +
                             $"\n  - weaponData: {(weaponControl.weaponData != null ? weaponControl.weaponData.weaponName : "❌ 未设置")}" +
                             $"\n  - 如需更换武器数据，请在 Inspector 中设置 WeaponControl 的 weaponData 字段");
                    
                    // 如果 externalWeaponPrefab 也设置了，更新引用以保持一致
                    if (externalWeaponPrefab == null)
                    {
                        externalWeaponPrefab = child.gameObject;
                    }
                    
                    return; // 找到武器后直接返回
                }
            }
        }

        // 如果场景中没有武器，再检查 Inspector 中是否指定了武器预制体
        if (externalWeaponPrefab != null)
        {
            Debug.Log($"[PlayerControl] 📋 场景中未找到武器，从 Prefab 实例化: {externalWeaponPrefab.name}");
            EquipExternalWeapon(externalWeaponPrefab);
        }
        else
        {
            Debug.LogWarning("[PlayerControl] ⚠️ 未检测到武器！" +
                           $"\n  - 方式1：在场景中将武器作为子物体挂载到 {weaponAttachPoint.name} 下" +
                           $"\n  - 方式2：在 Inspector 中设置 externalWeaponPrefab 字段" +
                           $"\n  - 方式3：通过代码调用 EquipExternalWeapon()");
        }
    }
    #endregion

    #region 帧更新：移动+朝向（核心逻辑）
    private void Update()
    {
        // 诊断快捷键：按 J 检查武器装备状态
        if (Input.GetKeyDown(KeyCode.J))
        {
            DiagnoseWeaponStatus();
        }
        
        if (!canMove) return; // 不能移动则直接返回

        // 1. 获取WASD输入（二维向量，自动归一化避免斜向加速）
        GetMoveInput();
        // 2. 计算鼠标朝向，让玩家始终面朝鼠标
        LookAtMouse();

        // 3. 攻击输入：所有手动武器都是连续开火（按住持续射击）
        HandleWeaponInput();
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
        // 获取最终移动速度（优先使用PropertyManager的修饰后值）
        float finalMoveSpeed = baseMoveSpeed;
        if (playerPropertyManager != null)
        {
            finalMoveSpeed = playerPropertyManager.GetMoveSpeed();
        }
        
        // 给刚体赋值速度，结合移动方向和速度，Time.fixedDeltaTime是固定帧时间
        rb.velocity = moveDir * finalMoveSpeed * Time.fixedDeltaTime;
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
    /// 处理武器输入并进行诊断
    /// </summary>
    private void HandleWeaponInput()
    {
        var equipped = GetEquippedWeapon();
        
        // 诊断检查1：检查是否有装备武器
        if (equipped == null)
        {
            if (Input.GetKeyDown(fireKey))
            {
                Debug.LogWarning("[PlayerControl] ❌ 武器未装备！GetEquippedWeapon() 返回 null。" +
                                 $"\n  - externalWeaponInstance: {(externalWeaponInstance != null ? "✅ 存在" : "❌ 为null")}" +
                                 $"\n  - externalWeaponScript: {(externalWeaponScript != null ? "✅ 存在" : "❌ 为null")}");
            }
            return;
        }
        
        var weaponData = GetEquippedWeaponData();
        
        // 诊断检查2：检查武器数据
        if (weaponData == null)
        {
            if (Input.GetKeyDown(fireKey))
            {
                Debug.LogWarning("[PlayerControl] ⚠️ 武器数据丢失！GetEquippedWeaponData() 返回 null。" +
                                 $"\n  - WeaponInstance: {externalWeaponInstance.name}" +
                                 $"\n  - 请检查 WeaponControl 是否获取到 weaponData");
            }
            return;
        }
        
        // 持续自动开火武器由WeaponControl自动处理，不响应玩家输入
        if (weaponData.continuousAutoFire)
        {
            return;
        }
        
        var wc = externalWeaponScript as WeaponControl;
        
        // 诊断检查3：检查WeaponControl组件
        if (wc == null)
        {
            if (Input.GetKeyDown(fireKey))
            {
                Debug.LogError("[PlayerControl] ❌ WeaponControl 组件缺失！" +
                               $"\n  - externalWeaponScript 类型: {externalWeaponScript?.GetType().Name ?? "null"}" +
                               $"\n  - 武器预制体: {externalWeaponInstance.name}" +
                               $"\n  - 请检查武器 Prefab 是否包含 WeaponControl 组件");
            }
            return;
        }
        
        // 诊断检查4：检查按键输入
        if (Input.GetKeyDown(fireKey))
        {
            equipped.Use(gameObject);
        }
        else if (Input.GetKeyUp(fireKey))
        {
            wc.StopFiring();
        }
    }

    /// <summary>
    /// 将武器预制体实例化并挂载到 weaponAttachPoint（若已有则替换）
    /// 如果实例上存在实现 IWeapon 的组件，会缓存引用便于调用
    /// </summary>
    /// <param name="weaponPrefab">武器预制体</param>
    public void EquipExternalWeapon(GameObject weaponPrefab)
    {
        if (weaponPrefab == null)
        {
            Debug.LogError("[PlayerControl] ❌ EquipExternalWeapon: weaponPrefab 为 null！");
            return;
        }

        // 检查挂点是否有效
        if (weaponAttachPoint == null)
        {
            Debug.LogWarning("[PlayerControl] ⚠️ weaponAttachPoint 为 null，使用玩家自身作为挂点");
            weaponAttachPoint = transform;
        }

        // 卸载旧武器
        if (externalWeaponInstance != null)
        {
            Destroy(externalWeaponInstance);
            externalWeaponInstance = null;
            externalWeaponScript = null;
        }

        Debug.Log($"[PlayerControl] 🔧 实例化武器预制体...\n  - Prefab: {weaponPrefab.name}\n  - 挂点: {weaponAttachPoint.name}");

        // 实例化并挂到挂点
        externalWeaponInstance = Instantiate(weaponPrefab, weaponAttachPoint.position, weaponAttachPoint.rotation, weaponAttachPoint);
        externalWeaponInstance.transform.localPosition = Vector3.zero;
        externalWeaponInstance.transform.localRotation = Quaternion.identity;

        // 查找实现 IWeapon 的脚本（若有）
        externalWeaponScript = externalWeaponInstance.GetComponentInChildren<IWeapon>();
        
        // 诊断输出
        Debug.Log($"[PlayerControl] ✅ 武器已装备: {weaponPrefab.name}" +
                 $"\n  - Instance: {externalWeaponInstance.name}" +
                 $"\n  - IWeapon 组件: {(externalWeaponScript != null ? "✅ 找到 (" + externalWeaponScript.GetType().Name + ")" : "❌ 未找到")}" +
                 $"\n  - 检查 WeaponControl...");
        
        // 额外诊断：检查 WeaponControl 组件
        var wc = externalWeaponInstance.GetComponentInChildren<WeaponControl>();
        if (wc != null)
        {
            Debug.Log($"[PlayerControl] ✅ WeaponControl 组件已找到" +
                     $"\n  - weaponData: {(wc.weaponData != null ? "✅ " + wc.weaponData.weaponName : "❌ 为null")}" +
                     $"\n  - propertyManager: {(wc.propertyManager != null ? "✅ 存在" : "⚠️ 缺失（可选，但推荐）")}" +
                     $"\n  - muzzlePoint: {(wc.muzzlePoint != null ? "✅ 已设置" : "⚠️ 未设置（将使用武器位置）")}" +
                     $"\n  - audioSource: {(wc.audioSource != null ? "✅ 已设置" : "⚠️ 未设置（音效将无法播放）")}");
        }
        else
        {
            Debug.LogError($"[PlayerControl] ❌ WeaponControl 组件未找到！" +
                          $"\n  - 武器 Prefab: {weaponPrefab.name}" +
                          $"\n  - 请检查 Prefab 是否包含 WeaponControl 脚本");
        }
    }

    /// <summary>
    /// 装备外置武器并将对应的 ScriptableObject 数据传入（供通过卡牌选择时使用）
    /// </summary>
    /// <param name="weaponPrefab">武器预制体</param>
    /// <param name="weaponData">Weapon ScriptableObject 数据</param>
    public void EquipExternalWeapon(GameObject weaponPrefab, Weapon weaponData)
    {
        if (weaponData == null)
        {
            Debug.LogError("[PlayerControl] ❌ EquipExternalWeapon: weaponData 为 null！");
            return;
        }
        
        Debug.Log($"[PlayerControl] 📋 开始装备武器: {weaponData.weaponName}");
        
        EquipExternalWeapon(weaponPrefab);

        if (externalWeaponInstance == null)
        {
            Debug.LogError("[PlayerControl] ❌ 武器实例化失败！externalWeaponInstance 为 null");
            return;
        }
        
        var wc = externalWeaponInstance.GetComponentInChildren<WeaponControl>();
        if (wc == null)
        {
            Debug.LogError("[PlayerControl] ❌ WeaponControl 组件不存在！无法设置武器数据");
            return;
        }
        
        Debug.Log($"[PlayerControl] 🔧 设置武器数据...");
        wc.SetWeaponData(weaponData);
        
        Debug.Log($"[PlayerControl] ✅ 武器数据已设置: {weaponData.weaponName}" +
                 $"\n  - 伤害: {weaponData.damage}" +
                 $"\n  - 攻速: {weaponData.attackRate}/s" +
                 $"\n  - 类型: {weaponData.weaponType}" +
                 $"\n  - 自动开火: {(weaponData.continuousAutoFire ? "是" : "否")}");
        
        // 若为持续自动开火武器，启动自动开火
        if (weaponData.continuousAutoFire)
        {
            Debug.Log($"[PlayerControl] 🔥 启动持续自动开火...");
            wc.Use(gameObject);
        }
    }
    
    /// <summary>
    /// 更换当前武器的数据（不销毁重建武器对象，保持属性卡加成）
    /// 推荐用于卡牌选择武器切换，避免丢失属性加成
    /// </summary>
    /// <param name="newWeaponData">新的武器数据</param>
    /// <returns>是否成功更换</returns>
    public bool SwitchWeaponData(Weapon newWeaponData)
    {
        if (newWeaponData == null)
        {
            Debug.LogWarning("[PlayerControl] Cannot switch to null weapon data!");
            return false;
        }
        
        // 检查是否有武器实例
        if (externalWeaponInstance == null)
        {
            Debug.LogWarning("[PlayerControl] No weapon equipped! Use EquipExternalWeapon() first.");
            return false;
        }
        
        // 获取 WeaponControl
        var wc = externalWeaponInstance.GetComponentInChildren<WeaponControl>();
        if (wc == null)
        {
            Debug.LogError("[PlayerControl] Weapon instance has no WeaponControl component!");
            return false;
        }
        
        // 停止当前武器的所有动作
        wc.StopAutomatic();
        
        // 更换武器数据（会自动刷新 PropertyManager 的基础值）
        wc.SetWeaponData(newWeaponData);
        
        // 若为持续自动开火武器，启动自动开火
        if (newWeaponData.continuousAutoFire)
        {
            wc.Use(gameObject);
        }
        
        Debug.Log($"[PlayerControl] Switched weapon to: {newWeaponData.weaponName} " +
                  $"({newWeaponData.weaponType}). Property card bonuses preserved.");
        
        return true;
    }

    /// <summary>
    /// 卸下当前外置武器（销毁实例）
    /// </summary>
    public void UnequipExternalWeapon()
    {
        if (externalWeaponInstance != null)
        {
            // 停止自动武器的开火
            if (externalWeaponScript != null)
            {
                var stopable = externalWeaponScript as WeaponControl;
                if (stopable != null)
                {
                    stopable.StopAutomatic();
                }
            }

            Destroy(externalWeaponInstance);
            externalWeaponInstance = null;
            externalWeaponScript = null;
        }
    }

    /// <summary>
    /// 获取当前装备武器的 Weapon 数据资源
    /// </summary>
    private Weapon GetEquippedWeaponData()
    {
        if (externalWeaponInstance == null) return null;
        var weaponCtrl = externalWeaponInstance.GetComponentInChildren<WeaponControl>();
        return weaponCtrl != null ? weaponCtrl.weaponData : null;
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
    public void TakeDamage(float damage)
    {
        currentHp = Mathf.Max(currentHp - Mathf.RoundToInt(damage), 0); // 血量不小于0，伤害四舍五入
        
        // 同步到PropertyManager系统（用于属性修饰）
        if (playerPropertyManager != null)
        {
            playerPropertyManager.SetCurrentHealth(currentHp);
        }
        
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

    // PickupItem is implemented above; duplicate removed to avoid CS0111

    /// <summary>
    /// 触发卡牌选择
    /// </summary>
    private void TriggerCardSelection()
    {
        // 暂停游戏逻辑
        PauseGameForCardSelection();

        // 显示卡牌选择UI（需要CardSelectionManager）
        var cardSelection = FindAnyObjectByType<CardSelectionManager>();
        if (cardSelection != null)
        {
            cardSelection.ShowCardSelection(3); // 显示3张卡牌选择
        }
    }

    /// <summary>
    /// 暂停游戏用于卡牌选择
    /// </summary>
    private void PauseGameForCardSelection()
    {
        canMove = false;
        // 暂停武器
        var weapon = GetEquippedWeapon() as WeaponControl;
        if (weapon != null)
        {
            weapon.PauseWeapon();
        }
        // 暂停敌人AI
        var enemies = FindObjectsByType<EnemyControl>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            enemy.PauseAI();
        }
        // 暂停时间缩放，但保持音乐
        Time.timeScale = 0f;
    }

    /// <summary>
    /// 恢复游戏
    /// </summary>
    public void ResumeGame()
    {
        canMove = true;
        // 恢复武器
        var weapon = GetEquippedWeapon() as WeaponControl;
        if (weapon != null)
        {
            weapon.ResumeWeapon();
        }
        Time.timeScale = 1f;
        var enemies = FindObjectsByType<EnemyControl>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            enemy.ResumeAI();
        }
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
                Debug.Log($"拾取金币 +{value}，当前金币: {coin}");

                // 检查是否触发卡牌选择
                if (coin >= 100)
                {
                    TriggerCardSelection();
                }
                break;
            case "Hp":
                currentHp = Mathf.Min(currentHp + value, maxHp); // 血量不超过最大值
                
                // 同步到PropertyManager系统（用于属性修饰）
                if (playerPropertyManager != null)
                {
                    playerPropertyManager.SetCurrentHealth(currentHp);
                }
                
                Debug.Log("拾取血包：" + value + "，当前血量：" + currentHp);
                break;
        }
        // 后续可加：拾取特效、拾取音效等
    }

    /// <summary>
    /// 诊断方法：检查武器装备状态（按 D 键调用）
    /// </summary>
    [ContextMenu("检查武器装备状态")]
    public void DiagnoseWeaponStatus()
    {
        Debug.Log($"\n[PlayerControl] 🔍 武器装备诊断" +
                 $"\n{'='} 基础配置" +
                 $"\n  - externalWeaponPrefab: {(externalWeaponPrefab != null ? $"✅ {externalWeaponPrefab.name}" : "❌ 未设置")}" +
                 $"\n  - weaponAttachPoint: {(weaponAttachPoint != null ? $"✅ {weaponAttachPoint.name}" : "❌ 为null")}" +
                 $"\n  - fireKey: {fireKey}" +
                 $"\n{'='} 运行时状态" +
                 $"\n  - externalWeaponInstance: {(externalWeaponInstance != null ? $"✅ {externalWeaponInstance.name}" : "❌ 为null")}" +
                 $"\n  - externalWeaponScript: {(externalWeaponScript != null ? $"✅ {externalWeaponScript.GetType().Name}" : "❌ 为null")}" +
                 $"\n{'='} 武器数据");
        
        var weaponData = GetEquippedWeaponData();
        if (weaponData != null)
        {
            Debug.Log($"  - 武器名称: ✅ {weaponData.weaponName}" +
                     $"\n  - 武器类型: {weaponData.weaponType}" +
                     $"\n  - 基础伤害: {weaponData.damage}" +
                     $"\n  - 攻击速率: {weaponData.attackRate}/s" +
                     $"\n  - 自动开火: {(weaponData.continuousAutoFire ? "是" : "否")}");
        }
        else
        {
            Debug.LogWarning("  - 武器数据: ❌ 无法获取");
        }
        
        if (externalWeaponInstance != null)
        {
            var wc = externalWeaponInstance.GetComponentInChildren<WeaponControl>();
            if (wc != null)
            {
                Debug.Log($"{'='} WeaponControl 状态" +
                         $"\n  - 组件: ✅ 已找到" +
                         $"\n  - weaponData: {(wc.weaponData != null ? "✅ " + wc.weaponData.weaponName : "❌ 为null")}" +
                         $"\n  - propertyManager: {(wc.propertyManager != null ? "✅ 已挂载" : "⚠️ 缺失（属性卡无效）")}" +
                         $"\n  - muzzlePoint: {(wc.muzzlePoint != null ? "✅ 已设置" : "⚠️ 未设置")}" +
                         $"\n  - audioSource: {(wc.audioSource != null ? "✅ 已设置" : "⚠️ 未设置")}");
            }
            else
            {
                Debug.LogError($"  - WeaponControl: ❌ 组件未找到！");
            }
        }
        
        Debug.Log("=" + "\n");
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
using UnityEngine;
using UnityEngine.UI;
using Y_Survivor;

/// <summary>
/// 武器控制：挂在武器预制体上，兼容 PlayerControl 的 IWeapon 调用
/// - 可通过 weaponData 从 ScriptableObject 填充 UI 与运行时参数
/// - 实现 IWeapon 接口以被 PlayerControl.UseEquippedWeapon() 调用
/// - 近战使用 OverlapCircleAll 判定；远程实例化 projectilePrefab，如果 prefab 没有 DamageDealer 则动态补充
/// - 支持自动开火（automatic = true）：装备时自动按 attackRate 或 cooldown 开火
/// - 属性卡系统：通过 WeaponPropertyManager 动态修改武器属性
/// </summary>
public class WeaponControl : MonoBehaviour, IWeapon
{ 
    public enum WeaponState
    {
        Idle,               // 空闲状态
        Charging,           // 蓄力状态
        Firing,             // 单次开火状态
        ContinuousFiring,   // 持续开火状态
        Cooldown            // 冷却状态
    }

    [Header("数据源（ScriptableObject）")]
    [Tooltip("武器数据")]
    public Weapon weaponData;

    [Header("UI（可选）")]
    [Tooltip("图标")]
    public Image icon;
    [Tooltip("武器名称文本")]
    public Text weaponNameText;
    [Tooltip("伤害文本")]
    public Text damageText;
    [Tooltip("冷却文本")]
    public Text cooldownText;

    [Header("运行时挂点")]
    [Tooltip("发射口或近战判定中心")]
    public Transform muzzlePoint;
    [Tooltip("音效播放组件（可选）")]
    public AudioSource audioSource;
    
    [Header("属性管理")]
    [Tooltip("武器属性管理器（可选，如为空则使用 weaponData 的静态值）")]
    public WeaponPropertyManager propertyManager;

    // 运行时参数缓存
    private GameObject weaponUser = null; // 缓存武器使用者（玩家）
    private bool isPaused = false; // 暂停标记
    
    // 新的状态管理
    private WeaponState currentState = WeaponState.Idle;
    private float stateStartTime = 0f;
    private float continuousFireStartTime = 0f;
    private float lastFireTime = 0f;
    
    // 连续开火状态
    private bool isContinuousFiring = false; // 是否正在连续开火（按住开火键）
    
    // FirePattern 相关
    private int currentShotIndex = 0; // 当前FirePattern发射索引

    #region 生命周期与 UI 同步
    private void Awake()
    {
        if (muzzlePoint == null) muzzlePoint = transform;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (propertyManager == null) propertyManager = GetComponent<WeaponPropertyManager>();
        SyncUI();
    }

    private void Start()
    {
        // 自动开火武器在Start时需要找到Player并启动
        if (weaponData != null && weaponData.continuousAutoFire)
        {
            var player = transform.root.GetComponentInParent<PlayerControl>();
            if (player != null)
            {
                weaponUser = player.gameObject;
                Debug.Log($"[WeaponControl.Start] 🔧 自动开火武器已初始化，将在首次Update时启动");
            }
        }
    }

    private void OnValidate()
    {
        // 编辑器下实时同步显示，方便调试
        SyncUI();
    }

    private void Update()
    {
        if (weaponData == null || weaponUser == null || isPaused) return;

        UpdateWeaponState();
        
        // 处理手动连续开火（按住开火键，非自动武器）
        if (isContinuousFiring && currentState == WeaponState.Firing)
        {
            HandleContinuousFiring();
        }
    }

    /// <summary>
    /// 更新武器状态机
    /// </summary>
    private void UpdateWeaponState()
    {
        float currentTime = Time.time;
        float stateDuration = currentTime - stateStartTime;
        
        switch (currentState)
        {
            case WeaponState.Idle:
                // 持续自动开火武器在Idle状态时需要自动启动
                if (weaponData.continuousAutoFire && weaponUser != null)
                {
                    StartContinuousAutoFiring();
                }
                break;
                
            case WeaponState.Charging:
                if (stateDuration >= GetEffectiveChargingTime())
                {
                    StopChargingSound();
                    ChangeState(WeaponState.Firing);
                    // 蓄力完成后进入开火状态，由HandleContinuousFiring或单次开火处理
                    if (!isContinuousFiring)
                    {
                        PerformSingleFire();
                        ChangeState(WeaponState.Idle);
                    }
                }
                break;
                
            case WeaponState.ContinuousFiring:
                // 持续自动开火武器专用状态
                // 检查持续开火时间是否结束
                if (currentTime - continuousFireStartTime >= GetEffectiveContinuousFireDuration())
                {
                    PlayContinuousFireEndSound();
                    ChangeState(WeaponState.Cooldown);
                    break;
                }
                
                // 使用FirePattern发射
                if (weaponData.firePattern != null)
                {
                    HandleFirePattern();
                }
                else
                {
                    // 没有FirePattern时按攻击速率持续开火
                    float fireInterval = 1f / Mathf.Max(GetEffectiveAttackRate(), 0.1f);
                    if (currentTime - lastFireTime >= fireInterval)
                    {
                        PerformSingleFire();
                        lastFireTime = currentTime;
                    }
                }
                break;
                
            case WeaponState.Cooldown:
                if (stateDuration >= GetEffectiveCooldown())
                {
                    PlayCooldownEndSound();
                    ChangeState(WeaponState.Idle);
                }
                break;
                
            case WeaponState.Firing:
                // 连续开火状态不自动退出，由StopContinuousFiring控制
                if (!isContinuousFiring)
                {
                    ChangeState(WeaponState.Idle);
                }
                break;
        }
    }
    
    /// <summary>
    /// 改变武器状态
    /// </summary>
    private void ChangeState(WeaponState newState)
    {
        currentState = newState;
        stateStartTime = Time.time;
        
        // 状态切换时的特殊处理
        switch (newState)
        {
            case WeaponState.Charging:
                PlayChargingSound();
                break;
            case WeaponState.ContinuousFiring:
                continuousFireStartTime = Time.time;
                lastFireTime = Time.time - 1f; // 确保立即开火
                break;
        }
    }
    
    /// <summary>
    /// 开始持续自动开火（持续自动开火武器专用）
    /// </summary>
    private void StartContinuousAutoFiring()
    {
        if (weaponData.firePattern != null)
        {
            weaponData.firePattern.OnFireStart();
            currentShotIndex = 0;
        }
        ChangeState(WeaponState.ContinuousFiring);
    }
    
    /// <summary>
    /// 处理FirePattern发射逻辑
    /// </summary>
    private void HandleFirePattern()
    {
        float currentTime = Time.time;
        // 统一使用动态攻击速率控制发射间隔
        float fireInterval = 1f / Mathf.Max(GetEffectiveAttackRate(), 0.1f);
        
        if (currentTime - lastFireTime >= fireInterval)
        {
            if (currentShotIndex < weaponData.firePattern.shotsPerCycle)
            {
                float elapsedTime = currentTime - continuousFireStartTime;
                Vector2 mainDirection = weaponData.firePattern.GetFireDirection(
                    muzzlePoint != null ? muzzlePoint : transform, 
                    currentShotIndex, 
                    weaponData.firePattern.shotsPerCycle, 
                    elapsedTime
                );
                
                Vector2 basePositionOffset = weaponData.firePattern.GetPositionOffset(
                    muzzlePoint != null ? muzzlePoint : transform, 
                    currentShotIndex, 
                    weaponData.firePattern.shotsPerCycle, 
                    elapsedTime
                );
                
                // 获取多发子弹的所有方向
                Vector2[] directions = weaponData.firePattern.GetMultiShotDirections(mainDirection, currentShotIndex);
                
                // 发射所有子弹
                for (int i = 0; i < directions.Length; i++)
                {
                    Vector2 bulletOffset = basePositionOffset + weaponData.firePattern.GetMultiShotPositionOffset(mainDirection, i, directions.Length);
                    FireProjectileWithPattern(weaponUser, directions[i], bulletOffset);
                }
                
                PlayFireSound();
                
                currentShotIndex++;
                lastFireTime = currentTime;
            }
            else
            {
                // 发射完一轮，重置
                currentShotIndex = 0;
                weaponData.firePattern.OnFireStart();
            }
        }
    }
    
    /// <summary>
    /// 处理手动连续开火
    /// </summary>
    private void HandleContinuousFiring()
    {
        float currentTime = Time.time;
        float fireInterval = 1f / Mathf.Max(GetEffectiveAttackRate(), 0.1f);
        
        if (currentTime - lastFireTime >= fireInterval)
        {
            PerformSingleFire();
            lastFireTime = currentTime;
        }
    }
    
    /// <summary>
    /// 执行单次开火
    /// </summary>
    private void PerformSingleFire()
    {
        PlayFireSound();
        
        if (weaponData.weaponType == Weapon.WEAPON_TYPE.Ranged)
        {
            FireProjectile(weaponUser);
        }
        else // Melee
        {
            PerformMelee(weaponUser);
        }
    }

    /// <summary>
    /// 将 ScriptableObject 的数据同步到 Inspector 指定的 UI 元素（最小侵入）
    /// 直接复制 Image 组件的所有属性（仿照 CardControl 模式）
    /// </summary>
    private void SyncUI()
    {
        if (weaponData == null) return;

        if (icon != null && weaponData.cardPicture_Wp != null)
        {
            // 仿照 CardControl 的方式，直接复制所有 Image 属性
            icon.sprite = weaponData.cardPicture_Wp.sprite;
            icon.color = weaponData.cardPicture_Wp.color;
            icon.material = weaponData.cardPicture_Wp.material;
            icon.enabled = true;
        }
        else if (icon != null)
        {
            icon.enabled = false;
        }

        if (weaponNameText != null) weaponNameText.text = weaponData.weaponName ?? "";
        if (damageText != null) damageText.text = weaponData.damage.ToString();
        if (cooldownText != null) cooldownText.text = weaponData.cooldown.ToString("F2");
    }

    /// <summary>
    /// 在运行时设置/替换 weaponData 并刷新 UI
    /// 支持从近战切换到远程，或从远程切换到近战
    /// 保持所有属性卡加成不变（仅更新基础值）
    /// </summary>
    public void SetWeaponData(Weapon newData)
    {
        if (newData == null)
        {
            Debug.LogWarning("[WeaponControl] Attempting to set null weapon data!");
            return;
        }
        
        // 记录旧的武器类型（用于日志）
        Weapon.WEAPON_TYPE oldType = weaponData != null ? weaponData.weaponType : Weapon.WEAPON_TYPE.Melee;
        Weapon.WEAPON_TYPE newType = newData.weaponType;
        
        // 停止当前所有动作
        StopChargingSound();
        ChangeState(WeaponState.Idle);
        isContinuousFiring = false;
        
        // 更新武器数据
        weaponData = newData;
        
        // 如果有属性管理器，刷新其基础值（保持所有修饰符）
        if (propertyManager != null)
        {
            propertyManager.RefreshBaseValues();
        }
        else
        {
            Debug.LogWarning("[WeaponControl] No PropertyManager found! Property cards will not work.");
        }
        
        SyncUI();
        
        // 日志输出
        if (oldType != newType)
        {
            Debug.Log($"[WeaponControl] Weapon type changed: {oldType} → {newType}");
        }
        Debug.Log($"[WeaponControl] Weapon data updated to: {newData.weaponName}");
    }
    
    #region 动态属性获取
    /// <summary>
    /// 获取当前冷却时间（考虑属性卡加成）
    /// </summary>
    private float GetEffectiveCooldown()
    {
        if (propertyManager != null)
            return propertyManager.GetCooldown();
        return weaponData != null ? weaponData.cooldown : 0.5f;
    }
    
    /// <summary>
    /// 获取当前攻击速率（考虑属性卡加成）
    /// </summary>
    private float GetEffectiveAttackRate()
    {
        if (propertyManager != null)
            return propertyManager.GetAttackRate();
        return weaponData != null ? weaponData.attackRate : 1f;
    }
    
    /// <summary>
    /// 获取当前伤害值（考虑属性卡加成）- 公开方法供外部使用
    /// </summary>
    public int GetEffectiveDamage()
    {
        if (propertyManager != null)
            return propertyManager.GetDamage();
        return weaponData != null ? weaponData.damage : 10;
    }
    
    /// <summary>
    /// 获取当前蓄力时间（考虑属性卡加成）
    /// </summary>
    private float GetEffectiveChargingTime()
    {
        if (propertyManager != null)
            return propertyManager.GetChargingTime();
        return weaponData != null ? weaponData.chargingTime : 0.2f;
    }
    
    /// <summary>
    /// 获取当前持续开火时间（考虑属性卡加成）
    /// </summary>
    private float GetEffectiveContinuousFireDuration()
    {
        if (propertyManager != null)
            return propertyManager.GetContinuousFireDuration();
        return weaponData != null ? weaponData.continuousFireDuration : 5f;
    }
    
    /// <summary>
    /// 获取当前暴击率（考虑属性卡加成）
    /// </summary>
    private float GetEffectiveCritChance()
    {
        if (propertyManager != null)
            return propertyManager.GetCritChance();
        return weaponData != null ? weaponData.critChanceBase : 0f;
    }
    
    /// <summary>
    /// 获取当前暴击伤害倍率（考虑属性卡加成）
    /// </summary>
    private float GetEffectiveCritDamageMultiplier()
    {
        if (propertyManager != null)
            return propertyManager.GetCritDamageMultiplier();
        return weaponData != null ? weaponData.critDamageBase : 1.5f;
    }
    
    /// <summary>
    /// 获取当前近战范围（考虑属性卡加成）
    /// </summary>
    private float GetEffectiveMeleeRange()
    {
        if (propertyManager != null)
            return propertyManager.GetMeleeRange();
        return weaponData != null ? weaponData.meleeRange : 2f;
    }
    #endregion
    #endregion

    #region IWeapon 实现
    /// <summary>
    /// IWeapon 接口实现 — 开始开火（按下开火键）
    /// 所有武器都是连续开火模式，按住持续射击
    /// </summary>
    /// <param name="user">发起使用的物体（通常为玩家）</param>
    public void Use(GameObject user)
    {
        if (weaponData == null)
        {
            Debug.LogError("[WeaponControl.Use] ❌ weaponData 为 null！无法开火");
            return;
        }

        if (user == null)
        {
            Debug.LogError("[WeaponControl.Use] ❌ user 参数为 null！");
            return;
        }

        weaponUser = user;
        
        // 持续自动开火武器由Update自动处理，不响应玩家输入
        if (weaponData.continuousAutoFire) 
        {
            // 确保已启动连续开火
            if (currentState == WeaponState.Idle)
            {
                StartContinuousAutoFiring();
            }
            return;
        }
        
        // 已经在开火或其他状态，忽略
        if (currentState != WeaponState.Idle)
        {
            return;
        }
        
        // 标记为连续开火状态
        isContinuousFiring = true;
        lastFireTime = Time.time - 1f; // 确保立即开火
        
        if (weaponData.requiresCharging)
        {
            // 需要蓄力：先蓄力，蓄力完成后进入Firing状态连续开火
            ChangeState(WeaponState.Charging);
        }
        else
        {
            // 不需要蓄力：直接进入Firing状态连续开火
            ChangeState(WeaponState.Firing);
        }
    }
    
    /// <summary>
    /// 停止开火（松开开火键）
    /// </summary>
    public void StopFiring()
    {
        isContinuousFiring = false;
        
        // 如果正在蓄力，取消蓄力
        if (currentState == WeaponState.Charging)
        {
            StopChargingSound();
            ChangeState(WeaponState.Idle);
        }
        // 如果正在开火，回到空闲
        else if (currentState == WeaponState.Firing)
        {
            ChangeState(WeaponState.Idle);
        }
    }

    /// <summary>
    /// 停止自动武器开火（卸除武器时调用）
    /// </summary>
    public void StopAutomatic()
    {
        weaponUser = null;
        isContinuousFiring = false;
        StopChargingSound();
        ChangeState(WeaponState.Idle);
    }
    
    /// <summary>
    /// 播放开火音效
    /// </summary>
    private void PlayFireSound()
    {
        if (audioSource != null && weaponData.fireSound != null)
        {
            audioSource.PlayOneShot(weaponData.fireSound);
        }
    }
    
    /// <summary>
    /// 播放蓄力音效
    /// </summary>
    private void PlayChargingSound()
    {
        if (audioSource != null && weaponData.chargingSound != null)
        {
            audioSource.clip = weaponData.chargingSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    
    /// <summary>
    /// 停止蓄力音效
    /// </summary>
    private void StopChargingSound()
    {
        if (audioSource != null && audioSource.isPlaying && audioSource.clip == weaponData.chargingSound)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }
    
    /// <summary>
    /// 播放持续开火结束音效（如过热音效）
    /// </summary>
    private void PlayContinuousFireEndSound()
    {
        if (audioSource != null && weaponData.continuousFireEndSound != null)
        {
            audioSource.PlayOneShot(weaponData.continuousFireEndSound);
        }
    }
    
    /// <summary>
    /// 播放冷却结束音效
    /// </summary>
    private void PlayCooldownEndSound()
    {
        if (audioSource != null && weaponData.cooldownEndSound != null)
        {
            audioSource.PlayOneShot(weaponData.cooldownEndSound);
        }
    }
    
    /// <summary>
    /// 获取当前武器状态（供外部查询）
    /// </summary>
    public WeaponState GetCurrentState()
    {
        return currentState;
    }
    
    /// <summary>
    /// 强制停止当前动作（如取消蓄力）
    /// </summary>
    public void CancelCurrentAction()
    {
        StopChargingSound();
        ChangeState(WeaponState.Idle);
    }
    #endregion
    
    #region 远程/近战实现
    private void FireProjectile(GameObject user)
    {
        GameObject prefab = weaponData.projectilePrefab != null ? weaponData.projectilePrefab : weaponData.weaponPrefab;
        if (prefab == null)
        {
            Debug.LogError($"[WeaponControl] ❌ 远程武器无子弹配置！" +
                          $"\n  - 武器: {(weaponData != null ? weaponData.weaponName : "null")}" +
                          $"\n  - projectilePrefab: {(weaponData?.projectilePrefab != null ? "✅ 已设置" : "❌ 为null")}" +
                          $"\n  - weaponPrefab: {(weaponData?.weaponPrefab != null ? "✅ 已设置" : "❌ 为null")}" +
                          $"\n  - 请在 Weapon ScriptableObject 中配置子弹预制体");
            return;
        }

        Vector3 spawnPos = muzzlePoint != null ? muzzlePoint.position : transform.position;
        Vector2 dir = (muzzlePoint != null) ? (muzzlePoint.right) : (user.transform.right);
        
        // 获取动态属性值
        int effectiveDamage = GetEffectiveDamage();
        float effectiveCritChance = GetEffectiveCritChance();
        float effectiveCritMultiplier = GetEffectiveCritDamageMultiplier();
        
        Debug.Log($"[WeaponControl] 🔫 发射子弹" +
                 $"\n  - 位置: {spawnPos}" +
                 $"\n  - 方向: {dir}" +
                 $"\n  - 伤害: {effectiveDamage}" +
                 $"\n  - 暴击率: {effectiveCritChance * 100:F1}%");

        // 实例化并朝向发射方向（便于视觉与旋转）
        GameObject proj = Instantiate(prefab, spawnPos, Quaternion.identity);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 优先取 DamageDealer（组件或子对象）
        var dd = proj.GetComponent<DamageDealer>() ?? proj.GetComponentInChildren<DamageDealer>();
        if (dd != null)
        {
            dd.Setup(effectiveDamage, effectiveCritChance, effectiveCritMultiplier, user);
            dd.destroyOnHit = true;
        }

        // 若预制体有 Rigidbody2D，直接设置速度；若没有则尽量补齐组件或使用 SimpleProjectile（如存在）
        var rb = proj.GetComponent<Rigidbody2D>() ?? proj.GetComponentInChildren<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.velocity = dir.normalized * weaponData.projectileSpeed;
        }
        else
        {
            // 若存在 SimpleProjectile 脚本，优先使用其 Initialize
            var sp = proj.GetComponent<SimpleProjectile>() ?? proj.GetComponentInChildren<SimpleProjectile>();
            if (sp != null)
            {
                sp.Initialize(dir.normalized, weaponData.projectileSpeed, effectiveDamage, user, 4f, true);
                return;
            }

            // 若既无 Rigidbody2D 也无 SimpleProjectile，则在实例上动态添加必要组件并驱动
            // 添加 DamageDealer（如果不存在）
            if (dd == null)
            {
                dd = proj.AddComponent<DamageDealer>();
                dd.Setup(effectiveDamage, effectiveCritChance, effectiveCritMultiplier, user);
                dd.destroyOnHit = true;
            }

            // 添加 Rigidbody2D 并赋速度
            var newRb = proj.AddComponent<Rigidbody2D>();
            newRb.gravityScale = 0f;
            newRb.velocity = dir.normalized * weaponData.projectileSpeed;

            // 添加默认碰撞体（CircleCollider2D）并设为 trigger，方便 EnemyControl 读取 DamageDealer
            var col = proj.GetComponent<Collider2D>();
            if (col == null)
            {
                var circle = proj.AddComponent<CircleCollider2D>();
                circle.isTrigger = true;
            }
            else
            {
                col.isTrigger = true;
            }
        }

        // 确保碰撞体为 trigger，便于 EnemyControl.ApplyDamageFromCollider 以触发方式读取 DamageDealer
        var collider = proj.GetComponent<Collider2D>() ?? proj.GetComponentInChildren<Collider2D>();
        if (collider != null) collider.isTrigger = true;
    }
    
    /// <summary>
    /// 使用FirePattern发射子弹（持续自动开火武器专用）
    /// </summary>
    private void FireProjectileWithPattern(GameObject user, Vector2 direction, Vector2 positionOffset)
    {
        GameObject prefab = weaponData.projectilePrefab != null ? weaponData.projectilePrefab : weaponData.weaponPrefab;
        if (prefab == null)
        {
            Debug.LogWarning("[WeaponControl] Ranged weapon has no projectilePrefab or weaponPrefab assigned: " + (weaponData != null ? weaponData.weaponName : "null"));
            return;
        }
        
        // 获取动态属性值
        int effectiveDamage = GetEffectiveDamage();
        float effectiveCritChance = GetEffectiveCritChance();
        float effectiveCritMultiplier = GetEffectiveCritDamageMultiplier();

        Vector3 spawnPos = (muzzlePoint != null ? muzzlePoint.position : transform.position) + (Vector3)positionOffset;

        // 实例化并朝向指定方向
        GameObject proj = Instantiate(prefab, spawnPos, Quaternion.identity);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 设置 DamageDealer
        var dd = proj.GetComponent<DamageDealer>() ?? proj.GetComponentInChildren<DamageDealer>();
        if (dd != null)
        {
            dd.Setup(effectiveDamage, effectiveCritChance, effectiveCritMultiplier, user);
            dd.destroyOnHit = true;
        }

        // 设置速度
        var rb = proj.GetComponent<Rigidbody2D>() ?? proj.GetComponentInChildren<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.velocity = direction.normalized * weaponData.projectileSpeed;
        }
        else
        {
            var sp = proj.GetComponent<SimpleProjectile>() ?? proj.GetComponentInChildren<SimpleProjectile>();
            if (sp != null)
            {
                sp.Initialize(direction.normalized, weaponData.projectileSpeed, effectiveDamage, user, 4f, true);
                return;
            }

            if (dd == null)
            {
                dd = proj.AddComponent<DamageDealer>();
                dd.Setup(effectiveDamage, effectiveCritChance, effectiveCritMultiplier, user);
                dd.destroyOnHit = true;
            }

            var newRb = proj.AddComponent<Rigidbody2D>();
            newRb.gravityScale = 0f;
            newRb.velocity = direction.normalized * weaponData.projectileSpeed;

            var col = proj.GetComponent<Collider2D>();
            if (col == null)
            {
                var circle = proj.AddComponent<CircleCollider2D>();
                circle.isTrigger = true;
            }
            else
            {
                col.isTrigger = true;
            }
        }

        var collider = proj.GetComponent<Collider2D>() ?? proj.GetComponentInChildren<Collider2D>();
        if (collider != null) collider.isTrigger = true;
    }

    private void PerformMelee(GameObject user)
    {
        // 获取动态属性值
        int effectiveDamage = GetEffectiveDamage();
        float effectiveMeleeRange = GetEffectiveMeleeRange();
        float effectiveCritChance = GetEffectiveCritChance();
        float effectiveCritMultiplier = GetEffectiveCritDamageMultiplier();
        
        Vector2 center = muzzlePoint != null ? (Vector2)muzzlePoint.position : (Vector2)transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, effectiveMeleeRange);
        int hitCount = 0;
        
        foreach (var col in hits)
        {
            if (col == null || col.gameObject == user) continue;

            var enemy = col.GetComponent<EnemyControl>() ?? col.GetComponentInParent<EnemyControl>();
            if (enemy != null)
            {
                // 计算暴击
                bool isCrit = Random.value < effectiveCritChance;
                float finalDamage = isCrit ? effectiveDamage * effectiveCritMultiplier : effectiveDamage;
                enemy.TakeDamage(finalDamage);
                hitCount++;
                
                if (isCrit)
                {
                    Debug.Log($"[WeaponControl] Critical Hit! Damage: {finalDamage}");
                }
                continue;
            }

            // 若是命中带 DamageDealer 的临时判定体（例如技能特效），则设置其 owner 与 damage
            var dd = col.GetComponent<DamageDealer>() ?? col.GetComponentInParent<DamageDealer>();
            if (dd != null)
            {
                dd.Setup(effectiveDamage, effectiveCritChance, effectiveCritMultiplier, user);
            }
        }

        Debug.Log($"[WeaponControl] Melee attack executed. Hits: {hitCount}  Damage: {effectiveDamage}  Range: {effectiveMeleeRange}");
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (weaponData == null) return;
        Transform p = (muzzlePoint != null) ? muzzlePoint : transform;
        float meleeRange = GetEffectiveMeleeRange();
        Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.15f);
        Gizmos.DrawSphere(p.position, 0.05f);
        UnityEditor.Handles.color = new Color(0.2f, 0.7f, 1f, 0.12f);
        UnityEditor.Handles.DrawSolidDisc(p.position, Vector3.forward, meleeRange);
    }
#endif

    /// <summary>
    /// 暂停武器
    /// </summary>
    public void PauseWeapon()
    {
        isPaused = true;
    }

    /// <summary>
    /// 恢复武器
    /// </summary>
    public void ResumeWeapon()
    {
        isPaused = false;
    }
}
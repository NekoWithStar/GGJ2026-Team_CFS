using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 武器控制：挂在武器预制体上，兼容 PlayerControl 的 IWeapon 调用
/// - 可通过 weaponData 从 ScriptableObject 填充 UI 与运行时参数
/// - 实现 IWeapon 接口以被 PlayerControl.UseEquippedWeapon() 调用
/// - 近战使用 OverlapCircleAll 判定；远程实例化 projectilePrefab，如果 prefab 没有 DamageDealer 则动态补充
/// </summary>
public class WeaponControl : MonoBehaviour, IWeapon
{
    [Header("数据源（ScriptableObject）")]
    public Weapon weaponData;

    [Header("UI（可选）")]
    public Image icon;
    public Text weaponNameText;
    public Text damageText;
    public Text cooldownText;
    public Text rangeText;

    [Header("运行时挂点")]
    [Tooltip("发射口或近战判定中心（为空则使用武器自身位置）")]
    public Transform muzzlePoint;

    // 运行时参数缓存
    private float lastUseTime = -999f;

    #region 生命周期与 UI 同步
    private void Awake()
    {
        if (muzzlePoint == null) muzzlePoint = transform;
        SyncUI();
    }

    private void OnValidate()
    {
        // 编辑器下实时同步显示，方便调试
        SyncUI();
    }

    /// <summary>
    /// 将 ScriptableObject 的数据同步到 Inspector 指定的 UI 元素（最小侵入）
    /// </summary>
    private void SyncUI()
    {
        if (weaponData == null) return;

        if (icon != null)
        {
            // Weapon.weaponIcon 在项目中以 Image 存储（与 Card 风格一致）。
            // 在 UI 上显示时只读取其 sprite（避免直接引用 UI 组件）
            icon.sprite = weaponData.weaponIcon != null ? weaponData.weaponIcon.sprite : null;
            icon.enabled = icon.sprite != null;
        }

        if (weaponNameText != null) weaponNameText.text = weaponData.weaponName ?? "";
        if (damageText != null) damageText.text = weaponData.damage.ToString();
        if (cooldownText != null) cooldownText.text = weaponData.cooldown.ToString("F2");
        if (rangeText != null) rangeText.text = weaponData.range.ToString("F1");
    }

    /// <summary>
    /// 在运行时设置/替换 weaponData 并刷新 UI
    /// </summary>
    public void SetWeaponData(Weapon newData)
    {
        weaponData = newData;
        SyncUI();
    }
    #endregion

    #region IWeapon 实现
    /// <summary>
    /// IWeapon 接口实现 — 由 PlayerControl 调用
    /// </summary>
    /// <param name="user">发起使用的物体（通常为玩家）</param>
    public void Use(GameObject user)
    {
        if (weaponData == null) return;

        if (Time.time - lastUseTime < weaponData.cooldown) return;
        lastUseTime = Time.time;

        if (weaponData.weaponType == Weapon.WEAPON_TYPE.Ranged)
        {
            FireProjectile(user);
        }
        else // Melee
        {
            PerformMelee(user);
        }
    }
    #endregion

    #region 远程/近战实现
    private void FireProjectile(GameObject user)
    {
        GameObject prefab = weaponData.projectilePrefab != null ? weaponData.projectilePrefab : weaponData.weaponPrefab;
        if (prefab == null)
        {
            Debug.LogWarning("[WeaponControl] Ranged weapon has no projectilePrefab or weaponPrefab assigned: " + (weaponData != null ? weaponData.weaponName : "null"));
            return;
        }

        Vector3 spawnPos = muzzlePoint != null ? muzzlePoint.position : transform.position;
        Vector2 dir = (muzzlePoint != null) ? (muzzlePoint.right) : (user.transform.right);

        // 实例化并朝向发射方向（便于视觉与旋转）
        GameObject proj = Instantiate(prefab, spawnPos, Quaternion.identity);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 优先取 DamageDealer（组件或子对象）
        var dd = proj.GetComponent<DamageDealer>() ?? proj.GetComponentInChildren<DamageDealer>();
        if (dd != null)
        {
            dd.damage = weaponData.damage;
            dd.owner = user;
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
                sp.Initialize(dir.normalized, weaponData.projectileSpeed, weaponData.damage, user, 4f, true);
                return;
            }

            // 若既无 Rigidbody2D 也无 SimpleProjectile，则在实例上动态添加必要组件并驱动
            // 添加 DamageDealer（如果不存在）
            if (dd == null)
            {
                dd = proj.AddComponent<DamageDealer>();
                dd.damage = weaponData.damage;
                dd.owner = user;
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

    private void PerformMelee(GameObject user)
    {
        Vector2 center = muzzlePoint != null ? (Vector2)muzzlePoint.position : (Vector2)transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, weaponData.range);
        int hitCount = 0;
        foreach (var col in hits)
        {
            if (col == null || col.gameObject == user) continue;

            var enemy = col.GetComponent<EnemyControl>() ?? col.GetComponentInParent<EnemyControl>();
            if (enemy != null)
            {
                enemy.TakeDamage(weaponData.damage);
                hitCount++;
                continue;
            }

            // 若是命中带 DamageDealer 的临时判定体（例如技能特效），则设置其 owner 与 damage
            var dd = col.GetComponent<DamageDealer>() ?? col.GetComponentInParent<DamageDealer>();
            if (dd != null)
            {
                dd.owner = user;
                dd.damage = weaponData.damage;
            }
        }

        Debug.Log($"[WeaponControl] Melee attack executed. Hits: {hitCount}  Damage: {weaponData.damage}");
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (weaponData == null) return;
        Transform p = (muzzlePoint != null) ? muzzlePoint : transform;
        Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.15f);
        Gizmos.DrawSphere(p.position, 0.05f);
        UnityEditor.Handles.color = new Color(0.2f, 0.7f, 1f, 0.12f);
        UnityEditor.Handles.DrawSolidDisc(p.position, Vector3.forward, weaponData.range);
    }
#endif
}
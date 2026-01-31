using UnityEngine;

/// <summary>
/// 运行时简单子弹实现（兼容现有 DamageDealer + EnemyControl）
/// - 初始化时添加/设置 DamageDealer、Rigidbody2D、Collider2D（Circle, trigger）
/// - 设置速度与生存时间；命中处理由 EnemyControl 的 ApplyDamageFromCollider 读取 DamageDealer 执行
/// </summary>
[DisallowMultipleComponent]
public class SimpleProjectile : MonoBehaviour
{
    public float lifeTime = 4f;
    public bool destroyOnAnyCollision = true;

    DamageDealer dd;
    Rigidbody2D rb;

    /// <summary>
    /// 初始化子弹（会自动添加所需组件并设置）
    /// </summary>
    /// <param name="direction">单位方向</param>
    /// <param name="speed">速度</param>
    /// <param name="damage">伤害</param>
    /// <param name="owner">发射者</param>
    /// <param name="life">存活时间</param>
    /// <param name="destroyOnHit">命中后是否销毁自己（交给 DamageDealer）</param>
    public void Initialize(Vector2 direction, float speed, int damage, GameObject owner, float life = 4f, bool destroyOnHit = true)
    {
        dd = gameObject.GetComponent<DamageDealer>() ?? gameObject.AddComponent<DamageDealer>();
        dd.damage = damage;
        dd.owner = owner;
        dd.destroyOnHit = destroyOnHit;

        rb = gameObject.GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.velocity = direction.normalized * speed;

        // 确保存在 Collider2D（用于触发检测），默认添加 CircleCollider2D 并设为 trigger
        Collider2D col = gameObject.GetComponent<Collider2D>();
        if (col == null)
        {
            var circle = gameObject.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
        }
        else
        {
            col.isTrigger = true;
        }

        lifeTime = life;
        destroyOnAnyCollision = destroyOnHit;
        Destroy(gameObject, lifeTime);
    }
}
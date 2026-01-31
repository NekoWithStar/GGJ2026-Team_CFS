using UnityEngine;

/// <summary>
/// 简单伤害承载器：将该组件放到武器命中判定体或一次性命中特效上，便于受击对象读取伤害值。
/// 使用建议：
/// - 近战武器：在武器的 Hitbox（Trigger）上挂该组件并设置 damage；武器在触发时启用该 Hitbox。
/// - 远程子弹：子弹预制体携带该组件并在命中后销毁（destroyOnHit = true）。
/// </summary>
public class DamageDealer : MonoBehaviour
{
    [Tooltip("伤害值")]
    public int damage = 10;

    [Tooltip("命中后是否销毁自己（子弹/一次性判定常用）")]
    public bool destroyOnHit = true;

    [Tooltip("伤害来源（可用于避免自伤），可在生成时设置为发起者 GameObject")]
    public GameObject owner;
}
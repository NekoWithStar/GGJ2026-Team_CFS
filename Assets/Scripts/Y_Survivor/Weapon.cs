using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    public enum WEAPON_TYPE
    {
        Melee,
        Ranged
    }

    [Header("基础信息")]
    public WEAPON_TYPE weaponType;
    public string weaponName;
    public Image weaponIcon; // 可在 Inspector 指定 UI Image

    [Header("数值")]
    public int damage = 10;

    [Tooltip("是否启用 cooldown 节流（优先级高）。若关闭则改用攻击速率 attackRate 计算间隔。")]
    public bool useCooldown = true;
    [Tooltip("当 useCooldown 为 true 时生效：每次攻击的冷却秒数（可在 Inspector 调整）。")]
    public float cooldown = 0.5f;

    [Tooltip("每秒攻击次数（attack per second）。当 useCooldown 为 false 时使用此值计算间隔（interval = 1 / attackRate）。")]
    public float attackRate = 1f;

    [Tooltip("是否为自动/连续开火武器（装备时自动按 attackRate 或 cooldown 开火）。")]
    public bool automatic = false;

    public float range = 2f; // 近战判定半径或远程可视范围
    public float attackSpeed = 1f; // 攻击动画速度或远程射速

    [Header("描述")]
    public string description;

    [Header("远程专用")]
    public GameObject projectilePrefab; // 远程子弹预制体（可为空，WeaponControl 也可使用自身 prefab）
    public float projectileSpeed = 12f;

    [Header("挂载预制体（可选）")]
    public GameObject weaponPrefab; // 可选：用于在场景中实例化的武器外观预制体
}
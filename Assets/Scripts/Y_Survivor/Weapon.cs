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
    public float cooldown = 0.5f;
    public float range = 2f; // 近战判定半径或远程可视范围

    [Header("描述")]
    public string description;

    [Header("远程专用")]
    public GameObject projectilePrefab; // 远程子弹预制体（可为空，WeaponControl 也可使用自身 prefab）
    public float projectileSpeed = 12f;

    [Header("挂载预制体（可选）")]
    public GameObject weaponPrefab; // 可选：用于在场景中实例化的武器外观预制体
}
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    public enum WEAPON_TYPE
    {
        Melee, // 近战
        Ranged // 远程
    }

    [Header("基础信息")]
    public WEAPON_TYPE weaponType;
    public string weaponName;
    public Image cardPicture_Wp; // 可在 Inspector 指定 UI Image

    [Header("数值")]
    public int damage = 10;

    [Header("开火模式")]
    [Tooltip("是否为持续自动开火武器（装备后自动连续开火，无法瞄准，按预设脚本发射）")]
    public bool continuousAutoFire = false;
    
    [Tooltip("持续自动开火持续时间（秒）- 持续自动开火武器能够开火的时间")]
    public float continuousFireDuration = 5f;
    
    [Tooltip("持续自动开火发射模式脚本（控制发射数量和方式，仅对持续自动开火武器生效）")]
    public FirePattern firePattern;
    
    [Tooltip("是否需要蓄力（所有武器类型都可以蓄力）")]
    public bool requiresCharging = false;
    
    [Tooltip("蓄力时间（秒）- 按住开火键后需要多少时间才能进入开火状态")]
    public float chargingTime = 0.2f;

    [Header("时间设置")]
    [Tooltip("冷却时间（秒）- 持续开火时间结束后，需要等待多久才能再次进行持续开火。若无需冷却可设为 0")]
    public float cooldown = 0.5f;

    [Tooltip("攻击速率（每秒攻击次数）- 在持续开火时间内或连续开火时的攻击速度")]
    public float attackRate = 1f;

    [Header("暴击")]
    [Tooltip("暴击率基础值（0~1，0=0%，1=100%）")]
    [Range(0f, 1f)]
    public float critChanceBase = 0f;
    
    [Tooltip("暴击伤害倍率基础值（1.5 = 150%伤害）")]
    public float critDamageBase = 1.5f;

    [Header("近战专用")]
    [Tooltip("近战攻击判定半径")]
    public float meleeRange = 2f;

    [Header("音效")]
    [Tooltip("开火时播放的音效")]
    public AudioClip fireSound;
    
    [Tooltip("蓄力时播放的音效")]
    public AudioClip chargingSound;
    
    [Tooltip("持续开火结束时播放的音效（如过热音效）")]
    public AudioClip continuousFireEndSound;
    
    [Tooltip("冷却结束时播放的音效")]
    public AudioClip cooldownEndSound;

    [Header("描述")]
    public string description;

    [Header("远程专用")]
    public GameObject projectilePrefab; // 远程子弹预制体（可为空，WeaponControl 也可使用自身 prefab）
    public float projectileSpeed = 12f;

    [Header("挂载预制体（可选）")]
    public GameObject weaponPrefab; // 可选：用于在场景中实例化的武器外观预制体
}
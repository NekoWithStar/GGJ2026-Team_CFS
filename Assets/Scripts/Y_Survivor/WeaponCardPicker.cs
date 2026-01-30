using UnityEngine;

/// <summary>
/// 监听 Flip_Card.OnWeaponConfirmed 并调用 PlayerControl.EquipExternalWeapon
/// 将此脚本挂到场景的 UI 管理器上，并在 Inspector 指定 player
/// </summary>
public class WeaponCardPicker : MonoBehaviour
{
    public PlayerControl player;

    private void OnEnable()
    {
        Flip_Card.OnWeaponConfirmed += HandleWeaponConfirmed;
    }

    private void OnDisable()
    {
        Flip_Card.OnWeaponConfirmed -= HandleWeaponConfirmed;
    }

    private void HandleWeaponConfirmed(Weapon weapon)
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerControl>();
        if (player == null || weapon == null) return;

        if (weapon.weaponPrefab != null)
        {
            player.EquipExternalWeapon(weapon.weaponPrefab);
        }
        else
        {
            Debug.LogWarning("WeaponCardPicker: 选中的 Weapon 没有 weaponPrefab 字段。");
        }
    }
}
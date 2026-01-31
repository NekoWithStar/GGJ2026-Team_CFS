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
        if (player == null)
        {
            Debug.LogError("[WeaponCardPicker] ❌ 未找到 Player！请确保 Player 有 'Player' 标签或在 Inspector 中设置。");
            return;
        }
        
        if (weapon == null)
        {
            Debug.LogError("[WeaponCardPicker] ❌ weapon 数据为 null！");
            return;
        }

        Debug.Log($"[WeaponCardPicker] 📋 收到武器确认: {weapon.weaponName}");

        // 检查玩家是否已有武器实例
        if (player.ExternalWeaponInstance != null)
        {
            // 已有武器：只切换数据，保持对象和属性加成
            Debug.Log($"[WeaponCardPicker] 🔄 切换武器数据（保持属性加成）");
            bool success = player.SwitchWeaponData(weapon);
            if (success)
            {
                Debug.Log($"[WeaponCardPicker] ✅ 武器数据已更新为: {weapon.weaponName}");
            }
            else
            {
                Debug.LogError($"[WeaponCardPicker] ❌ 切换武器数据失败！");
            }
        }
        else
        {
            // 首次装备：需要创建武器对象
            Debug.Log($"[WeaponCardPicker] 🆕 首次装备武器");
            
            if (weapon.weaponPrefab != null)
            {
                player.EquipExternalWeapon(weapon.weaponPrefab, weapon);
                Debug.Log($"[WeaponCardPicker] ✅ 武器已装备: {weapon.weaponName}");
            }
            else
            {
                Debug.LogError($"[WeaponCardPicker] ❌ 武器 '{weapon.weaponName}' 缺少 weaponPrefab 字段！" +
                             $"\n  - 请在 Weapon ScriptableObject 中设置 weaponPrefab");
            }
        }
    }
}
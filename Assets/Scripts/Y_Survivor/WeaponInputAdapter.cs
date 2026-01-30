using UnityEngine;

/// <summary>
/// 简单输入适配器：按键触发玩家已装备武器的 Use 调用
/// </summary>
[RequireComponent(typeof(PlayerControl))]
public class WeaponInputAdapter : MonoBehaviour
{
    public PlayerControl player;
    public KeyCode fireKey = KeyCode.Mouse0;

    private void Awake()
    {
        if (player == null) player = GetComponent<PlayerControl>() ?? GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerControl>();
    }

    private void Update()
    {
        if (player == null) return;
        if (Input.GetKeyDown(fireKey))
        {
            player.UseEquippedWeapon();
        }
    }
}
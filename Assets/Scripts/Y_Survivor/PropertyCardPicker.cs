using UnityEngine;
using Y_Survivor;

/// <summary>
/// 属性卡选择处理器 - 监听 Flip_Card.OnPropertyCardConfirmed 事件
/// 将确认的属性卡应用到玩家或敌人的属性管理器
/// </summary>
public class PropertyCardPicker : MonoBehaviour
{
    [Header("应用对象")]
    [Tooltip("将属性卡应用到玩家")]
    public bool applyToPlayer = true;
    
    [Tooltip("将属性卡应用到所有敌人（通过 EnemyPropertyManager 单例）")]
    public bool applyToAllEnemies = true;

    private PlayerControl playerControl;

    private void OnEnable()
    {
        Flip_Card.OnPropertyCardConfirmed += HandlePropertyCardConfirmed;
    }

    private void OnDisable()
    {
        Flip_Card.OnPropertyCardConfirmed -= HandlePropertyCardConfirmed;
    }

    private void HandlePropertyCardConfirmed(PropertyCard propertyCard)
    {
        if (propertyCard == null)
        {
            Debug.LogError("[PropertyCardPicker] ❌ 属性卡数据为 null！");
            return;
        }

        Debug.Log($"[PropertyCardPicker] 📋 收到属性卡确认: {propertyCard.cardName}");

        // 应用到玩家
        if (applyToPlayer)
        {
            ApplyToPlayer(propertyCard);
        }

        // 应用到所有敌人
        if (applyToAllEnemies)
        {
            ApplyToAllEnemies(propertyCard);
        }
    }

    /// <summary>
    /// 将属性卡应用到玩家
    /// </summary>
    private void ApplyToPlayer(PropertyCard propertyCard)
    {
        if (playerControl == null)
        {
            playerControl = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerControl>();
        }

        if (playerControl == null)
        {
            Debug.LogError("[PropertyCardPicker] ❌ 未找到 Player！请确保 Player 有 'Player' 标签");
            return;
        }

        var playerPropMgr = playerControl.GetComponent<PlayerPropertyManager>();
        if (playerPropMgr == null)
        {
            Debug.LogWarning("[PropertyCardPicker] ⚠️ 玩家未挂载 PlayerPropertyManager，无法应用属性卡");
            return;
        }

        playerPropMgr.ApplyPropertyCard(propertyCard);
        Debug.Log($"[PropertyCardPicker] ✅ 属性卡已应用到玩家: {propertyCard.cardName}");
    }

    /// <summary>
    /// 将属性卡应用到所有敌人
    /// </summary>
    private void ApplyToAllEnemies(PropertyCard propertyCard)
    {
        var enemyPropMgr = EnemyPropertyManager.Instance;
        if (enemyPropMgr == null)
        {
            Debug.LogWarning("[PropertyCardPicker] ⚠️ 场景中未找到 EnemyPropertyManager，无法应用属性卡到敌人");
            return;
        }

        enemyPropMgr.ApplyPropertyCard(propertyCard);
        Debug.Log($"[PropertyCardPicker] ✅ 属性卡已应用到所有敌人: {propertyCard.cardName}");
    }
}

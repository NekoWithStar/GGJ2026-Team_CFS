using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Y_Survivor;

public class Click_Sure : MonoBehaviour
{
    public enum CloseAction
    {
        SetInactive,
        Destroy
    }

    [Tooltip("选择关闭卡牌的方式：隐藏还是销毁。")]
    public CloseAction closeAction = CloseAction.SetInactive;

    [Tooltip("是否包含未激活（inactive）的卡牌。")]
    public bool includeInactive = true;

    void OnEnable()
    {
        Flip_Card.OnCardConfirmed += OnCardConfirmed;
        Flip_Card.OnWeaponConfirmed += OnWeaponConfirmed;
    }

    void OnDisable()
    {
        Flip_Card.OnCardConfirmed -= OnCardConfirmed;
        Flip_Card.OnWeaponConfirmed -= OnWeaponConfirmed;
    }

    /// <summary>
    /// 当有“通用卡牌”被确认选择后，关闭（或销毁）当前场景中所有 Flip_Card 对应的 GameObject。
    /// </summary>
    void OnCardConfirmed(Card confirmedCard)
    {
        CloseFlipCards(onlyWeaponCards: false, confirmedName: confirmedCard != null ? confirmedCard.cardName : "null");
    }

    /// <summary>
    /// 当有“武器卡”被确认选择后，关闭（或销毁）当前场景中所有“武器卡” Flip_Card（含 WeaponCardControl）。
    /// </summary>
    void OnWeaponConfirmed(Weapon confirmedWeapon)
    {
        CloseFlipCards(onlyWeaponCards: true, confirmedName: confirmedWeapon != null ? confirmedWeapon.name : "null");
    }

    /// <summary>
    /// 关闭（或销毁）场景中的 Flip_Card。
    /// - onlyWeaponCards 为 true 时，仅处理带有 WeaponCardControl 的 Flip_Card。
    /// - 使用 Resources.FindObjectsOfTypeAll 来同时查找 inactive 对象，并根据 scene.isLoaded 过滤场景内对象，
    ///   避免影响项目内的字体/资源资产或 prefab 资产。
    /// </summary>
    private void CloseFlipCards(bool onlyWeaponCards, string confirmedName)
    {
        try
        {
            var all = Resources.FindObjectsOfTypeAll<Flip_Card>();
            int closedCount = 0;

            foreach (var f in all)
            {
                if (f == null || f.gameObject == null) continue;

                // 只处理已存在于某个已加载场景中的实例（排除资产 / 未入场景的 prefab）
                if (!f.gameObject.scene.isLoaded) continue;

                // 如果不想处理 inactive 的对象，则跳过
                if (!includeInactive && !f.gameObject.activeInHierarchy) continue;

                // 当仅关闭“武器卡”时，要求该 Flip_Card 下存在 WeaponCardControl
                if (onlyWeaponCards && f.GetComponentInChildren<WeaponCardControl>() == null) continue;

                if (closeAction == CloseAction.SetInactive)
                {
                    f.gameObject.SetActive(false);
                }
                else
                {
                    UnityEngine.Object.Destroy(f.gameObject);
                }

                closedCount++;
            }

            Debug.Log($"Click_Sure: confirmed '{confirmedName}', closed {closedCount} Flip_Card(s). onlyWeaponCards={onlyWeaponCards}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Click_Sure: error while closing cards (onlyWeaponCards={onlyWeaponCards}): {ex}");
        }
    }
}

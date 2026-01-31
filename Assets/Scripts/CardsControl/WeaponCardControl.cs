using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// 将 Weapon（ScriptableObject）数据绑定到 UI，风格仿照 CardControl，供 Flip_Card 使用
/// </summary>
public class WeaponCardControl : MonoBehaviour
{
    public Weapon weapon_data; // 武器数据 ScriptableObject

    // UI 元素
    public Image icon; // 武器图标
    public Text weapon_name; // 武器名称文本
    public Text damage; // 伤害文本
    public Text cooldown; // 冷却文本
    public Text range; // 范围文本
    public Text describe; // 描述文本
    public GameObject back; // 背面对象（可选）

    private void Awake()
    {
        if (weapon_data == null) return;

        if (icon != null && weapon_data.weaponIcon != null)
        {
            icon.sprite = weapon_data.weaponIcon.sprite;
            icon.enabled = true;
        }

        if (weapon_name != null) weapon_name.text = weapon_data.weaponName;
        if (damage != null) damage.text = weapon_data.damage.ToString();
        if (cooldown != null) cooldown.text = weapon_data.cooldown.ToString("F2");
        if (range != null) range.text = weapon_data.range.ToString("F1");
        if (describe != null) describe.text = weapon_data.description;
        if (back == null && weapon_data.weaponPrefab != null)
        {
            // 可选：不强制赋值，仅保留字段方便 Inspector 绑定
        }
    }

    private void OnValidate()
    {
        // 编辑器下实时同步显示，方便调试
        if (weapon_data == null) return;
        if (weapon_name != null) weapon_name.text = weapon_data.weaponName;
        if (damage != null) damage.text = weapon_data.damage.ToString();
        if (cooldown != null) cooldown.text = weapon_data.cooldown.ToString("F2");
        if (range != null) range.text = weapon_data.range.ToString("F1");
        if (describe != null) describe.text = weapon_data.description;
        if (icon != null && weapon_data.weaponIcon != null)
        {
            icon.sprite = weapon_data.weaponIcon.sprite;
        }
    }
}
using UnityEngine;

/// <summary>
/// 最小侵入性地自动在拉近时临时禁用同对象上的 CircularMotion，并在回到轨道附近时恢复它。
/// 不修改 Pigeon 脚本，挂在与 Pigeon / CircularMotion 同一 GameObject 上即可生效。
/// </summary>
[DisallowMultipleComponent]
public class CircularMotionAutoToggle : MonoBehaviour
{
    [Tooltip("可选：优先使用此 Transform 作为旋转中心；为空时使用 CircularMotion.centerPosition")]
    public Transform rotationCenter;

    [Tooltip("当距离初始半径比例低于该值时禁用 CircularMotion（防止位置争抢）")]
    [Range(0.0f, 1.0f)]
    public float disableFactor = 0.9f;

    [Tooltip("当距离恢复到初始半径比例高于该值时重新启用 CircularMotion（应大于 disableFactor，避免抖动）")]
    [Range(0.0f, 1.0f)]
    public float enableFactor = 0.98f;

    [Tooltip("开启后打印调试日志，便于排查时序问题")]
    public bool debugLogs = false;

    CircularMotion _cm;
    float _initialRadius;
    bool _initialized;

    void Reset()
    {
        if (rotationCenter == null)
        {
            var go = GameObject.Find("RotationCenter");
            if (go != null) rotationCenter = go.transform;
        }
    }

    void Start()
    {
        _cm = GetComponent<CircularMotion>();
        if (_cm == null)
        {
            if (debugLogs) Debug.LogWarning("[CircularMotionAutoToggle] 未找到 CircularMotion，脚本已禁用。", this);
            enabled = false;
            return;
        }

        Vector3 center = GetCenterPosition();
        _initialRadius = (transform.position - center).magnitude;
        // 容错：若初始半径为 0，设置一个微小值以避免除 0 / 判定问题
        if (_initialRadius <= 0f) _initialRadius = 0.0001f;
        _initialized = true;

        if (debugLogs) Debug.Log($"[CircularMotionAutoToggle] 初始化 initialRadius={_initialRadius:F3}", this);
    }

    void Update()
    {
        if (!_initialized) return;

        Vector3 center = GetCenterPosition();
        float currentDist = (transform.position - center).magnitude;

        // 禁用条件：当前距离比初始半径小（拉近）
        if (_cm.enabled)
        {
            if (currentDist < _initialRadius * disableFactor)
            {
                _cm.enabled = false;
                if (debugLogs) Debug.Log($"[CircularMotionAutoToggle] 禁用 CircularMotion (dist={currentDist:F3}).", this);
            }
        }
        else
        {
            // 恢复条件：回到接近初始半径的位置（使用 enableFactor 做回弹阈值以避免抖动）
            if (currentDist >= _initialRadius * enableFactor)
            {
                _cm.enabled = true;
                if (debugLogs) Debug.Log($"[CircularMotionAutoToggle] 恢复 CircularMotion (dist={currentDist:F3}).", this);
            }
        }
    }

    Vector3 GetCenterPosition()
    {
        return rotationCenter != null ? rotationCenter.position : _cm.centerPosition;
    }
}
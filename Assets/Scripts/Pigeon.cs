using System.Collections;
using UnityEngine;

public class Pigeon : MonoBehaviour
{
    [Header("Pull / Return 设置")]
    [Tooltip("旋转中心（可在 Inspector 指定）")]
    public Transform rotationCenter;
    [Tooltip("被点击后靠近中心的速度（单位：单位/秒）")]
    public float pullSpeed = 8f;
    [Tooltip("到达中心判定距离")]
    public float reachThreshold = 0.15f;
    [Tooltip("到达中心后等待多少秒再恢复")]
    public float returnDelay = 0.05f;
    [Tooltip("恢复到原始位置的速度（单位：单位/秒）")]
    public float returnSpeed = 12f;

    [Header("潮汐锁定（可选）")]
    [Tooltip("启用后物体会朝向或背向旋转中心（可在 Inspector 切换）")]
    public bool enableTidalLock = false;
    [Tooltip("潮汐锁定旋转速度（度/秒），较大值更快逼近目标朝向；为 0 则瞬间对齐")]
    public float tidalRotateSpeed = 720f;
    [Tooltip("true = 面朝中心；false = 背向中心（根据精灵/模型朝向调整）")]
    public bool faceCenter = true;

    Vector3 _originalPosition;
    bool _isPulling;
    bool _isReturning;

    void Reset()
    {
        // 如果没指定中心，尝试在场景中寻找名为 "RotationCenter" 的物体（可选）
        if (rotationCenter == null)
        {
            var go = GameObject.Find("RotationCenter");
            if (go != null) rotationCenter = go.transform;
        }
    }

    void OnMouseDown()
    {
        // 使用 OnMouseDown 需要物体带 Collider，并且场景中有启用的 Camera
        if (rotationCenter == null) return;
        if (_isPulling || _isReturning) return;

        _originalPosition = transform.position;
        _isPulling = true;
    }

    void Update()
    {
        if (_isPulling)
        {
            // 向中心移动（与外部旋转脚本并行工作，结果会是半径改变的旋转或螺旋）
            transform.position = Vector3.MoveTowards(transform.position, rotationCenter.position, pullSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, rotationCenter.position) <= reachThreshold)
            {
                // 到达中心，开始恢复流程
                _isPulling = false;
                StartCoroutine(ReturnToOriginal());
            }
        }
    }

    IEnumerator ReturnToOriginal()
    {
        _isReturning = true;

        // 可选短暂停顿，再恢复
        if (returnDelay > 0f) yield return new WaitForSeconds(returnDelay);

        // 平滑移动回点击前位置
        while (Vector3.Distance(transform.position, _originalPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, _originalPosition, returnSpeed * Time.deltaTime);
            yield return null;
        }

        // 确保精确复位
        transform.position = _originalPosition;
        _isReturning = false;
        yield break;
    }

    // 可由外部脚本调用以编程触发拉向中心（例如 UI 按钮或自动测试）
    public void TriggerPullToCenter()
    {
        if (rotationCenter == null) return;
        if (_isPulling || _isReturning) return;
        _originalPosition = transform.position;
        _isPulling = true;
    }

    void LateUpdate()
    {
        // 在 LateUpdate 中应用潮汐锁定，以覆盖 CircularMotion 等在 Update 中对 transform.rotation 的更改
        if (!enableTidalLock || rotationCenter == null) return;

        Vector3 dir = rotationCenter.position - transform.position;
        if (dir.sqrMagnitude <= 0.0001f) return;

        // 计算目标角度（Z 轴旋转，适用于 2D 平面 XY）
        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (!faceCenter) angleDeg += 180f;

        Quaternion targetRot = Quaternion.Euler(0f, 0f, angleDeg);

        if (tidalRotateSpeed <= 0f)
        {
            transform.rotation = targetRot;
        }
        else
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, tidalRotateSpeed * Time.deltaTime);
        }
    }
}

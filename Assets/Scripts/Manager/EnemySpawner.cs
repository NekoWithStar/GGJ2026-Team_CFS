using UnityEngine;
using Y_Survivor;

/// <summary>
/// 敌人生成管理器 - 控制敌人的自动刷新、随机生成位置、摄像机距离等
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject prefab;
        public float weight = 1f; // 刷新权重
    }

    [Header("敌人管理")]
    [Tooltip("敌人预制体及权重（支持多种）")]
    public EnemySpawnData[] enemySpawnData;
    
    [Tooltip("初始敌人数量")]
    public int initialEnemyCount = 3;
    
    [Tooltip("最多敌人数量")]
    public int maxEnemyCount = 10;

    [Header("生成范围")]
    [Tooltip("指定摄像机（为空则使用 Camera.main）")]
    public Camera targetCamera;
    
    [Tooltip("敌人生成距离（相对摄像机可见范围）")]
    [Range(1f, 10f)]
    public float spawnDistance = 2f; // 表示在摄像机视口外倍数的距离

    [Header("刷新设置")]
    [Tooltip("是否启用自动刷新")]
    public bool enableAutoSpawn = true;
    
    [Tooltip("刷新间隔（秒）")]
    public float spawnInterval = 3f;

    private float lastSpawnTime = 0f;
    private int currentEnemyCount = 0;
    private Transform enemyContainer; // 统一的容器

    // 新增：动态调整计时器
    private float lastMaxIncreaseTime = 0f;
    private float lastSpeedIncreaseTime = 0f;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        
        // 创建敌人容器
        enemyContainer = new GameObject("EnemyContainer").transform;
        enemyContainer.parent = transform;
    }

    private void Start()
    {
        // 初始化生成敌人
        for (int i = 0; i < initialEnemyCount; i++)
        {
            SpawnEnemy();
        }

        // 初始化动态调整计时器
        lastMaxIncreaseTime = Time.time;
        lastSpeedIncreaseTime = Time.time;
    }

    private void Update()
    {
        // 动态调整最大敌人数量和生成速度
        if (Time.time - lastMaxIncreaseTime >= 10f)
        {
            maxEnemyCount *= 2;
            lastMaxIncreaseTime = Time.time;
            Debug.Log($"[EnemySpawner] 📈 最大敌人数量翻倍至: {maxEnemyCount}");
        }

        if (Time.time - lastSpeedIncreaseTime >= 20f)
        {
            spawnInterval /= 1.5f;
            // 防止间隔过小
            spawnInterval = Mathf.Max(spawnInterval, 0.1f);
            lastSpeedIncreaseTime = Time.time;
            Debug.Log($"[EnemySpawner] ⚡ 生成速度增加，间隔降至: {spawnInterval}秒");
        }

        if (!enableAutoSpawn) return;

        // 自动刷新逻辑
        if (Time.time - lastSpawnTime >= spawnInterval && currentEnemyCount < maxEnemyCount)
        {
            SpawnEnemy();
            lastSpawnTime = Time.time;
        }

        // 检查已死亡的敌人并更新计数
        UpdateEnemyCount();
    }

    /// <summary>
    /// 生成一个敌人
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemySpawnData == null || enemySpawnData.Length == 0)
        {
            Debug.LogError("[EnemySpawner] ❌ 敌人预制体列表为空！");
            return;
        }

        // 根据权重随机选择敌人预制体
        GameObject prefab = GetRandomEnemyPrefab();
        if (prefab == null) return;

        // 获取随机生成位置（摄像机可见范围外）
        Vector3 spawnPos = GetRandomSpawnPosition();

        // 实例化敌人
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity, enemyContainer);
        currentEnemyCount++;

        Debug.Log($"[EnemySpawner] 🆕 生成敌人 ({currentEnemyCount}/{maxEnemyCount}) 位置: {spawnPos}");
    }

    /// <summary>
    /// 根据权重随机选择敌人预制体
    /// </summary>
    private GameObject GetRandomEnemyPrefab()
    {
        float totalWeight = 0f;
        foreach (var data in enemySpawnData)
        {
            if (data.prefab != null)
            {
                totalWeight += data.weight;
            }
        }

        if (totalWeight <= 0f) return null;

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var data in enemySpawnData)
        {
            if (data.prefab != null)
            {
                currentWeight += data.weight;
                if (randomValue <= currentWeight)
                {
                    return data.prefab;
                }
            }
        }

        return null; // 不应该到达这里
    }

    /// <summary>
    /// 获取摄像机不可见范围外的随机位置
    /// </summary>
    private Vector3 GetRandomSpawnPosition()
    {
        if (targetCamera == null) return Vector3.zero;

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Vector3 worldCenter = targetCamera.ScreenToWorldPoint(screenCenter);

        // 计算摄像机视口大小
        float height = targetCamera.orthographicSize * 2f;
        float width = height * Screen.width / Screen.height;

        // 在摄像机外生成敌人（距离 = spawnDistance * 摄像机半尺寸）
        float spawnRange = Mathf.Max(width, height) * 0.5f * spawnDistance;

        // 随机方向
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = spawnRange;

        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * distance,
            Mathf.Sin(angle) * distance,
            0f
        );

        Vector3 spawnPos = worldCenter + offset;

        // 统一Z轴与玩家
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            spawnPos.z = player.transform.position.z;
        }

        return spawnPos;
    }

    /// <summary>
    /// 更新敌人计数（检查并移除已销毁的敌人）
    /// </summary>
    private void UpdateEnemyCount()
    {
        int aliveEnemies = 0;
        if (enemyContainer != null)
        {
            foreach (Transform child in enemyContainer)
            {
                if (child != null && child.gameObject.activeInHierarchy)
                {
                    aliveEnemies++;
                }
            }
        }
        currentEnemyCount = aliveEnemies;
    }

    /// <summary>
    /// 获取当前敌人数量
    /// </summary>
    public int GetCurrentEnemyCount()
    {
        UpdateEnemyCount();
        return currentEnemyCount;
    }

    /// <summary>
    /// 立即生成指定数量的敌人
    /// </summary>
    public void SpawnEnemies(int count)
    {
        for (int i = 0; i < count && currentEnemyCount < maxEnemyCount; i++)
        {
            SpawnEnemy();
        }
    }

    /// <summary>
    /// 生成单个敌人（随机位置）
    /// </summary>
    public void SpawnSingleEnemy()
    {
        if (currentEnemyCount < maxEnemyCount)
        {
            SpawnEnemy();
        }
    }

    /// <summary>
    /// 生成单个敌人（指定位置）
    /// </summary>
    public void SpawnSingleEnemy(Vector3 position)
    {
        if (enemySpawnData == null || enemySpawnData.Length == 0 || currentEnemyCount >= maxEnemyCount)
        {
            return;
        }

        GameObject prefab = GetRandomEnemyPrefab();
        if (prefab == null) return;

        GameObject enemy = Instantiate(prefab, position, Quaternion.identity, enemyContainer);
        currentEnemyCount++;

        Debug.Log($"[EnemySpawner] 🆕 手动生成敌人 ({currentEnemyCount}/{maxEnemyCount}) 位置: {position}");
    }

    /// <summary>
    /// 清空所有敌人
    /// </summary>
    public void ClearAllEnemies()
    {
        if (enemyContainer != null)
        {
            foreach (Transform child in enemyContainer)
            {
                Destroy(child.gameObject);
            }
        }
        currentEnemyCount = 0;
    }

    /// <summary>
    /// 切换自动刷新
    /// </summary>
    public void SetAutoSpawnEnabled(bool enabled)
    {
        enableAutoSpawn = enabled;
    }
}

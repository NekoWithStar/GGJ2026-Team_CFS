using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private Transform enemyContainer;
    private List<Enemy> activeEnemies = new List<Enemy>();

    public void SpawnEnemy(Enemy type, Vector3 position)
    {
        // 对象池生成敌怪
    }

    public void ClearAllEnemies()
    {
        // 清理所有敌怪
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

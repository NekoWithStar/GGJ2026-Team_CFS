using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }
    [SerializeField] private Transform enemyContainer;
    private List<EnemyControl> activeEnemies = new List<EnemyControl>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SpawnEnemy(EnemyControl _enemy, Vector3 _position)
    {
        // ��������ɵй�
    }

    public void ClearAllEnemies()
    {
        // �������ей�
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

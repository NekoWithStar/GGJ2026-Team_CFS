using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameState currentState;
    private float gameTime;

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
    void Start()
    {
        SceneManager sceneManager = SceneManager.Instance;//创建场景管理
        EnemyManager enemyManager = EnemyManager.Instance;//创建敌怪管理
    }

    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        LevelUp,
        GameOver
    }
}

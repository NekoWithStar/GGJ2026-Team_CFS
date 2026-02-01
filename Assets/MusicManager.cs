using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    // 音频源组件
    private AudioSource audioSource;
    
    // 公共列表：存储所有背景音乐
    public List<AudioClip> bgmClips = new List<AudioClip>();
    
    // 用于无重复随机播放的列表
    private List<AudioClip> shuffledClips = new List<AudioClip>();
    
    // 当前播放索引
    private int currentIndex = 0;
    
    // 是否需要重新洗牌
    private bool needShuffle = true;

    void Start()
    {
        // 获取或添加 AudioSource 组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 初始化洗牌列表
        ShuffleClips();
    }

    void Update()
    {
        // 如果当前没有播放音乐，播放下一首
        if (!audioSource.isPlaying && bgmClips.Count > 0)
        {
            PlayNextBGM();
        }
    }

    /// <summary>
    /// 洗牌：随机排列所有音频片段
    /// </summary>
    private void ShuffleClips()
    {
        shuffledClips = new List<AudioClip>(bgmClips);
        
        // Fisher-Yates 洗牌算法
        for (int i = shuffledClips.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            
            // 交换
            AudioClip temp = shuffledClips[i];
            shuffledClips[i] = shuffledClips[randomIndex];
            shuffledClips[randomIndex] = temp;
        }
        
        currentIndex = 0;
    }

    /// <summary>
    /// 播放下一首背景音乐
    /// </summary>
    public void PlayNextBGM()
    {
        if (bgmClips.Count == 0)
        {
            Debug.LogWarning("BGM列表为空！");
            return;
        }
        
        // 如果列表已播放完，重新洗牌
        if (currentIndex >= shuffledClips.Count)
        {
            ShuffleClips();
        }
        
        // 播放当前音频
        audioSource.clip = shuffledClips[currentIndex];
        audioSource.Play();
        
        Debug.Log($"正在播放: {audioSource.clip.name}");
        currentIndex++;
    }

    /// <summary>
    /// 停止播放
    /// </summary>
    public void StopBGM()
    {
        audioSource.Stop();
    }

    /// <summary>
    /// 暂停播放
    /// </summary>
    public void PauseBGM()
    {
        audioSource.Pause();
    }

    /// <summary>
    /// 继续播放
    /// </summary>
    public void ResumeBGM()
    {
        audioSource.Play();
    }
}

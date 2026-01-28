using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Scores_Got : MonoBehaviour
{
    [Header("检测设置")]
    [Tooltip("要检测的对象 Tag")]
    public string targetTag = "Pigeon";
    [Tooltip("轮询间隔（秒），使用 WaitForSecondsRealtime 以便在 Time.timeScale==0 下仍能检测（若启用自动恢复）")]
    public float pollInterval = 0.1f;

    [Header("得分设置")]
    [Tooltip("每减少一个目标对象增加的分数")]
    public int pointsPerObject = 1;
    [Tooltip("用于显示得分的 UI Text（可在 Inspector 指定）。如果未指定，会尝试在子对象或场景中查找同名 Text")]
    public Text scoreText;
    [Tooltip("得分显示格式，使用 {0} 占位分数，例如 \"Score: {0}\"")]
    public string scoreFormat = "Score: {0}";

    [Header("场景结束 / 恢复")]
    [Tooltip("当场景中目标 tag 数量为 0 时是否暂停（Time.timeScale=0）")]
    public bool pauseWhenZero = true;
    [Tooltip("当暂停时显示的文本（如果为空则显示格式化分数）")]
    public string zeroMessage = "All cleared!";
    [Tooltip("若场景被 pause，当检测到有新目标生成时是否自动恢复 Time.timeScale")]
    public bool autoResumeOnNewObjects = true;

    int _score;
    int _prevCount;
    float _savedTimeScale = 1f;
    bool _isPausedByThis;

    void Start()
    {
        if (string.IsNullOrEmpty(targetTag))
        {
            Debug.LogWarning("[Scores_Got] targetTag 为空，脚本已禁用。", this);
            enabled = false;
            return;
        }

        // 尝试自动寻找 scoreText（若未在 Inspector 指定）
        if (scoreText == null)
        {
            // 先找同 GameObject 的 Text
            scoreText = GetComponent<Text>();
            if (scoreText == null)
            {
                // 再查找子对象
                scoreText = GetComponentInChildren<Text>();
            }
            if (scoreText == null)
            {
                // 最后尝试场景全局查找名为 "ScoreText" 的对象上的 Text（容错）
                var go = GameObject.Find("ScoreText");
                if (go != null) scoreText = go.GetComponent<Text>();
            }
        }

        _prevCount = CountTagged();
        UpdateScoreText();
        StartCoroutine(PollLoop());
    }

    IEnumerator PollLoop()
    {
        while (true)
        {
            int current = CountTagged();

            if (current < _prevCount)
            {
                int diff = _prevCount - current;
                _score += diff * pointsPerObject;
                UpdateScoreText();
            }

            // 处理当数量为0的逻辑（暂停）
            if (current == 0)
            {
                if (pauseWhenZero && !_isPausedByThis)
                {
                    _savedTimeScale = Time.timeScale;
                    Time.timeScale = 0f;
                    _isPausedByThis = true;
                    // 如果需要在 UI 上显示专门信息，覆盖文本
                    if (!string.IsNullOrEmpty(zeroMessage) && scoreText != null)
                    {
                        scoreText.text = zeroMessage + (string.IsNullOrEmpty(scoreFormat) ? "" : ("\n" + string.Format(scoreFormat, _score)));
                    }
                }
            }
            else
            {
                // 若之前我们暂停了场景，且检测到新对象，按设置决定是否恢复
                if (_isPausedByThis && autoResumeOnNewObjects)
                {
                    Time.timeScale = _savedTimeScale;
                    _isPausedByThis = false;
                    UpdateScoreText();
                }
            }

            _prevCount = current;

            // 使用实时等待，保证在 Time.timeScale==0 时仍能检测（用于 autoResume）
            yield return new WaitForSecondsRealtime(Mathf.Max(0.01f, pollInterval));
        }
    }

    int CountTagged()
    {
        try
        {
            var objs = GameObject.FindGameObjectsWithTag(targetTag);
            return objs != null ? objs.Length : 0;
        }
        catch (UnityException)
        {
            // 如果 tag 不存在，FindGameObjectsWithTag 会抛异常，处理为 0 并警告一次
            Debug.LogWarning($"[Scores_Got] Tag \"{targetTag}\" 未定义或不存在。", this);
            enabled = false;
            return 0;
        }
    }

    void UpdateScoreText()
    {
        if (scoreText == null) return;
        scoreText.text = string.Format(scoreFormat ?? "{0}", _score);
    }

    // 对外接口：重置分数
    public void ResetScore()
    {
        _score = 0;
        UpdateScoreText();
    }

    // 对外接口：强制恢复（如果脚本之前暂停了场景）
    public void ForceResume()
    {
        if (_isPausedByThis)
        {
            Time.timeScale = _savedTimeScale;
            _isPausedByThis = false;
            UpdateScoreText();
        }
    }
}

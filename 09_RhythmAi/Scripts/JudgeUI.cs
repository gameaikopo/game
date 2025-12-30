using UnityEngine;
using TMPro;

public class JudgeUI : MonoBehaviour
{
    public static JudgeUI Instance;

    [SerializeField] 
    private TextMeshProUGUI judgeText;

    private float showTime = 0.5f;  
    private float timer = 0f;

    private Vector3 defaultScale;

    void Awake()
    {
        Instance = this;
        judgeText.text = "";
        defaultScale = judgeText.transform.localScale;
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                judgeText.text = "";
            }
        }
    }

    public void ShowJudge(string result)
    {
        judgeText.text = result;

        if (result == "Perfect")
            judgeText.color = Color.yellow;
        else if (result == "Good")
            judgeText.color = Color.green;
        else
            judgeText.color = Color.red;

        timer = showTime;

        // 기본 스케일 초기화
        judgeText.transform.localScale = defaultScale;

        // 애니메이션 시작
        StopAllCoroutines();
        StartCoroutine(PopAnimation());
    }

    private System.Collections.IEnumerator PopAnimation()
    {
        float upTime = 0.1f;
        float downTime = 0.1f;

        // 1) 커지기
        float t = 0f;
        while (t < upTime)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 1.3f, t / upTime);
            judgeText.transform.localScale = defaultScale * scale;
            yield return null;
        }

        // 2) 원래 크기로 돌아오기
        t = 0f;
        while (t < downTime)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(1.3f, 1f, t / downTime);
            judgeText.transform.localScale = defaultScale * scale;
            yield return null;
        }

        judgeText.transform.localScale = defaultScale;
    }
}
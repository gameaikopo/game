using UnityEngine;

public class TimingJudge : MonoBehaviour
{
    public static TimingJudge Instance;

    // DDAController에서 참조하는 변수명과 반드시 동일해야 함!
    public float PerfectThreshold = 0.05f;   // Perfect 판정 폭 (50ms)
    public float GoodThreshold = 0.10f;      // Good 판정 폭 (100ms)


    /// <summary>
    /// beat 오차값(diff)을 받아서
    /// Perfect / Good / Miss 문자열을 반환하는 함수
    /// </summary>
    void Awake()
    {
        Instance = this;
    }

    public string Judge(float beatDiff)
    {
        beatDiff = Mathf.Abs(beatDiff);

        if (beatDiff <= PerfectThreshold)
            return "Perfect";

        if (beatDiff <= GoodThreshold)
            return "Good";

        return "Miss";
    }
}

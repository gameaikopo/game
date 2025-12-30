using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DDAController : MonoBehaviour
{
    public static DDAController Instance;

    // 판정 기록
    public int perfectCount = 0;
    public int goodCount = 0;
    public int missCount = 0;

    // 최근 정확도(diff)
    public List<float> recentDiffs = new List<float>();

    private float timer = 0f;
    private float checkInterval = 2f;

    void Awake()
    {
        Instance = this;
    }


    // ======================================================================
    // 🎮 강제 난이도 테스트 (항상 입력 체크)
    // ======================================================================
    void Update()
    {
        // -----------------------------
        // F1 = 난이도 ↑
        // -----------------------------
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("🔧 Forced: Difficulty Increase");

            IncreaseDifficulty();

            if (DifficultyUI.Instance != null)
                DifficultyUI.Instance.ShowDifficulty("Increase");
        }

        // -----------------------------
        // F2 = 난이도 ↓
        // -----------------------------
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("🔧 Forced: Difficulty Decrease");

            DecreaseDifficulty();

            if (DifficultyUI.Instance != null)
                DifficultyUI.Instance.ShowDifficulty("Decrease");
        }

        // -----------------------------
        // F3 = 유지 (변화 없음)
        // -----------------------------
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log("🔧 Forced: Difficulty Maintain");

            if (DifficultyUI.Instance != null)
                DifficultyUI.Instance.ShowDifficulty("Maintain");
        }



        // ==================================================================
        // ⏱ 자동 난이도 조정 (2초마다 1회)
        // ==================================================================
        timer += Time.deltaTime;

        if (timer >= checkInterval)
        {
            timer = 0f;

            string state = GetDifficultyState();

            if (state == "Increase")
                IncreaseDifficulty();
            else if (state == "Decrease")
                DecreaseDifficulty();

            if (DifficultyUI.Instance != null)
                DifficultyUI.Instance.ShowDifficulty(state);
        }
    }


    // ======================================================================
    // 판정 기록
    // ======================================================================
    public void RecordJudge(string result, float diff)
    {
        if (result == "Perfect") perfectCount++;
        else if (result == "Good") goodCount++;
        else if (result == "Miss") missCount++;

        recentDiffs.Add(Mathf.Abs(diff));

        if (recentDiffs.Count > 20)
            recentDiffs.RemoveAt(0);
    }


    // ======================================================================
    // 평균 정확도 계산
    // ======================================================================
    public float GetAccuracyLevel()
    {
        if (recentDiffs.Count == 0)
            return 999f;

        float sum = 0f;
        foreach (float d in recentDiffs)
            sum += d;

        return sum / recentDiffs.Count;
    }


    // ======================================================================
    // 난이도 상태 평가
    // ======================================================================
    public string GetDifficultyState()
    {
        float acc = GetAccuracyLevel();

        if (acc <= 0.03f) return "Increase";
        if (acc <= 0.06f) return "Maintain";
        return "Decrease";
    }


    // ======================================================================
    // 난이도 증가
    // ======================================================================
    public void IncreaseDifficulty()
    {
        if (NoteSpawner.Instance != null)
        {
            NoteSpawner.Instance.noteSpeed -= 0.2f;

            NoteSpawner.Instance.ApplyNewSpeedToSpawnedNotes(
                NoteSpawner.Instance.noteSpeed
            );
        }

        if (TimingJudge.Instance != null)
        {
            TimingJudge.Instance.PerfectThreshold *= 0.95f;
            TimingJudge.Instance.GoodThreshold *= 0.97f;
        }

        Debug.Log("<color=yellow>난이도 상승</color>");
    }


    // ======================================================================
    // 난이도 감소
    // ======================================================================
    public void DecreaseDifficulty()
    {
        if (NoteSpawner.Instance != null)
        {
            NoteSpawner.Instance.noteSpeed += 0.2f;

            NoteSpawner.Instance.ApplyNewSpeedToSpawnedNotes(
                NoteSpawner.Instance.noteSpeed
            );
        }

        if (TimingJudge.Instance != null)
        {
            TimingJudge.Instance.PerfectThreshold *= 1.05f;
            TimingJudge.Instance.GoodThreshold *= 1.03f;
        }

        Debug.Log("<color=cyan>난이도 하락</color>");
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private SpriteRenderer sprite;

    [SerializeField]
    private ScreenFlash screenFlash;

    private bool isHitting = false;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // BASS 입력 처리
        if (Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.F))
        {
            HandleHit(NoteType.BASS);
        }

        // TREBLE 입력 처리
        if (Input.GetKeyDown(KeyCode.J) ||
            Input.GetKeyDown(KeyCode.K) ||
            Input.GetKeyDown(KeyCode.L))
        {
            HandleHit(NoteType.TREBLE);
        }
    }


    // ======================================================================
    // 🎯 리듬 판정 처리
    // ======================================================================
    private void HandleHit(NoteType type)
    {
        isHitting = true;
        StartCoroutine(Hitting());

        Note note = FindClosestNote(type);

        if (note == null)
        {
            // 노트 없음 → Miss 처리할지 말지?
            if (JudgeUI.Instance != null)
                JudgeUI.Instance.ShowJudge("Miss");

            return;
        }

        float currentBeat = (float)Conductor.Instance.GetSongBeat();
        float diff = currentBeat - note.targetBeat;
        float absDiff = Mathf.Abs(diff);

        // 🎯 안전장치: 너무 멀리 있는 노트는 무시
        if (absDiff > TimingJudge.Instance.GoodThreshold * 2f)
        {
            if (JudgeUI.Instance != null)
                JudgeUI.Instance.ShowJudge("Miss");

            if (DDAController.Instance != null)
                DDAController.Instance.RecordJudge("Miss", absDiff);

            return;
        }

        // 🎯 판정
        string result = TimingJudge.Instance.Judge(diff);
        Debug.Log($"Judge: {result} (diff: {diff})");

        // 🎯 UI 표시
        if (JudgeUI.Instance != null)
            JudgeUI.Instance.ShowJudge(result);

        // 🎯 DDA 기록
        if (DDAController.Instance != null)
            DDAController.Instance.RecordJudge(result, diff);

        // 🎯 Perfect/Good = 노트 제거
        if (result != "Miss")
        {
            if (screenFlash != null)
                screenFlash.Flash();

            note.Death();
        }
    }


    // ======================================================================
    // 🎯 가장 가까운 노트 찾기
    // ======================================================================
    private Note FindClosestNote(NoteType type)
    {
        Note[] notes = FindObjectsOfType<Note>();

        Note closest = null;
        float minDiff = float.MaxValue;
        float currentBeat = (float)Conductor.Instance.GetSongBeat();

        foreach (var note in notes)
        {
            if (note == null) continue;
            if (note.GetNoteType() != type) continue;

            float diff = Mathf.Abs(currentBeat - note.targetBeat);

            // 너무 멀리 있는 노트는 후보로 고려하지 않음
            if (diff > 1f) continue;

            if (diff < minDiff)
            {
                minDiff = diff;
                closest = note;
            }
        }

        return closest;
    }


    // ======================================================================
    // 🎨 입력 색상 변화
    // ======================================================================
    IEnumerator Hitting()
    {
        UpdateHitColor();
        yield return new WaitForSeconds(0.05f);
        isHitting = false;
        UpdateHitColor();
    }

    private void UpdateHitColor()
    {
        sprite.color = isHitting ? Color.blue : Color.white;
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        // 사용 안 함
    }
}
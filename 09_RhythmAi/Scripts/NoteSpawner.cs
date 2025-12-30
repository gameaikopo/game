using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// MIDI 데이터를 읽어서 노트, 쉼표, 마디선을 화면에 생성하는 스크립트.
/// DDAController에서 노트 속도를 변경하더라도
/// 모든 노트의 속도가 올바르게 업데이트되도록 구조를 개선한 버전.
/// </summary>
public class NoteSpawner : MonoBehaviour
{
    // --------------------------------------------------
    // ⭐ Singleton
    // --------------------------------------------------
    public static NoteSpawner Instance;

    void Awake()
    {
        Instance = this;
        // (선택) 씬이 바뀌어도 유지하고 싶으면 활성화
        // DontDestroyOnLoad(gameObject);
    }

    // --------------------------------------------------
    // 1. PREFABS
    // --------------------------------------------------
    [SerializeField] private GameObject note;
    public float noteSpeed = -4f;   // DDA 적용 대상

    [SerializeField] private GameObject measure;
    [SerializeField] private GameObject trebleScore;
    [SerializeField] private GameObject bassScore;

    // --------------------------------------------------
    // 2. SPRITES (음표 + 쉼표)
    // --------------------------------------------------
    [SerializeField] private Sprite wholeNoteSprite;
    [SerializeField] private Sprite halfNoteSprite;
    [SerializeField] private Sprite quarterNoteSprite;
    [SerializeField] private Sprite eighthNoteSprite;

    [SerializeField] private Sprite wholeRestSprite;
    [SerializeField] private Sprite halfRestSprite;
    [SerializeField] private Sprite quarterRestSprite;
    [SerializeField] private Sprite eighthRestSprite;

    // --------------------------------------------------
    // 3. 위치 계산 관련 데이터
    // --------------------------------------------------
    [SerializeField] private float noteStartOffset = -6.3f;
    [SerializeField] private float measureStartOffset;

    private float noteHeightOffset = 0.1f;
    private float trebleScoreHeight;
    private float bassScoreHeight;

    private float scoreStep = 0.5f;  // 8분음표 단위로 스캔
    private bool runOnce = true;

    private float spawnDistanceMultiplier;

    // 생성된 모든 노트 저장 → DDA 적용시 속도 업데이트용
    private List<Note> spawnedNotes = new List<Note>();


    // --------------------------------------------------
    // Start()
    // --------------------------------------------------
    void Start()
    {
        measureStartOffset = noteStartOffset - 1f;

        RecalculateSpawnMultiplier();  // noteSpeed 기반 거리 계산

        trebleScoreHeight = trebleScore.transform.position.y;
        bassScoreHeight = bassScore.transform.position.y;
    }


    // --------------------------------------------------
    // spawnDistanceMultiplier 재계산
    // --------------------------------------------------
    public void RecalculateSpawnMultiplier()
    {
        float bps = Conductor.Instance.GetBpm() / 60f;  // beat per second
        spawnDistanceMultiplier = Mathf.Abs(noteSpeed) / bps;
    }


    // --------------------------------------------------
    // Update()
    // --------------------------------------------------
    void Update()
    {
        if (runOnce)
        {
            runOnce = false;
            SpawnAllNotes(NoteType.TREBLE, trebleScoreHeight);
            SpawnAllNotes(NoteType.BASS, bassScoreHeight);
        }
    }


    // --------------------------------------------------
    // MIDI 기반 노트 생성
    // --------------------------------------------------
    private void SpawnAllNotes(NoteType noteType, float scoreHeight)
    {
        List<MidiNote> midiNotes = Conductor.Instance.GetMidiNotes(noteType);

        int index = 0;

        for (float scorePosition = 0.0f;
             scorePosition < Conductor.Instance.GetFinalBeat();
             scorePosition += scoreStep)
        {
            // 음표 생성 구간
            if (index < midiNotes.Count - 1 &&
                scorePosition == midiNotes[index].Position)
            {
                // 쉼표 생성
                CreateRest(
                    midiNotes[index].Position + midiNotes[index].Length,
                    midiNotes[index + 1].Position,
                    scoreHeight
                );

                // 음표 생성
                CreateNote(
                    scorePosition,
                    midiNotes[index].Length,
                    scoreHeight,
                    noteType
                );

                index++;
            }

            // ---------------------------
            // 마디선 생성
            // ---------------------------
            if (scorePosition % Conductor.Instance.GetTimeSig().Num == 0)
            {
                GameObject bar = Instantiate(
                    measure,
                    new Vector3(
                        measureStartOffset + (scorePosition * spawnDistanceMultiplier),
                        scoreHeight,
                        0
                    ),
                    Quaternion.identity
                );

                bar.GetComponent<Note>().SetSpeed(noteSpeed);
            }
        }
    }


    // --------------------------------------------------
    // 실제 노트 생성
    // --------------------------------------------------
    private void CreateNote(float scorePosition, float currentNoteLength, float scoreHeight, NoteType noteType)
    {
        float roundedLength = RoundLength(currentNoteLength);

        Sprite sprite = quarterNoteSprite;
        if (roundedLength == Conductor.Instance.GetTimeSig().WHOLE) sprite = wholeNoteSprite;
        else if (roundedLength == Conductor.Instance.GetTimeSig().HALF) sprite = halfNoteSprite;
        else if (roundedLength == Conductor.Instance.GetTimeSig().QUARTER) sprite = quarterNoteSprite;
        else if (roundedLength == Conductor.Instance.GetTimeSig().EIGHTH) sprite = eighthNoteSprite;

        GameObject obj = Instantiate(
            note,
            new Vector3(
                noteStartOffset + (scorePosition * spawnDistanceMultiplier),
                noteHeightOffset + scoreHeight,
                0
            ),
            Quaternion.identity
        );

        Note n = obj.GetComponent<Note>();
        obj.GetComponent<SpriteRenderer>().sprite = sprite;

        n.SetSpeed(noteSpeed);
        n.SetNoteType(noteType);

        // ★ 판정 기준 beat 저장
        n.targetBeat = scorePosition;

        spawnedNotes.Add(n);
    }


    // --------------------------------------------------
    // 쉼표 생성
    // --------------------------------------------------
    private bool CreateRest(float endOfCurrentNote, float startOfNextNote, float scoreHeight)
    {
        float restDur = startOfNextNote - endOfCurrentNote;
        float rounded = RoundLength(restDur);

        if (rounded <= 0) return false;

        Sprite sprite = wholeRestSprite;
        if (rounded == Conductor.Instance.GetTimeSig().HALF) sprite = halfRestSprite;
        else if (rounded == Conductor.Instance.GetTimeSig().QUARTER) sprite = quarterRestSprite;
        else if (rounded == Conductor.Instance.GetTimeSig().EIGHTH) sprite = eighthRestSprite;

        GameObject obj = Instantiate(
            note,
            new Vector3(
                noteStartOffset + (endOfCurrentNote * spawnDistanceMultiplier),
                noteHeightOffset + scoreHeight,
                0
            ),
            Quaternion.identity
        );

        obj.GetComponent<SpriteRenderer>().sprite = sprite;
        obj.GetComponent<Note>().SetSpeed(noteSpeed);

        return true;
    }


    // --------------------------------------------------
    // 길이 반올림
    // --------------------------------------------------
    private float RoundLength(float num)
    {
        num *= 4;
        num = Mathf.Round(num);
        num /= 4;
        return num;
    }


    // --------------------------------------------------
    // ★ 난이도 조절 시 모든 노트 속도 + spawnDistanceMultiplier 동기화
    // --------------------------------------------------
    public void ApplyNewSpeedToSpawnedNotes(float newSpeed)
    {
        noteSpeed = newSpeed;

        RecalculateSpawnMultiplier();

        foreach (var n in spawnedNotes)
        {
            if (n != null)
                n.SetSpeed(newSpeed);
        }
    }
}
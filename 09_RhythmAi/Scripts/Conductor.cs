using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// MIDI 노트 하나를 표현하는 구조체
/// - Bar, Beat, Tick: 음악적 위치
/// - Position: 실제 비트 단위 위치 (float)
/// - Length: 노트 길이
/// </summary>
public struct MidiNote
{
    public int Bar;
    public int Beat;
    public int Tick;
    public float Position;
    public float Length;

    public MidiNote(int bar, int beat, int tick, float notePosition, float length)
    {
        Bar = bar;
        Beat = beat;
        Tick = tick;
        Position = notePosition;
        Length = length;
    }
}

public struct TimeSig
{
    public int Num;
    public int Denom;
    public float WHOLE;
    public float HALF;
    public float QUARTER;
    public float EIGHTH;
    public float SIXTEENTH;

    // 현재 프로젝트에서는 모든 단위가 4분표 기반이라 Denom = 4로 고정된 상태
    public TimeSig(int num, int denom)
    {
        Num = num;
        // TODO fix denom being 2 sometimes
        denom = 4;
        Assert.AreEqual(denom, 4);
        Denom = denom;
        // 한 마디 전체 길이와 비트 단위 길이 계산
        WHOLE = num;
        HALF = 2.0f;
        QUARTER = 1.0f;
        EIGHTH = QUARTER / 2.0f;
        SIXTEENTH = EIGHTH / 2.0f;
    }
}

public enum NoteType
{
    TREBLE, // 높은 음역대(오른손)
    BASS // 낮은 음역대(왼손)
};

/// <summary>
/// 🎶 Conductor
/// 리듬게임의 핵심 "타이밍 관리 시스템"
/// - 노래의 진행 시간(songTime)
/// - 현재 비트 위치(songPositionInBeats)
/// - BPM 기반 beat 계산
/// - MIDI 노트 목록 보유
/// 모든 노트 판정의 기준이 되는 클래스
/// </summary>

public class Conductor : MonoBehaviour
{
    //Conductor instance
    public static Conductor Instance; // 싱글톤 형태로 전역 접근 가능하게 함

    private AudioSource musicSource; // 실제 음악 AudioSource

    // MIDI로부터 읽어온 노트 리스트
    private List<MidiNote> trebleMidiNotes; 
    private List<MidiNote> bassMidiNotes;
    private float ticksperQuarterNote;  // 1박자당 MIDI tick 수
    private TimeSig timeSig;   // 박자표
    private float finalBeat;  // 마지막 노트의 비트 위치

    // 노래 진행 시간 관련 변수들
    private double previousFrameTime;   // 이전 프레임의 dspTime
    private double lastReportedPlayheadPosition = 0;  // AudioSource가 보고한 실제 재생 위치
    private double songTime; // DSP 기반으로 계산된 실제 시간
    private double songPositionInBeats;  // songTime을 BPM으로 나눈 현재 비트 위치

    // 노래 속성
    [SerializeField]
    private double songBpm;  // BPM
    private double secPerBeat;  // 1비트당 시간(sec)
    // firstBeatOffset accounts for small silences before the first beat of the song in the audio file.
    [SerializeField]
    private double firstBeatOffset;  // 오디오 파일 시작 전 공백 시간(0~0.2초 사이 자주 존재)

    private bool hasStarted = false;  // 노래가 재생 중인지 여부

    private float correctThreshold = 0.3f;  // 판정 허용 범위 (비트 기준)


    void Awake()
    {
        Instance = this;  // 싱글톤 설정
    }

    void Start()
    {
        musicSource = GetComponent<AudioSource>();  // AudioSource 가져오기
        secPerBeat = 60f / songBpm;   // BPM → 1박자 시간 변환
    }

    void Update()
    {
        // 노래가 시작했을 때만 타이밍 계산
        if (hasStarted)
        {
            // DSP 기반 시간 증가
            // AudioSettings.dspTime → 더 정확한 오디오 기반 시간
            songTime += AudioSettings.dspTime - previousFrameTime - firstBeatOffset; // TODO fix firstbeatoffset
            previousFrameTime = AudioSettings.dspTime;
            
            // AudioSource가 재생 위치 보고하면 DSP 기반 시간과 보정
            if (musicSource.time != lastReportedPlayheadPosition)
            {
                songTime = (songTime + musicSource.time) / 2;
                lastReportedPlayheadPosition = musicSource.time;
            }
            
            // 노래 시간 → 비트 위치 변환
            songPositionInBeats = songTime / secPerBeat;
        }
    }

    /// <summary>
    /// StartSong()
    /// - 음악 재생 시작
    /// - DSP 시간 초기화
    /// - 계산 루틴 활성화
    /// </summary>
    public void StartSong()
    {
        musicSource.Play();
        //song started
        previousFrameTime = AudioSettings.dspTime;
        songTime = 0;
        hasStarted = true;
    }

    /* DEPRECATED
    public bool IsQuarterBeat()
    {
        float intSongPositionInBeats = (int) Math.Round (songPositionInBeats, 0) + 0.5f;
        if (songPositionInBeats < intSongPositionInBeats + correctThreshold && songPositionInBeats > intSongPositionInBeats - correctThreshold)
        {
            Debug.Log (songPositionInBeats);
            return true;
        }
        return false;
    }*/
    
    /// <summary>
    /// 특정 노트 타입(TREBLE/BASS)에 대해 타이밍이 맞았는지 검사
    /// currentBeat = 현재 비트 위치
    /// midiNote.Position 주변 correctThreshold 범위 안이면 true
    /// </summary>
    public bool CheckHit(NoteType type)
    {
        var midiNotes = new List<MidiNote>();
        if (type == NoteType.TREBLE)
            midiNotes = trebleMidiNotes;
        else if (type == NoteType.BASS)
            midiNotes = bassMidiNotes;
        else
            Debug.LogError("Error: Conductor.cs CheckHit() invalid NoteType");
        double currentBeat = songPositionInBeats;
        foreach (MidiNote midiNote in midiNotes)
        {
            if (currentBeat > midiNote.Position + correctThreshold)
            {
                //midiNotes.Remove (midiNote); // TODO figure out a way to remove notes after they have been passed
            }
            if (currentBeat < midiNote.Position + correctThreshold && currentBeat > midiNote.Position - correctThreshold)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// GETTERS AND SETTERS
    /// </summary>

    public float GetBpm()
    {
        return (float)songBpm;
    }

    public void SetBpm(float newBpm)
    {
        songBpm = (double)newBpm;
    }

    public double GetAudioSourceTime()
    {
        return musicSource.time;
    }

    public double GetSongTime()
    {
        return songTime;
    }

    public double GetSongBeat()
    {
        return songPositionInBeats;
    }

    public List<MidiNote> GetMidiNotes(NoteType type)
    {
        if (type == NoteType.TREBLE)
            return trebleMidiNotes;
        else if (type == NoteType.BASS)
            return bassMidiNotes;
        Debug.LogError("Error: Conductor.cs GetMidiNotes() invalid NoteType");
        return new List<MidiNote>();
    }

    public void SetMidiNotes(List<MidiNote> newTrebleList, List<MidiNote> newBassList)
    {
        trebleMidiNotes = newTrebleList;
        bassMidiNotes = newBassList;
    }

    public float GetTicksPerQuarterNote()
    {
        return ticksperQuarterNote;
    }

    public void SetTicksperQuarterNote(float newTicksperQuarterNote)
    {
        ticksperQuarterNote = newTicksperQuarterNote;
    }

    public TimeSig GetTimeSig()
    {
        return timeSig;
    }

    public void SetTimeSig(TimeSig newTimeSig)
    {
        timeSig = newTimeSig;
    }

    public float GetFinalBeat()
    {
        return finalBeat;
    }

    public void SetFinalBeat(float newfinalTick)
    {
        finalBeat = newfinalTick;
    }

}

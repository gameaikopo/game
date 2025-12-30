using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 리듬게임의 노트 오브젝트
/// - x축 이동
/// - 목표 비트(targetBeat) 저장
/// - 노트 종류(TREBLE/BASS)
/// - Death()로 파괴 + 파티클 생성
/// </summary>
public class Note : MonoBehaviour
{
    [SerializeField]
    private float speed = -4f; 
    // 노트 이동 속도 (기본: 왼쪽으로 이동)

    [SerializeField]
    private GameObject deathParticles;
    // 노트 파괴 시 생성될 파티클

    public float targetBeat;
    // ++ 이 노트가 맞춰야 하는 정확한 비트 (Conductor 기준)

    private NoteType noteType;
    // TREBLE 또는 BASS

    private Rigidbody2D rb;
    // 실제 이동 담당 물리 컴포넌트


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 필요 시 노트 상태 확인 UI 추가 가능
    }

    private void FixedUpdate()
    {
        // 일정한 속도로 x축 이동
        rb.linearVelocity = new Vector2(speed, 0f);
    }

    /// <summary>
    /// 노트 제거 (Perfect/Good 판정 시)
    /// 파티클 생성 후 오브젝트 삭제
    /// </summary>
    public void Death()
    {
        if (deathParticles != null)
        {
            Instantiate(deathParticles, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// DDA(난이도 조절)로 속도 동적으로 변경 가능
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    /// <summary>
    /// MIDI에서 받은 노트 타입 지정
    /// </summary>
    public void SetNoteType(NoteType type)
    {
        noteType = type;
    }

    /// <summary>
    /// NoteType(TREBLE/BASS) 반환
    /// </summary>
    public NoteType GetNoteType()
    {
        return noteType;
    }
}
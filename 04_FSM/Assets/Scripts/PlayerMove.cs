// ==============================================
// 🎮 PlayerMove.cs
// 키보드 입력(WASD / 방향키)을 받아서
// 플레이어 오브젝트를 이동시키는 기본 스크립트
// ==============================================

using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    // 🔸 이동 속도 조절 변수
    // Inspector에서 직접 값(예: 5)을 변경할 수 있음
    public float speed = 5f;

    // =========================================
    // ▶ Update()
    // 매 프레임마다 자동으로 호출되는 Unity 기본 함수
    // 키보드 입력을 실시간으로 감지하고 이동을 처리
    // =========================================
    void Update()
    {
        // 1️) 입력 감지
        // "Horizontal" → A/D 또는 ←/→ 키
        // "Vertical"   → W/S 또는 ↑/↓ 키
        // GetAxis는 -1.0 ~ +1.0 사이의 값을 부드럽게 반환함
        float h = Input.GetAxis("Horizontal"); // 좌(-1) ↔ 우(+1)
        float v = Input.GetAxis("Vertical");   // 뒤(-1) ↔ 앞(+1)

        // 2️) 이동 방향 벡터 계산
        // X축은 좌우, Z축은 앞뒤 방향
        // Y축은 0으로 고정 (지면 위를 이동하므로)
        Vector3 dir = new Vector3(h, 0, v);

        // 3️) 실제 이동 처리
        // dir 벡터 방향으로 speed 속도로 이동
        // Time.deltaTime을 곱해 프레임 속도와 상관없이 일정한 속도로 움직이게 함
        transform.Translate(dir * speed * Time.deltaTime);

        // ※ Translate()는 현재 위치에서 지정한 방향(dir)만큼 이동시키는 함수
        // ex) dir = (1,0,0) → 오른쪽으로 이동
    }
}
using UnityEngine;
using UnityEngine.AI; // 🔹 NavMeshAgent를 사용하기 위한 네임스페이스 (AI 경로 탐색용)

// ==============================================
// 🎮 EnemyFSM : 적(Enemy)의 행동 상태를 FSM 방식으로 제어하는 스크립트
// ==============================================
public class EnemyFSM : MonoBehaviour
{
    // 🔸 FSM의 상태를 정의하는 열거형 (Enum)
    //     - Idle   : 대기 상태 (플레이어를 찾지 못함)
    //     - Chase  : 추격 상태 (플레이어를 발견함)
    //     - Attack : 공격 상태 (플레이어가 가까이 있음)
    private enum State { Idle, Chase, Attack }

    // 🔸 현재 적이 어떤 상태에 있는지를 저장하는 변수
    private State currentState;

    // 🔸 플레이어의 위치를 참조하기 위한 Transform 변수 (Inspector에서 드래그로 연결)
    public Transform player;

    // 🔸 Unity의 내비게이션 시스템(NavMesh)으로 이동을 제어하기 위한 컴포넌트
    private NavMeshAgent agent;

    // 🔸 공격 거리 (이 거리 안에 플레이어가 들어오면 Attack 상태로 전환)
    public float attackRange = 2f;

    // 🔸 감지 거리 (이 거리 안에 플레이어가 들어오면 Chase 상태로 전환)
    public float sightRange = 7f;

    // 🔸 색상 변경용 Renderer (Attack 시 색상 변경)
    private Renderer rend;

    // 🔸 원래 색상 저장 (Idle, Chase 상태일 때 복구용)
    private Color originalColor;

    // ======================================================
    // ▶ Start() : 게임이 시작될 때 한 번 실행되는 초기화 함수
    // ======================================================
    private void Start()
    {
        // ✅ 초기 상태를 Idle로 설정
        currentState = State.Idle;

        // ✅ NavMeshAgent, Renderer 컴포넌트를 가져옴
        agent = GetComponent<NavMeshAgent>();
        rend = GetComponent<Renderer>();

        // ✅ 현재 머티리얼의 원래 색상을 저장 (Attack 시 변경 후 복귀할 때 사용)
        if (rend != null)
        {
            originalColor = rend.material.color;
        }
    }

    // ======================================================
    // ▶ Update() : 매 프레임마다 호출되는 함수
    //              FSM의 핵심 루프 (현재 상태에 따라 동작 분기)
    // ======================================================
    private void Update()
    {
        switch (currentState)
        {
            case State.Idle:
                Idle();
                break;

            case State.Chase:
                Chase();
                break;

            case State.Attack:
                Attack();
                break;
        }
    }

    // ======================================================
    // ▶ Idle() : 대기 상태 (플레이어를 아직 발견하지 못함)
    // ======================================================
    void Idle()
    {
        Debug.Log("Idle 상태: 대기 중...");
        float distance = Vector3.Distance(transform.position, player.position);

        // 원래 색상으로 복귀 (공격이 끝난 후 복원)
        if (rend != null)
            rend.material.color = originalColor;

        // 플레이어가 감지 범위 안으로 들어오면 추격 상태로 전환
        if (distance < sightRange)
        {
            ChangeState(State.Chase);
        }
    }

    // ======================================================
    // ▶ Chase() : 추격 상태 (플레이어를 발견했을 때)
    // ======================================================
    void Chase()
    {
        Debug.Log("Chase 상태: 플레이어 추격 중...");

        // NavMeshAgent를 이용해 플레이어의 위치로 이동 (자동 경로 탐색)
        agent.SetDestination(player.position);

        float distance = Vector3.Distance(transform.position, player.position);

        // 추격 중에도 원래 색상 유지
        if (rend != null)
            rend.material.color = originalColor;

        // 플레이어가 공격 범위 안에 들어오면 공격 상태로 전환
        if (distance <= attackRange)
        {
            ChangeState(State.Attack);
        }
        // 플레이어를 놓치면 다시 Idle 상태로 전환
        else if (distance > sightRange)
        {
            ChangeState(State.Idle);
        }
    }

    // ======================================================
    // ▶ Attack() : 공격 상태 (플레이어가 매우 가까이 있을 때)
    // ======================================================
    void Attack()
    {
        Debug.Log("Attack 상태: 공격 중!");

        // 공격 중에는 플레이어를 바라보도록 회전
        transform.LookAt(player);

        // 공격 중일 때 노란색으로 색상 변경
        if (rend != null)
            rend.material.color = Color.yellow;

        float distance = Vector3.Distance(transform.position, player.position);

        // 플레이어가 다시 멀어지면 추격 상태로 복귀
        if (distance > attackRange)
        {
            ChangeState(State.Chase);
        }
    }

    // ======================================================
    // ▶ ChangeState() : 상태 전환 로직을 담당하는 함수
    // ======================================================
    void ChangeState(State newState)
    {
        Debug.Log($"상태 전환: {currentState} → {newState}");

        // 현재 상태를 새로운 상태로 변경
        currentState = newState;
    }
}

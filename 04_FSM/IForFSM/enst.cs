using UnityEngine;

public class EnemyFSM : MonoBehaviour
{
    // [1] 상태(State)를 정의하는 열거형(enum)
    // 적 캐릭터가 가질 수 있는 모든 행동 상태를 정의함
    // FSM의 핵심은 “상태를 명확히 구분하고 관리”하는 것
    private enum State 
    { 
        Idle,   // 대기 상태
        Chase,  // 추격 상태
        Attack  // 공격 상태
    }

    // 현재 적 캐릭터가 어떤 상태에 있는지를 저장하는 변수
    private State currentState;

    // 감지 관련 변수 (플레이어 감지 여부)
    // seePlayer: 플레이어가 시야 범위 안에 있는가?
    // inAttackRange: 플레이어가 공격 범위 안에 있는가?
    public bool seePlayer = false;
    public bool inAttackRange = false;


    // Unity의 Start()는 게임이 시작될 때 한 번만 실행됨
    // 초기 상태를 Idle로 설정하여 기본 동작을 시작
    private void Start()
    {
        currentState = State.Idle;
    }


    // Unity의 Update()는 매 프레임마다 자동으로 호출됨
    // FSM 구조에서는 이곳에서 “현재 상태별로 행동”을 수행함
    private void Update()
    {
        // switch문을 이용해 현재 상태(currentState)에 따라 행동 분기
        switch (currentState)
        {
            // [상태 1] Idle (대기 상태)
            case State.Idle:
                Debug.Log("Idle 상태: 주변 탐색 중");

                // 플레이어를 발견하면 → Chase 상태로 전환
                if (seePlayer)
                    ChangeState(State.Chase);
                break;

            // [상태 2] Chase (추격 상태)
            case State.Chase:
                Debug.Log("Chase 상태: 플레이어 추격 중");

                // 공격 범위에 들어오면 → Attack 상태로 전환
                if (inAttackRange)
                    ChangeState(State.Attack);

                // 플레이어를 놓치면 → 다시 Idle 상태로 전환
                else if (!seePlayer)
                    ChangeState(State.Idle);
                break;

            // [상태 3] Attack (공격 상태)
            case State.Attack:
                Debug.Log("Attack 상태: 공격 중");

                // 플레이어가 공격 범위를 벗어나면 → Chase 상태로 돌아감
                if (!inAttackRange)
                    ChangeState(State.Chase);
                break;
        }
    }


    // 상태 전환을 담당하는 함수
    // FSM의 가장 핵심적인 부분: 상태 변경이 일어날 때 실행됨
    private void ChangeState(State newState)
    {
        // 현재 상태에서 새로운 상태로 변경되었음을 콘솔에 표시
        Debug.Log($"상태 전환: {currentState} → {newState}");

        // 실제 상태 변경
        currentState = newState;
    }
}

// 상태별 코드가 독립적이며 구조가 깔끔함
// 상태 추가 시 enum + case만 추가하면 됨
// 상태 전환 흐름이 명시적(ChangeState()로 통제)
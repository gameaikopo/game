using UnityEngine;

public class EnemyIfElse : MonoBehaviour
{
    // 🔹 플레이어를 인식했는지를 나타내는 변수
    // true이면 적이 플레이어를 시야 내에서 보고 있는 상태
    public bool seePlayer = false;

    // 🔹 공격 범위 안에 플레이어가 들어왔는지를 나타내는 변수
    // true이면 공격 가능한 거리 안에 있음
    public bool inAttackRange = false;

    // Unity의 Update() 함수는 매 프레임마다 자동으로 호출됨
    // 여기서는 매 프레임마다 적의 행동 상태를 판단함
    private void Update()
    {
        // 1) 플레이어를 보지 못한 상태
        // → Idle 상태로 간주 (가만히 있거나 주변을 탐색 중)
        if (!seePlayer)
        {
            Debug.Log("Idle 상태: 주변 탐색 중");
        }

        // 2) 플레이어를 봤지만, 공격 범위에는 아직 들어오지 않음
        // → Chase 상태로 간주 (플레이어를 향해 이동 중)
        else if (seePlayer && !inAttackRange)
        {
            Debug.Log("Chase 상태: 플레이어 추격 중");
        }

        // 3) 플레이어를 보고 있으며, 공격 범위 안에 들어왔을 때
        // → Attack 상태로 간주 (공격 행동 수행)
        else if (seePlayer && inAttackRange)
        {
            Debug.Log("Attack 상태: 공격 중");
        }

        // ⚙️ 이 구조는 상태(State)라는 개념 없이 단순한 조건문으로만 행동을 결정함.
        // 즉, seePlayer와 inAttackRange라는 두 조건의 조합으로 모든 행동이 정의됨.
        // FSM과 동일한 동작을 하지만, 상태 구분 없이 단순히 조건문으로만 제어하는 구조입니다.
        // 빠르게 작성 가능하지만, 상태가 많아지면 가독성이 급격히 떨어집니다.

    }
}
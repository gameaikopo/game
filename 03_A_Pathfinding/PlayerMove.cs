using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 키보드 입력 받기
        moveInput.x = Input.GetAxisRaw("Horizontal"); // ← →
        moveInput.y = Input.GetAxisRaw("Vertical");   // ↑ ↓
        moveInput = moveInput.normalized; // 대각선 이동 속도 보정
    }

    void FixedUpdate()
    {
        // 물리 이동
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}
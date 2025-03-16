using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] InputReader m_Input;

    [SerializeField] float m_MoveSpeed;
    [SerializeField] float m_JumpSpeed;

    private float m_MoveDirection;
    private bool m_IsJumping;

    void Start()
    {
        
    }

     void OnEnable()
    {
        Debug.Log("Enabled Player Controller");
        m_Input.Move += HandleMove;
        m_Input.Jump += HandleJump;
        m_Input.JumpCancelled += HandleJumpCancelled;
    }

     void OnDisable()
    {
        Debug.Log("Disabled Player Controller");

        m_Input.Move -= HandleMove;
        m_Input.Jump -= HandleJump;
        m_Input.JumpCancelled -= HandleJumpCancelled;
    }

    void Update()
    {
        Move();
        Jump();
    }

    void HandleMove(float direction)
    {
        m_MoveDirection = direction;
    }
    void HandleJump()
    {
        m_IsJumping = true;
    }
    void HandleJumpCancelled()
    {
        m_IsJumping = false;
    }

    void Move()
    {
        transform.position += new Vector3(m_MoveDirection * m_MoveSpeed * Time.deltaTime, 0);
    }

    void Jump()
    {
        if (m_IsJumping)
        {
            transform.position += new Vector3(0, m_JumpSpeed * Time.deltaTime);
        }
    }
}
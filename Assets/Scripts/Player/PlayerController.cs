using System.Timers;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Refs")]
    public PlayerMovementData MovementData;
    [SerializeField] private InputReader m_InputReader;

    private StateMachine m_StateMachine;


    [SerializeField] private Vector2 m_MoveVelocity;
    private bool m_IsJumping;
    private bool m_IsFacingRight;

    void Awake()
    {
        m_IsFacingRight = (Mathf.Abs(transform.rotation.y) < 90.0f);

        m_StateMachine = new StateMachine();
    }

    void Start()
    {
        //var idleState = new PlayerIdleState();
        //var walkState = new PlayerWalkState();
        //var jumpState = new PlayerJumpState();
        //
        //m_StateMachine.SetState(idleState);
    }

    private void Update()
    {
        m_StateMachine.Update();
    }

    private void OnMove(float direction)
    {
        m_MoveVelocity.x = direction * 5.0f;
    }

    private bool jumpInput;
    private void OnJump()
    {
        m_MoveVelocity.y = 5.0f;
    }
    private void OnJumpCancelled()
    {
        m_MoveVelocity.y = 0.0f;
    }
    private void OnDash()
    {

    }


    private void OnEnable()
    {
        m_InputReader.ListenEvents(this);

    }

    void OnDisable()
    {
        m_InputReader.StopListening(this);
    }



}

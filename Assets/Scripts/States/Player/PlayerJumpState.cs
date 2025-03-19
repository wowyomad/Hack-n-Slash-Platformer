using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    private InputReader m_Input;
    public float JumpHeight = 10.0f;
    public float JumpDuration = 1.0f;
    public float JumpTimer = 0.0f;
    public float JumpVelocity = 0.0f;
    public float Acceleration = 0.0f;

    public PlayerJumpState(Player player) : base(player) { }

    public override void OnEnter(IState from)
    {
        Debug.Log("Player entered Jump State");
        JumpTimer = 0.0f;

        // Calculate the required velocity and acceleration to reach the height in the duration interval
        JumpVelocity = (2 * JumpHeight) / JumpDuration;
        Acceleration = (2 * JumpHeight) / (JumpDuration * JumpDuration);

        // Adjust the jump velocity and acceleration considering the gravity and max fall speed
        JumpVelocity += Mathf.Abs(Player.Controller.Gravity) * JumpDuration / 2;
        Acceleration += Mathf.Abs(Player.Controller.Gravity);
    }

    public override void OnExit()
    {
        // Reset the jump timer and velocity
        JumpTimer = 0.0f;
        JumpVelocity = 0.0f;
        Acceleration = 0.0f;
    }

    public override void Update()
    {
        if (JumpTimer > JumpDuration)
        {
            return;
        };

        // Apply vertical movement
        float verticalVelocity = JumpVelocity - (Acceleration * JumpTimer);
        verticalVelocity = Mathf.Max(verticalVelocity, Player.Controller.MaxFallSpeed);
        Player.Controller.Move(new Vector2(0.0f, verticalVelocity));

        JumpTimer += Time.deltaTime;
    }
}


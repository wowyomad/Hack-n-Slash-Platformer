using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    public bool DashFinished { get; private set; } = false;
    private CharacterController2D m_Controller;

    private float m_DashDistance;
    private float m_TimeToReachDistance;
    private float m_VelocityThreshold;

    private float m_DashStartTime;
    private float m_CalculatedDashSpeed;
    private float m_InitialVelocityY;
    private float m_InitialDashVelocityX;

    public PlayerDashState(Player player, float dashDistance, float timeToReachDistance, float velocityThreshold) : base(player)
    {
        m_Controller = player.GetComponent<CharacterController2D>();
        
        m_DashDistance = dashDistance;
        m_TimeToReachDistance = timeToReachDistance;
        m_VelocityThreshold = velocityThreshold;

        if (m_Controller == null)
        {
            Debug.LogError("CharacterController2D component not found on Player.", player);
        }

        if (m_TimeToReachDistance <= 0)
        {
            Debug.LogError("Time to reach distance for dash must be greater than 0.", player);
            m_CalculatedDashSpeed = 0;
        }
        else
        {
            m_CalculatedDashSpeed = m_DashDistance / m_TimeToReachDistance;
        }
    }

    public override void OnEnter()
    {
        DashFinished = false;
        m_DashStartTime = Time.time;
        m_InitialVelocityY = m_Controller.Velocity.y; 

        float dashDirection = Player.Input.Horizontal != 0 ? Mathf.Sign(Player.Input.Horizontal) : Player.FacingDirection;
        if (dashDirection == 0)
        {
            dashDirection = 1; 
        }
        
        m_InitialDashVelocityX = dashDirection * m_CalculatedDashSpeed;
        m_Controller.Velocity = new Vector2(m_InitialDashVelocityX, m_InitialVelocityY);
        m_Controller.ApplyGravity = false;
    }

    public override void OnExit()
    {
        if (!DashFinished)
        {
             m_Controller.Velocity = new Vector2(0f, m_InitialVelocityY);
        }
        m_Controller.ApplyGravity = true;
    }

    public override void OnUpdate()
    {
        if (DashFinished) return;

        float elapsedTime = Time.time - m_DashStartTime;
        float newXVelocity;

        if (m_TimeToReachDistance <= 0f)
        {
            newXVelocity = 0f;
            DashFinished = true;
        }
        else
        {
            float t = elapsedTime / m_TimeToReachDistance;
            newXVelocity = Mathf.Lerp(m_InitialDashVelocityX, 0f, t);

            if (Mathf.Abs(newXVelocity) < m_VelocityThreshold)
            {
                newXVelocity = 0f;
                DashFinished = true;
            }
        }

        m_Controller.Velocity = new Vector2(newXVelocity, m_InitialVelocityY);

        if (DashFinished && m_Controller.Velocity.x != 0f)
        {
            m_Controller.Velocity = new Vector2(0.0f, m_InitialVelocityY);
        }
    }
}
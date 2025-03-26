

using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttackingMeleeState : PlayerBaseState
{
    private IState m_PreviousState;
    private float m_FallbackDuration = 0.4375f;
    private float m_Timer = 0.0f;
    private Weapon m_Weapon;
    public PlayerAttackingMeleeState(Player player) : base(player)
    {
        m_Weapon = player.GetComponentInChildren<Weapon>();
    }

    public override void OnEnter(IState from)
    {
        m_PreviousState = from;
        m_Timer = 0.0f;

        TurnToCursor();

        Player.Animator.CrossFade(AttackMeleeAnimationHash, 0.0f);
        Player.Animation.OnAttackMeleeFinished.AddListener(OnAnimationFinished);
    }

    protected void TurnToCursor()
    {
        Vector3 mousePosition = Input.CursorPosition;
        Vector3 playerPosition = Camera.main.WorldToScreenPoint(Player.transform.position);
        Vector3 direction = mousePosition - playerPosition;
        Player.Flip(direction.x > 0 ? 1 : -1);
    }

    public override void OnExit()
    {
        
    }

    public override void Update()
    {
        if (m_Timer >= m_FallbackDuration)
        {
            ChangeState(Player.IdleState);
            return;
        }
        m_Timer += Time.deltaTime;
        Player.ApplyGravity();
    }

    protected void OnAnimationFinished()
    {
        IState newState = (IState)m_PreviousState.GetType().Instantiate(Player);
        ChangeState(newState);
    }
}
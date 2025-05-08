using TheGame;
using UnityEditor.UI;
using UnityEngine;

public class EnemyBaseState : IEnemyState
{
    public Enemy Self;
    public CharacterController2D Controller => Self.Controller;
    public float DistanceToPlayer => Vector2.Distance(Self.transform.position, Self.PlayerReference.transform.position);
    public bool PlayerIsOnSight => Self.transform.position.y - Self.PlayerReference.transform.position.y < 3.0f;
    public int DirectionToPlayer => (int)Mathf.Sign(Self.PlayerReference.transform.position.x - Self.transform.position.x);

    public EnemyBaseState(Enemy self)
    {
        Self = self;
    }

    public virtual void Update()
    {

    }
    public virtual void FixedUpdate()
    {
        
    }

    public virtual void Enter(IState from)
    {
        
    }

    public virtual void Exit()
    {
        
    }


    protected void ChangeState(IEnemyState state) => Self.StateMachine.ChangeState(state);
}

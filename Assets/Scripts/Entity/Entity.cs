using Unity.Behavior;
using UnityEngine;

namespace TheGame
{
    [SelectionBase]
    public abstract class Entity : MonoBehaviour, IHittable
    {
        public virtual bool IsAlive { get; protected set; }
        public bool IsDead => !IsAlive;

        public int FacingDirection { get; protected set; } = 1;

        public virtual event System.Action OnHit;

        virtual public HitResult TakeHit(HitData hitData)
        {
            return HitResult.Nothing;
        }

        public void Flip(int direction)
        {
            if (direction == 0)
            {
                return;
            }
            direction = direction > 0 ? 1 : -1;

            if (direction != FacingDirection)
            {
                FacingDirection = -FacingDirection;
                transform.localScale = new Vector3(FacingDirection, 1.0f, 1.0f);
            }
        }

        public void Flip(float direction)
        {
            if (direction == 0.0f)
            {
                return;
            }
            Flip(direction > 0.0f ? 1 : -1);
        }
    }

    
    [BlackboardEnum]
    public enum Affliction
    {
        None,
        Stun,
        Slow
    }
}

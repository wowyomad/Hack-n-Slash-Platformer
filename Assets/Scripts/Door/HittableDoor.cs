using System;
using UnityEngine;

namespace TheGame
{
    [RequireComponent(typeof(Collider2D))]
    [SelectionBase]
    public class HittableDoor : Door, IHittable
    {
        public event Action OnHit;

        [SerializeField] private LayerMask m_WhoAllowedToHit;

        public virtual HitResult TakeHit(HitData hitData)
        {
            if (((1 << hitData.Attacker.layer) & m_WhoAllowedToHit) != 0)
            {
                if (Open())
                {
                    OnHit?.Invoke();
                    EventBus<DoorOpenedWithHitEvent>.Raise(new DoorOpenedWithHitEvent
                    {
                        DoorPosition = transform.position
                    });
                }
            }
            return HitResult.Nothing;
        }
    }

}

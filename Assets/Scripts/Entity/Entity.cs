using System;
using UnityEngine;

namespace TheGame
{
    public abstract class Entity : MonoBehaviour, IHittable
    {
        public virtual bool IsAlive { get; protected set; }
        public bool IsDead => !IsAlive;

        virtual public HitResult TakeHit(HitData hitData)
        {
            return HitResult.Nothing;
        }
        public virtual event Action OnHit;
    }
}

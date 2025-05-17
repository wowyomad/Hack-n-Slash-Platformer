using System;
using UnityEngine;

namespace TheGame
{
    public interface IHittable
    {
        HitResult TakeHit();
        event Action OnHit;
    }

    public interface IHittableBetter
    {
        HitResult TakeHit(AttackData attackData);
        event Action OnHit;
    }
    public struct AttackData
    {
        public float StaggerForce;
        public bool IsBlockable;
        public bool IsParryable;
        public GameObject Attacker;
    }

    public enum HitResult
    {
        Nothing,
        Hit,
        Block,
        Parry
    }


}


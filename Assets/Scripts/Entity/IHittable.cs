using System;

namespace TheGame
{
    public enum HitResult
    {
        Nothing,
        Hit,
        Block,
        Parry
    }

    public interface IHittable
    {
        HitResult TakeHit();
        event Action OnHit;
    }
}


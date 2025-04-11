using System;

public interface IHittable
{
    void TakeHit();
    bool CanTakeHit { get; }
    event Action Hit;
}
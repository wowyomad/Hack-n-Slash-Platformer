using System;

public interface IHittable
{
    public void TakeHit();
    public bool CanTakeHit { get; }
    public event Action OnHit;
}
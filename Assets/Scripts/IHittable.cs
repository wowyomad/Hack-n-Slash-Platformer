using System;

public interface IHittable
{
    public void TakeHit();
    public event Action OnHit;
}
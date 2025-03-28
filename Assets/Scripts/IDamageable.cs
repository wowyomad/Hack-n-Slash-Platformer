using System;
using UnityEngine;

public interface IDamageable
{
    public void TakeDamage(float value, Vector2 direction);
    public event Action<float, Vector2> OnTakeDamage;
}

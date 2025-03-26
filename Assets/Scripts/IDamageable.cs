using System;
using UnityEngine;

public interface IDamageable
{
    public void TakeDamage(float value, Vector2 direction);
}

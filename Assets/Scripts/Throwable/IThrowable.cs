using System;
using UnityEngine;

public interface IThrowable
{
    public void Throw(Vector2 origin, Vector2 direction);
    event Action<GameObject> OnImpact;
}

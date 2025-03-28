using System;
using UnityEngine;

public interface IThrowable
{
    public void Throw(Vector2 origin, Vector2 target);
    event Action<GameObject, Vector2, Vector2> OnImpact;
    event Action<Vector2> OnThrow;
}

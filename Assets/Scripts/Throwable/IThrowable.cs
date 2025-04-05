using System;
using UnityEngine;

public interface IThrowable
{
    delegate void ThrownEvent (Vector2 direction);
    delegate void ImpactEVent(GameObject victim, Vector2 point, Vector2 normal);
    void Throw(Vector2 origin, Vector2 target);
    event ImpactEVent Impact;
    event ThrownEvent Thrown;
}

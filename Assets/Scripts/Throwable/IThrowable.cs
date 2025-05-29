using UnityEngine;

namespace TheGame
{
    public interface IThrowable
    {
        delegate void ThrownEvent(Vector2 direction);
        delegate void ImpactEvent(GameObject victim, Vector2 point, Vector2 normal);

        sealed void Throw(Vector2 origin, Vector2 target)
        {
            var direction = (target - origin).normalized;
            ThrowInDirection(origin, direction);
        }

        void ThrowInDirection(Vector2 origin, Vector2 direction);
        event ImpactEvent Impact;
        event ThrownEvent Thrown;
        Sprite Icon { get; }
    }

    public interface IThrowableWithThrower : IThrowable
    {
        void SetThrower(GameObject thrower);
    }
}
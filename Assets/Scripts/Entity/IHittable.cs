using UnityEngine;

namespace TheGame
{
    public interface IHittable
    {
        HitResult TakeHit(HitData attackData);
        public event System.Action OnHit;
    }

    [System.Serializable]
    public class HitData
    {
        public HitData(GameObject attacker)
        {
            Direction = Vector2.zero;
            StaggerForce = 0.0f;
            IsBlockable = false;
            IsParryable = false;
            Attacker = attacker;
        }
        public Vector2 Direction;
        public float StaggerForce;
        public bool IsBlockable;
        public bool IsParryable;
        public GameObject Attacker;
    }

}


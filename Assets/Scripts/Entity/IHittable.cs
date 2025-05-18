using UnityEngine;

namespace TheGame
{
    public interface IHittable
    {
        HitResult TakeHit(HitData attackData);
        public event System.Action OnHit;
    }

    [System.Serializable]
    public class HitData : ScriptableObject
    {
        public HitData(GameObject attacker)
        {
            HitDirection = Vector2.zero;
            StaggerForce = 0.0f;
            IsBlockable = false;
            IsParryable = false;
            Attacker = attacker;
        }
        public Vector2 HitDirection;
        public float StaggerForce;
        public bool IsBlockable;
        public bool IsParryable;
        public GameObject Attacker;
    }

    public enum HitResult
    {
        Nothing,
        Hit,
        Block,
        Parry
    }

}


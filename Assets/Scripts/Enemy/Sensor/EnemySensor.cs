using UnityEngine;
namespace TheGame
{
    namespace EnemySensor
    {
        [RequireComponent(typeof(Enemy))]
        public abstract class Sensor : MonoBehaviour
        {
            public Enemy Owner { get; private set; }
            public virtual void Initialize(Enemy owner)
            {
                Owner = owner;
            }
            public abstract bool Check(GameObject target);
        }
    }
}

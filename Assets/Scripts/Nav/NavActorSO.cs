using UnityEngine;

namespace Nav
{
    [CreateAssetMenu(fileName = "NavActor", menuName = "Nav2D/NavActor", order = 2)]

    public class NavActorSO : ScriptableObject
    {
        public float MinFallJumpDuration = 0.2f;
        public float MaxFallJumpDuration = 1.0f;
        public float BaseSpeed = 6.0f;
    }
}

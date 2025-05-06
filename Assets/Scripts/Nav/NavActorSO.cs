using NUnit.Framework;
using UnityEngine;

namespace Nav
{
    [CreateAssetMenu(fileName = "NavActor", menuName = "Nav2D/NavActor", order = 2)]
    public class NavActorSO : ScriptableObject
    {
        [Header("Movement")]
        public float BaseSpeed = 6.0f;

        public float Gravity { get; private set; }
        public float MaxGravityVelocity { get; private set; }
        public float JumpVelocity { get; private set; }

        public float MaxJumpDistance = 5.0f;
        public float MaxJumpHeight = 5.0f;
        public float TimeToJumpApex = 0.5f;
        public float MaxGravityScale = 1.5f;



        private void Awake()
        {
            RecalculateGravity();
        }

        private void OnValidate()
        {
            RecalculateGravity();
        }

        private void RecalculateGravity()
        {
            Gravity = -(2 * MaxJumpHeight) / Mathf.Pow(TimeToJumpApex, 2);
            MaxGravityVelocity = Gravity * MaxGravityScale;
            JumpVelocity = -Gravity * TimeToJumpApex;
        }
    }
}
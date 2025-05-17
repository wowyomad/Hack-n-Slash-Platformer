using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private string m_AnimationClipPrefix = "Player_";

    public static readonly int IdleAnimationHash = Animator.StringToHash("Idle");
    public static readonly int JumpAnimationHash = Animator.StringToHash("Jump");
    public static readonly int WalkAnimationHash = Animator.StringToHash("Walk");
    public static readonly int RunAnimationHash = Animator.StringToHash("Run");
    public static readonly int AirAnimationHash = Animator.StringToHash("Air");
    public static readonly int DashAnimationHash = Animator.StringToHash("Dash");
    public static readonly int DeadAnimationHash = Animator.StringToHash("Dead");
    public static readonly int AttackMeleeAnimationHash = Animator.StringToHash("AttackMelee");
    private Dictionary<int, float> m_AnimationDurations = new();

    private AnimatorWrapper m_Animator;
    private Player m_Player;
    private InputReader m_Input;


    public float GetAnimationDuration(int animationHash)
    {
        return GetAnimationDuration(animationHash, out float _);
    }

    public float GetAnimationDuration(int animationHash, out float animationDuration)
    {

        if (m_AnimationDurations.TryGetValue(animationHash, out float duration))
        {
            animationDuration = duration;
        }
        else
        {
            Debug.LogWarning($"Animation with hash {animationHash} not found in durations dictionary.");
            animationDuration = 0.0f;
        }
        return animationDuration;
    }

    private void Awake()
    {
        m_Player = GetComponentInParent<Player>();

        if (!TryGetComponent(out Animator animator))
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("Animator component not found on Player or its children.", this);
            return;
        }


        m_Animator = new AnimatorWrapper(animator);
        m_AnimationDurations = animator.GetClipsDurations(m_AnimationClipPrefix);
    }

    private void LateUpdate()
    {
        m_Animator.Update();
    }

    private void Start()
    {
        m_Input = m_Player.Input;


    }

    private void OnEnable()
    {
        m_Player.OnPlayerIdle += OnIdle;
        m_Player.OnPlayerWalk += OnWalk;
        m_Player.OnPlayerJump += OnJump;
        m_Player.OnPlayerAir += OnAir;
        m_Player.OnPlayerAttack += OnAttack;
        m_Player.OnPlayerThrow += OnThrow;
        m_Player.OnPlayerStunned += OnStun;
        m_Player.OnPlayerDead += OnDie;
        m_Player.OnPlayerDash += OnDash;
    }
    private void OnDisable()
    {
        m_Player.OnPlayerIdle -= OnIdle;
        m_Player.OnPlayerWalk -= OnWalk;
        m_Player.OnPlayerJump -= OnJump;
        m_Player.OnPlayerAttack -= OnAttack;
        m_Player.OnPlayerThrow -= OnThrow;
        m_Player.OnPlayerStunned -= OnStun;
        m_Player.OnPlayerDead -= OnDie;
        m_Player.OnPlayerDash -= OnDash;
    }
    private void OnIdle()
    {
        m_Animator.CrossFade(IdleAnimationHash, 0.0f);
    }
    private void OnWalk()
    {
        m_Animator.CrossFade(WalkAnimationHash, 0.0f);
    }
    private void OnJump()
    {
        m_Animator.CrossFade(JumpAnimationHash, 0.0f);
    }
    private void OnAir()
    {
        m_Animator.CrossFade(AirAnimationHash, 0.0f);
    }
    private void OnAttack()
    {
        m_Animator.CrossFade(AttackMeleeAnimationHash, 0.0f);
    }
    private void OnThrow()
    {
        m_Animator.CrossFade(AttackMeleeAnimationHash, 0.0f);
    }
    private void OnStun()
    {
        m_Animator.CrossFade(IdleAnimationHash, 0.0f);
    }
    private void OnDash()
    {
        m_Animator.CrossFade(DashAnimationHash, 0.0f);
    }
    private void OnDie()
    {
        m_Animator.CrossFade(DeadAnimationHash, 0.0f);
    }
}


public static class AnimatorExtensions
{
    public static Dictionary<int, float> GetClipsDurations(this Animator animator, string prefix)
    {
        var durations = new Dictionary<int, float>();
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name.StartsWith(prefix))
            {
                string cleanName = clip.name.Replace(prefix, "");
                int hash = Animator.StringToHash(cleanName);
                durations[hash] = clip.length;
            }
        }
        return durations;
    }
}
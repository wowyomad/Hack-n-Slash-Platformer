using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnimation : MonoBehaviour
{
    public string AnimatoinClipPrefix = "Weapon_"; 
    private Animator m_Animator;
    private Weapon m_Weapon;
    private SpriteRenderer m_SpriteRenderer;

    public static readonly int AttackAnimationHash = Animator.StringToHash("Attack");

    private Dictionary<int, float> m_AnimationDurations;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
        m_Weapon = GetComponentInParent<Weapon>();

        m_AnimationDurations = m_Animator.GetClipsDurations(AnimatoinClipPrefix);
    }

    private void OnEnable()
    {
        m_Weapon.OnAttacked += PlayAnimation;
    }

    private void OnDisable()
    {
        m_Weapon.OnAttacked -= PlayAnimation;
    }



    private void PlayAnimation()
    {
        m_SpriteRenderer.enabled = true;
        m_Animator.Rebind();
        m_Animator.Update(0f);
        m_Animator.Play(AttackAnimationHash, 0, 0.0f);

        if (m_AnimationDurations.TryGetValue(AttackAnimationHash, out float duration))
        {
            StartCoroutine(DisableRendererAfterAnimation(duration));
        }
        else
        {
            Debug.LogWarning($"Animation duration for hash {AttackAnimationHash} not found", this);
            m_SpriteRenderer.enabled = false;
        }
    }

    private IEnumerator DisableRendererAfterAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        m_SpriteRenderer.enabled = false;
    }
}

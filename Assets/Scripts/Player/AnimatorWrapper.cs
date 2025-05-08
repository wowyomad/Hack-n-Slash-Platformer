using UnityEngine;


public class AnimatorWrapper
{
    public Animator Animator { get; private set; }

    public AnimatorWrapper(Animator animator)
    {
        Animator = animator;
    }

    private bool m_HasAnimation;
    private int m_NextAnimationHash = -1;
    private float m_NextTransitionDruation = 0.0f;

    public void CrossFade(int hash, float transitionDuration)
    {
        m_NextAnimationHash = hash;
        m_NextTransitionDruation = transitionDuration;
        m_HasAnimation = true;
    }

    public void Update()
    {
        if (m_HasAnimation)
        {
            Animator.CrossFade(m_NextAnimationHash, m_NextTransitionDruation);
            m_HasAnimation = false;
        }
    }
}

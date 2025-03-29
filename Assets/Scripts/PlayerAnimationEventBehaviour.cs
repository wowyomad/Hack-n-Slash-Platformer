using UnityEngine;
using UnityEngine.Events;

public class PlayerAnimationEventBehaviour : MonoBehaviour
{
    [SerializeField] public UnityEvent OnAttackMeleeEntered;
    [SerializeField] public UnityEvent OnAttackMeleeFinished;
    
    void Awake()
    {
        
    }

    void AttackMeleeEnter()
    {
        OnAttackMeleeEntered?.Invoke();
    }
    void AttackMeleeFinish()
    {
        OnAttackMeleeFinished?.Invoke();    
    }
}

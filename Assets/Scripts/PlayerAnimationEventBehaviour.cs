using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerAnimationEvents : MonoBehaviour
{
    public event Action AttackMeleeEntered;
    public event Action AttackMeleeFinished;
    
    void Awake()
    {
        
    }

    void AttackMeleeEnter()
    {
        AttackMeleeEntered?.Invoke();
    }
    void AttackMeleeFinish()
    {
        AttackMeleeFinished?.Invoke();    
    }
}

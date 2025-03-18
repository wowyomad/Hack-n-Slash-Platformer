using Unity.VisualScripting;
using UnityEngine;
public class PlayerWalkState : PlayerBaseState
{
    public override void OnEnter()
    {      
        Debug.Log("Player entered walk state");
    }

    public override void OnExit()
    {

    }
}
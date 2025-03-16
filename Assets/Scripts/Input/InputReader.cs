using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "InputReader")]
public class InputReader : ScriptableObject, GameInput.IGameplayActions, GameInput.IUIActions
{
    public event Action<float> Move;
    public event Action Jump;
    public event Action JumpHeld;
    public event Action JumpCancelled;
    public event Action Dash;
    public event Action Run;

    public event Action Pause;
    public event Action Resume;

    private GameInput m_GameInput;
    public void SetGameplay()
    {
        m_GameInput.Gameplay.Enable();
        m_GameInput.UI.Disable();
    }

    public void SetUI()
    {
        m_GameInput.UI.Enable();
        m_GameInput.Gameplay.Disable();
    }

     private void OnEnable()
    {
            if (m_GameInput == null)
        {
            m_GameInput = new GameInput();

            m_GameInput.Gameplay.SetCallbacks(this);
            m_GameInput.UI.SetCallbacks(this);

            SetGameplay();
        }
    }

    private void OnDisable()
    {
        m_GameInput.Gameplay.Disable();
        m_GameInput.UI.Disable();
    }


    #region interface implementation
    public void OnMove(InputAction.CallbackContext context)
    {
        Move?.Invoke(context.ReadValue<float>());
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (InputActionPhase.Performed == context.phase)
        {
            Jump?.Invoke();
        }

        if (InputActionPhase.Canceled == context.phase)
        {
            JumpCancelled?.Invoke();
        }
    }

    public void OnDash()
    {
        Dash?.Invoke(); 
    }

    public void OnRun()
    {
        Run?.Invoke();
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (InputActionPhase.Performed == context.phase)
        {
            Pause?.Invoke();
        }
    }

    public void OnResume(InputAction.CallbackContext context)
    {
        if (InputActionPhase.Performed == context.phase)
        {
            Resume?.Invoke();
        }
    }

    #endregion

}

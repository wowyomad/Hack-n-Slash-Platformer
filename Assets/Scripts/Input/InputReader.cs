using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "InputReader")]
public class InputReader : ScriptableObject, GameInput.IGameplayActions, GameInput.IUIActions
{

    public event Action<float> MoveEvent;
    public event Action JumpEvent;
    public event Action JumpCancelledEvent;

    public event Action PauseEvent;
    public event Action ResumeEvent;

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

    public void OnJump(InputAction.CallbackContext context)
    {
        if (InputActionPhase.Performed == context.phase)
        {
            JumpEvent?.Invoke();
        }

        if (InputActionPhase.Canceled == context.phase)
        {
            JumpCancelledEvent?.Invoke();
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<float>());
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (InputActionPhase.Performed == context.phase)
        {
            PauseEvent?.Invoke();
        }
    }

    public void OnResume(InputAction.CallbackContext context)
    {
        if (InputActionPhase.Performed == context.phase)
        {
            ResumeEvent?.Invoke();
        }
    }

    #endregion

}

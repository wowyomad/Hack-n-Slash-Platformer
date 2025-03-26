using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "InputReader")]
public class InputReader : ScriptableObject, GameInput.IGameplayActions, GameInput.IUIActions
{
    public event Action<float> Move;
    public event Action Jump;
    public event Action JumpCancelled;
    public event Action Dash;
    public event Action Run;
    public event Action AttackMelee;
    public event Action Throw;

    public event Action Pause;
    public event Action Resume;


    public bool IsJumpPressed {  get; private set; }
    public bool IsDashPressed { get; private set; }

    public float HorizontalMovement { get; private set; }
    public Vector2 CursorPosition { get; private set; }

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

    public void OnThrow(InputAction.CallbackContext context)
    {
        if(InputActionPhase.Performed == context.phase)
        {
            Throw?.Invoke();
        }
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
        HorizontalMovement = context.ReadValue<float>();
        if (HorizontalMovement != 0.0f)
        {
            Move?.Invoke(context.ReadValue<float>());
        }
    }

    public void OnMeleeAttack(InputAction.CallbackContext context)
    {
        if (InputActionPhase.Performed == context.phase)
        {
            AttackMelee?.Invoke();
        }
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

    public void OnCursor(InputAction.CallbackContext context)
    {
        CursorPosition = context.ReadValue<Vector2>();
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


    public void ListenEvents(object listener)
    {
        ModifyEventHandlers(listener, (eventInfo, action) => eventInfo.AddEventHandler(this, action));
    }

    public void StopListening(object listener)
    {
        ModifyEventHandlers(listener, (eventInfo, action) => eventInfo.RemoveEventHandler(this, action));
    }

    private void ModifyEventHandlers(object listener, Action<EventInfo, Delegate> modifyHandler)
    {
        foreach (var method in listener.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        {
            if (method.Name.StartsWith("On"))
            {
                var eventName = method.Name.Substring(2);
                var eventInfo = this.GetType().GetEvent(eventName);
                if (eventInfo != null)
                {
                    var parameters = method.GetParameters();
                    var eventHandlerType = eventInfo.EventHandlerType;
                    var eventHandlerInvokeMethod = eventHandlerType.GetMethod("Invoke");
                    var eventHandlerParameters = eventHandlerInvokeMethod.GetParameters();

                    if (parameters.Length == eventHandlerParameters.Length)
                    {
                        bool parametersMatch = true;
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            if (parameters[i].ParameterType != eventHandlerParameters[i].ParameterType)
                            {
                                parametersMatch = false;
                                break;
                            }
                        }

                        if (parametersMatch)
                        {
                            var action = Delegate.CreateDelegate(eventHandlerType, listener, method);
                            modifyHandler(eventInfo, action);
                        }
                    }
                }
            }
        }
    }

}

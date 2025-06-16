using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "InputReader")]
public class InputReader : ScriptableObject, GameInput.IGameplayActions, GameInput.IUIActions, GameInput.IDeathActions
{
    public event Action<float> Move;
    public event Action<float> MoveStared; //unused
    public event Action<float> MoveCancelled; //unused
    public event Action<float> Zoom;

    public event Action Jump;
    public event Action JumpCancelled;
    public event Action Dash;
    public event Action Run;
    public event Action Attack;
    public event Action Throw;
    public event Action Pause;
    public event Action Resume;
    public event Action Secret;
    public event Action ClimbDown;
    public event Action Restart;
    public event Action DebugGood;
    public event Action DebugBad;

    public event Action<Vector2> CursorMove;

    public bool IsJumpPressed { get; private set; }
    public bool IsDashPressed { get; private set; }

    public float Horizontal { get; private set; }
    public Vector2 CursorPosition { get; private set; }

    private GameInput m_GameInput;

    public static InputReader Load()
    {
        var instance = Resources.Load<InputReader>("Input/InputReader");
        if (instance == null)
        {
            Debug.LogError("InputReader not found in Resources folder.");
        }
        return instance;
    }


    public void SetGameplay()
    {
        m_GameInput.Gameplay.Enable();
        m_GameInput.UI.Disable();
        m_GameInput.Death.Disable();
    }

    public void SetUI()
    {
        m_GameInput.UI.Enable();
        m_GameInput.Gameplay.Disable();
        m_GameInput.Death.Disable();
    }

    public void SetDeath()
    {
        m_GameInput.Death.Enable();
        m_GameInput.UI.Disable();
        m_GameInput.Gameplay.Disable();
    }

    public void OnThrow(InputAction.CallbackContext context)
    {
        if (InputActionPhase.Performed == context.phase)
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
            m_GameInput.Death.SetCallbacks(this);
        }
    }

    private void OnDisable()
    {
        m_GameInput.Gameplay.Disable();
        m_GameInput.UI.Disable();
        m_GameInput.Death.Disable();
    }

    private void OnValidate()
    {
        var gameActions = Enum.GetValues(typeof(GameActions.ActionType));
        foreach (var action in gameActions)
        {
            var actionName = action.ToString();
            var eventField = GetType().GetEvent(actionName);
            if (eventField == null)
            {
                Debug.LogError($"Event for ActionType({actionName}) is missing");
                continue;
            }
        }
    }

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
            var attribute = method.GetCustomAttribute<GameActions.GameActionAttribute>();
            if (attribute != null)
            {
                var eventName = attribute.ActionType.ToString();
                var @event = GetEventInfo(eventName);
                if (@event == null)
                {
                    Debug.LogError($"No event found for action type: {eventName}");
                    continue;
                }

                if (!MethodSignatureMatchesEvent(method, @event))
                {
                    Debug.LogError($"Method signature does not match for action type: {eventName}");
                    continue;
                }

                var action = CreateDelegate(listener, method, @event);
                if (action != null)
                {
                    modifyHandler(@event, action);
                }
            }
        }
    }

    private bool MethodSignatureMatchesEvent(MethodInfo method, EventInfo eventInfo)
    {
        var parameters = method.GetParameters();
        var eventHandlerType = eventInfo.EventHandlerType;
        var eventHandlerInvokeMethod = eventHandlerType.GetMethod("Invoke");
        var eventHandlerParameters = eventHandlerInvokeMethod.GetParameters();

        if (parameters.Length != eventHandlerParameters.Length)
        {
            return false;
        }

        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].ParameterType != eventHandlerParameters[i].ParameterType)
            {
                return false;
            }
        }

        return true;
    }

    private EventInfo GetEventInfo(string eventName)
    {
        return GetType().GetEvent(eventName);
    }

    private Delegate CreateDelegate(object listener, MethodInfo method, EventInfo eventInfo)
    {
        try
        {
            return Delegate.CreateDelegate(eventInfo.EventHandlerType, listener, method);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create delegate for method: {method.Name}. Exception: {ex}");
            return null;
        }
    }

    #region interface implementation

    public void OnRestart(InputAction.CallbackContext context)
    {
        if (InputActionPhase.Performed == context.phase)
        {
            Restart?.Invoke();
        }
    }

    public void OnDeathRestart(InputAction.CallbackContext context)
    {
        if (InputActionPhase.Performed == context.phase)
        {
            Restart?.Invoke();
        }
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        Horizontal = context.ReadValue<float>();
        if (Horizontal != 0.0f)
        {
            Move?.Invoke(Horizontal);
        }
    }

    public void OnSecret(InputAction.CallbackContext context)
    {
        if (InputActionPhase.Performed == context.phase)
        {
            Secret?.Invoke();
        }
    }

    public void OnDebugGood(InputAction.CallbackContext context)
    {
        if (InputActionPhase.Performed == context.phase)
        {
            DebugGood?.Invoke();
        }
    }

    public void OnDebugBad(InputAction.CallbackContext context)
    {
        if (InputActionPhase.Performed == context.phase)
        {
            DebugBad?.Invoke();
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (InputActionPhase.Performed == context.phase)
        {
            Attack?.Invoke();
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

    public void OnDash(InputAction.CallbackContext context)
    {
        if (InputActionPhase.Performed == context.phase)
        {
            Dash?.Invoke();
        }
    }

    public void OnClimbDown(InputAction.CallbackContext context)
    {
        if (InputActionPhase.Performed == context.phase)
        {
            ClimbDown?.Invoke();
        }
    }



    public void OnCursor(InputAction.CallbackContext context)
    {
        CursorPosition = context.ReadValue<Vector2>();
        CursorMove?.Invoke(CursorPosition);
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

    public void OnZoom(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        if (value != 0.0f && InputActionPhase.Performed == context.phase)
        {
            Zoom?.Invoke(value > 0.0f ? 1.0f : -1.0f);
        }
    }
    #endregion
}

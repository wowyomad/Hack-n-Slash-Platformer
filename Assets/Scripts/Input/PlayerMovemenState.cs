using UnityEngine;

public class PlayerMovemenState
{
    private InputReader m_Input;

    public float HorizontalInput {  get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsJumping {  get; private set; }
    
    public PlayerMovemenState(InputReader input) { m_Input = input; }
     public void Listen()
    {
        Debug.Log("Enabled Player Controller");
        m_Input.Move += HandleMove;
        m_Input.Jump += HandleJump;
        m_Input.JumpCancelled += HandleJumpCancelled;
    }

     public void Stop()
    {
        Debug.Log("Disabled Player Controller");

        m_Input.Move -= HandleMove;
        m_Input.Jump -= HandleJump;
        m_Input.JumpCancelled -= HandleJumpCancelled;
    }

    void HandleMove(float direction)
    {
        HorizontalInput = direction;
    }
    void HandleJump()
    {
        IsJumping = true;
    }
    void HandleJumpCancelled()
    {
        IsJumping = false;
    }
}
using System;

namespace GameActions
{
    public class GameActionAttribute : Attribute
    {
        public ActionType ActionType { get; }
        public GameActionAttribute(ActionType actionType) => ActionType = actionType;
    }
    public enum ActionType
    {
        Move,
        Jump,
        JumpCancelled,
        Throw,
        Attack,
        Dash,
        Run
    }
}


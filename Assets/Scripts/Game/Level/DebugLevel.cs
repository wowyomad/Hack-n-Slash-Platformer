
using UnityEngine;

namespace TheGame
{
    [CreateAssetMenu(fileName = "DebugLevel", menuName = "Game/Levels/Debug Level")]
    public class DebugLevel : Level
    {
        InputReader m_Input;
        public override void OnLevelLoaded()
        {
            base.OnLevelLoaded();

            m_Input = InputReader.Load();
            m_Input.DebugGood += CompleteLevel;
        }

        private void CompleteLevel()
        {
            if (LevelStatus == Status.InProgress)
            {
                Debug.Log("Completing Debug level");
                EventBus<LevelFinishReachedEvent>.Raise(new LevelFinishReachedEvent());
            }
        }
    }
}

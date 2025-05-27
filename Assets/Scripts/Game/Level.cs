using System.Collections.Generic;
using UnityEngine;

namespace TheGame
{
    public class Level : ScriptableObject
    {
        public string ID = System.Guid.NewGuid().ToString();
        public string Name;
        public string Description;
        public bool Opened;
        public bool Completed;
        public string SceneName;
        public SceneReference SceneReference;
        public List<Challenge> Challenges = new List<Challenge>();
        public List<Level> NextLevels = new List<Level>();

#if UNITY_EDITOR
        private void OnEnable()
        {
            SceneReference.OnValueChanged += OnValidate;
        }
        private void OnDisable()
        {
            SceneReference.OnValueChanged -= OnValidate;
        }
#endif

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(ID))
            {
                ID = System.Guid.NewGuid().ToString();
            }
            if (SceneReference != null)
            {
                //get the name from the path
                SceneName = System.IO.Path.GetFileNameWithoutExtension(SceneReference.Path);
            }
        }
    }
}

using UnityEngine;

namespace TheGame
{
    [System.Serializable]
    public enum AbilityType
    {
        Passive,
        Active
    }
    public abstract class Ability : ScriptableObject, IAbility
    {
        public string ID = System.Guid.NewGuid().ToString();
        public string Name = "Ability";
        public string Description = "Description";
        public AbilityType Type;
        public Sprite ItemIcon;
        public bool Unlocked = false;

        public abstract void Apply(Player player);
        public abstract void Remove(Player player);

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(ID))
            {
                ID = System.Guid.NewGuid().ToString();
            }
        }
#endif
    }
}

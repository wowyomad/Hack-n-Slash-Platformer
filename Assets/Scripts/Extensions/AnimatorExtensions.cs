using System.Collections.Generic;
using UnityEngine;

namespace TheGame
{
    public static class AnimatorExtensions
    {
        public static Dictionary<int, float> GetClipsDurations(this Animator animator, string prefix)
        {
            var durations = new Dictionary<int, float>();
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name.StartsWith(prefix))
                {
                    string cleanName = clip.name.Replace(prefix, "");
                    int hash = Animator.StringToHash(cleanName);
                    durations[hash] = clip.length;
                }
            }
            return durations;
        }
    }
}

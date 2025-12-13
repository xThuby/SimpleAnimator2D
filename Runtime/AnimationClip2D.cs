using UnityEngine;

namespace Thuby.SimpleAnimator2D
{
    [CreateAssetMenu(fileName = "AnimationClip2D", menuName = "AnimationClip2D", order = 1)]
    public class AnimationClip2D : ScriptableObject
    {
        [HideInInspector]
        public Texture2D Texture;

        public Sprite[] Cells;
        public float FrameRate = 12;
        public float RangeStart = 0;
        public float RangeEnd = 1;
        public bool InvertRange = false;
        public AnimationStyle AnimationStyle;
        public bool Looping = true;
        public Transition[] Transitions;

        public float Length => 1.0f / FrameRate * Cells.Length;

        // Checks if this clip contains transitions to the specificd clip
        // If so returns that transition, otherwise return null.
        public Transition ContainsTransition(AnimationClip2D clip)
        {
            if (Transitions == null)
                return null;

            foreach (var transition in Transitions)
            {
                if (transition.Clip == clip)
                    return transition;
            }

            return null;
        }
    }
}

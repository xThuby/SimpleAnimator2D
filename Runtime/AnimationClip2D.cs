using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thuby.SimpleAnimator2D
{
    [CreateAssetMenu(fileName = "AnimationClip2D", menuName = "AnimationClip2D", order = 1)]
    public class AnimationClip2D : ScriptableObject
    {
        [HideInInspector]
        public Texture2D texture;

        public Sprite[] cells;
        public float frameRate = 12;
        public float rangeStart = 0;
        public float rangeEnd = 1;
        public bool invertRange = false;
        public AnimationStyle animationStyle;
        public bool looping = true;
        public Transition[] transitions;

        public float Length
        {
            get
            {
                return (1.0f / frameRate) * cells.Length;
            }
        }

        // Checks if this clip contains transitions to the specificd clip
        // If so returns that transition, otherwise return null.
        public Transition ContainsTransition(AnimationClip2D clip)
        {
            if (transitions != null)
            {
                foreach (var transition in transitions)
                {
                    if (transition.clip == clip)
                        return transition;
                }
            }

            return null;
        }
    }
}

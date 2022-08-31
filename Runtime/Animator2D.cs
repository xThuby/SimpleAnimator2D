using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thuby.SimpleAnimator2D
{
    public class Animator2D : MonoBehaviour
    {
        [SerializeField] private AnimationClip2D startingAnimation;
        private AnimationClip2D currentAnimation;
        public AnimationClip2D CurrentAnimation { get { return currentAnimation; } }

        private AnimationClip2D prevAnimation;
        private AnimationClip2D currentTransitionTo;

        private int currentFrame = 0;
        public int CurrentFrame { get { return currentFrame; } }

        private float frameTime;
        private float secondsPerFrame;

        private SpriteRenderer spriteRenderer;

        private bool reverse;

        private bool isPlaying = false;
        public bool IsPlaying { get { return isPlaying; } }

        private bool canAnimate = true;

        private bool inTransition = false;

        private float normalizedAnimationTime = 0;
        public float NormalizedAnimationTime { get { return normalizedAnimationTime; } }

        private float animationTime = 0;
        public float AnimationTime { get { return animationTime; } }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (startingAnimation != null)
            {
                SetAnimation(startingAnimation);
            }
        }

        private void Update()
        {
            if (currentAnimation != null && isPlaying && canAnimate)
            {
#if UNITY_EDITOR
                secondsPerFrame = 1.0f / currentAnimation.frameRate;
#endif
                frameTime += Time.deltaTime;
                animationTime += Time.deltaTime;
                normalizedAnimationTime = animationTime / currentAnimation.Length;

                if (frameTime > secondsPerFrame)
                {
                    AdvanceFrame();

                    frameTime = 0;
                    currentFrame = mod(currentFrame, currentAnimation.cells.Length);
                    spriteRenderer.sprite = currentAnimation.cells[currentFrame];
                }
            }
        }

        private void AdvanceFrame()
        {
            int spriteCount = currentAnimation.cells.Length;

            inTransition = false;

            switch (currentAnimation.animationStyle)
            {
                case AnimationStyle.Normal:
                    currentFrame++;
                    break;
                case AnimationStyle.PingPong:
                    if (reverse)
                    {
                        currentFrame--;
                    }
                    else
                    {
                        currentFrame++;
                    }

                    if ((!reverse && currentFrame == spriteCount - 1) || (reverse && currentFrame == 0))
                    {
                        reverse = !reverse;
                    }
                    break;
                case AnimationStyle.Random:
                    currentFrame = (int)Random.Range(0, spriteCount - 1);
                    break;
                case AnimationStyle.Transition:
                    if (currentFrame == currentAnimation.cells.Length - 1)
                        SetAnimation(currentTransitionTo);

                    currentFrame++;
                    inTransition = true;
                    break;
            }

            if (!currentAnimation.looping && currentFrame == spriteCount - 1)
                isPlaying = false;
        }

        private void SetAnimation(AnimationClip2D clip)
        {
            currentAnimation = clip;
            isPlaying = true;
            reverse = false;
            currentFrame = 0;
            frameTime = 0;
            secondsPerFrame = 1.0f / currentAnimation.frameRate;
            animationTime = 0;
            spriteRenderer.sprite = clip.cells[0];
        }

        private void TransitionAnimation(Transition transition, AnimationClip2D toClip)
        {
            AnimationClip2D transitionClip = ScriptableObject.CreateInstance<AnimationClip2D>();
            transitionClip.cells = transition.sprites;
            transitionClip.frameRate = transition.frameRate;
            transitionClip.animationStyle = AnimationStyle.Transition;

            currentTransitionTo = toClip;

            SetAnimation(transitionClip);

            inTransition = true;
        }

        private int mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        #region Public methods

        public void Play(AnimationClip2D clip, bool cancelSelf = false)
        {
            // Ignore if we're already playing that clip
            if ((clip != currentAnimation || currentAnimation == null || cancelSelf) && (!inTransition || currentTransitionTo != clip))
            {
                if (currentAnimation != null)
                {
                    Transition transition = currentAnimation.ContainsTransition(clip);
                    if (transition != null)
                        TransitionAnimation(transition, clip);
                    else
                        SetAnimation(clip);
                }
                else
                {
                    SetAnimation(clip);
                }
            }
        }

        #endregion

    }

    public enum AnimationStyle
    {
        Normal,
        PingPong,
        Random,
        [InspectorName(null)] Transition
    }

    [System.Serializable]
    public class Transition
    {
        public AnimationClip2D clip;
        public int frameRate;
        public Sprite[] sprites;
    }
}

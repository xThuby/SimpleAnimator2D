using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thuby.SimpleAnimator2D
{
    public class Animator2D : MonoBehaviour
    {
        [SerializeField] private AnimationClip2D startingAnimation;
        public AnimationClip2D StartingAnimation => startingAnimation;
        private AnimationClip2D currentAnimation;
        public AnimationClip2D CurrentAnimation => currentAnimation;

        public bool useUnscaledDeltaTime;
        private float deltaTime;

        private AnimationClip2D prevAnimation;
        private AnimationClip2D currentTransitionTo;

        private int currentFrame = 0;
        public int CurrentFrame { get { return currentFrame; } }

        private float frameTime;
        public float FrameTime { get { return frameTime; } }
        private float secondsPerFrame;

        private SpriteRenderer spriteRenderer;

        private bool reverse;

        private bool isPlaying = false;
        public bool IsPlaying { get { return isPlaying; } }

        private bool canAnimate = true;

        private bool inTransition = false;

        private bool initialised = false;

        private float normalizedAnimationTime = 0;
        public float NormalizedAnimationTime { get { return normalizedAnimationTime; } }

        private float animationTime = 0;
        public float AnimationTime { get { return animationTime; } }

        private Queue<AnimationClip2D> clipQueue = new Queue<AnimationClip2D>();
        public Queue<AnimationClip2D> ClipQueue { get { return clipQueue; } }

        private HashSet<AnimationEvent> events = new HashSet<AnimationEvent>();
        private HashSet<AnimationEvent> eventsToRemove = new HashSet<AnimationEvent>();

        private float rangeValue = 0;
        public float RangeValue { get => rangeValue; set => rangeValue = value; }

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (useUnscaledDeltaTime)
                deltaTime = Time.unscaledDeltaTime;
            else
                deltaTime = Time.deltaTime;

            if (!initialised)
            {
                if (startingAnimation != null)
                {
                    SetAnimation(startingAnimation);
                }
                initialised = true;
                return;
            }

            if (!isPlaying && clipQueue.Count > 0)
                Play(clipQueue.Dequeue(), true);

            if (currentAnimation != null && canAnimate)
            {
                // We want to handle animation differently if it's using a range.
                if (currentAnimation.animationStyle == AnimationStyle.Range)
                {
                    // Store bigger and smaller rangeValues
                    float minRange = 0;
                    float maxRange = 0;
                    if (currentAnimation.rangeEnd > currentAnimation.rangeStart)
                    {
                        maxRange = currentAnimation.rangeEnd;
                        minRange = currentAnimation.rangeStart;
                    }
                    else
                    {
                        maxRange = currentAnimation.rangeStart;
                        minRange = currentAnimation.rangeEnd;
                    }

                    // Clamp range value
                    rangeValue = Mathf.Clamp(rangeValue, minRange, maxRange);

                    // Map range to 0-1 space.
                    float normalRange = MapRange(rangeValue, minRange, maxRange, 0, 1);

                    // Select frame based on amount of cells and normalRange
                    currentFrame = (int)Mathf.Floor(normalRange * currentAnimation.cells.Length);
                    spriteRenderer.sprite = currentAnimation.cells[currentFrame];
                    normalizedAnimationTime = normalRange;
                }
                else
                {
                    if (isPlaying)
                    {
#if UNITY_EDITOR
                secondsPerFrame = 1.0f / currentAnimation.frameRate;
#endif
                        frameTime += deltaTime;
                        animationTime += deltaTime;
                        normalizedAnimationTime = animationTime / currentAnimation.Length;

                        if (frameTime > secondsPerFrame)
                        {
                            OnFrameEnd();

                            AdvanceFrame();

                            if (!isPlaying)
                                return;

                            frameTime = 0;
                            currentFrame = Mod(currentFrame, currentAnimation.cells.Length);
                            spriteRenderer.sprite = currentAnimation.cells[currentFrame];

                            OnFrameStart();
                        }
                    }
                }
            }
        }

        private void AdvanceFrame()
        {
            int spriteCount = currentAnimation.cells.Length;

            inTransition = false;

            if (currentFrame == spriteCount - 1)
            {
                if (clipQueue.Count > 0)
                {
                    Play(clipQueue.Dequeue(), true);
                    return;
                }

                if (!currentAnimation.looping)
                    isPlaying = false;
            }

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
                    currentFrame = (int)UnityEngine.Random.Range(0, spriteCount - 1);
                    break;
                case AnimationStyle.Transition:
                    if (currentFrame == currentAnimation.cells.Length - 1)
                        SetAnimation(currentTransitionTo);

                    currentFrame++;
                    inTransition = true;
                    break;
            }
        }

        private void OnFrameStart()
        {
            // Remove events slated for removal
            foreach (AnimationEvent e in eventsToRemove)
            {
                events.Remove(e);
            }
            eventsToRemove.Clear();

            // Check for events and call
            foreach (AnimationEvent e in events)
            {
                //Debug.Log(currentFrame);
                //Debug.Log(e);
                if (currentAnimation != e.clip)
                    continue;

                if (currentFrame != e.frame)
                    continue;

                e.callback?.Invoke(e, e.eventTag);
            }
        }

        private void OnFrameEnd()
        {
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
            OnFrameStart();
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

        private int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        private float MapRange(float value, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            float diffOutputRange = Math.Abs((outputMax - outputMin));
            float diffInputRange = Math.Abs((inputMax - inputMin));
            float convFactor = (diffOutputRange / diffInputRange);
            return (outputMin + (convFactor * (value - inputMin)));
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

        public void Hotswap(AnimationClip2D clip)
        {
            if (clip.Length != currentAnimation.Length)
            {
                Debug.LogError("Cannot hot swap to an animation with a different cell count or framerate");
                return;
            }

            currentAnimation = clip;
            spriteRenderer.sprite = currentAnimation.cells[currentFrame];
        }

        public void QueueAnimation(AnimationClip2D clip)
        {
            clipQueue.Enqueue(clip);
        }

        public void ClearQueue()
        {
            clipQueue.Clear();
        }

        public AnimationEvent AddEvent(AnimationClip2D clip, int frame, Action<AnimationEvent, string> callback, string eventTag = "")
        {
            AnimationEvent e = new AnimationEvent(clip, frame, callback, eventTag);
            events.Add(e);
            return e;
        }

        public AnimationEvent AddEvent(AnimationEvent animationEvent)
        {
            events.Add(animationEvent);
            return animationEvent;
        }

        public void RemoveEvent(AnimationEvent animEvent)
        {
            eventsToRemove.Add(animEvent);
        }

        #endregion

    }

    public struct AnimationEvent
    {
        public AnimationClip2D clip;
        public int frame;
        public Action<AnimationEvent, string> callback;
        public string eventTag;

        public AnimationEvent(AnimationClip2D _clip, int _frame, Action<AnimationEvent, string> _callback, string _eventTag)
        {
            clip = _clip;
            frame = _frame;
            callback = _callback;
            eventTag = _eventTag;
        }

        public override string ToString()
        {
            return "Clip: " + clip.name + ", Frame: " + frame + ", Param: " + eventTag;
        }
    }

    public enum AnimationStyle
    {
        Normal,
        PingPong,
        Random,
        Range,
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

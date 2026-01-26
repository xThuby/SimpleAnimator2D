using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Thuby.SimpleAnimator2D
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Animator2D : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private AnimationClip2D startingAnimation;

        private AnimationClip2D currentAnimation;

        private AnimationClip2D currentTransitionTo;

        private readonly Queue<AnimationClip2D> clipQueue = new();
        private readonly HashSet<AnimationEvent> events = new();
        private readonly HashSet<AnimationEvent> eventsToRemove = new();

        private int currentFrame = 0;
        private float frameTime;
        private float secondsPerFrame;
        private bool reverse;

        private bool isPlaying = false;
        private bool inTransition = false;

        private float elapsed = 0;
        private float animationSpeed = 1;

        private float rangeValue = 0;
        private float timescale = 1;

        [SerializeField]
        private bool useUnscaledDeltaTime;

        private SpriteRenderer spriteRenderer;

        #endregion // Private Fields

        #region Public Properties

        public AnimationClip2D StartingAnimation => startingAnimation;
        public AnimationClip2D CurrentAnimation => currentAnimation;

        public Queue<AnimationClip2D> ClipQueue => clipQueue;

        public int CurrentFrame => currentFrame;

        public float FrameTime => frameTime;

        public bool IsPlaying => isPlaying;

        public float Elapsed => elapsed;

        public float Progress =>
            (currentAnimation == null && currentAnimation.Length != 0)
                ? 0
                : elapsed / currentAnimation.Length;

        public bool UseUnscaledDeltaTime
        {
            get => useUnscaledDeltaTime;
            set => useUnscaledDeltaTime = value;
        }

        public float AnimationSpeed
        {
            get => animationSpeed;
            set => animationSpeed = value;
        }

        public float RangeValue
        {
            get => rangeValue;
            set => rangeValue = value;
        }

        public float Timescale
        {
            get => timescale;
            set
            {
                Assert.IsTrue(
                    value >= 0,
                    "Tried to set animator Timescale to a negative value, which is not allowed."
                );
                timescale = Mathf.Max(value, 0);
            }
        }

        #endregion // Public Properties


        #region Private Properties

        private float DeltaTime =>
            (UseUnscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime) * timescale;

        #endregion // Private Properties


        #region Unity Messages

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            if (startingAnimation != null)
            {
                SetAnimation(startingAnimation);
            }
        }

        private void Update()
        {
            if (!isPlaying && clipQueue.Count > 0)
                Play(clipQueue.Dequeue(), true);

            if (currentAnimation == null)
                return;

            // We want to handle animation differently if it's using a range.
            if (currentAnimation.AnimationStyle == AnimationStyle.Range)
            {
                UpdateAnimationRange();
            }
            else
            {
                UpdateAnimation();
            }
        }

        #endregion // Unity Messages


        #region Public Methods

        public void Play(AnimationClip2D clip, bool cancelSelf = false)
        {
            // Ignore if we're already playing that clip
            if (
                clip == currentAnimation && currentAnimation != null && !cancelSelf
                || inTransition && currentTransitionTo == clip
            )
                return;

            if (currentAnimation == null)
                SetAnimation(clip);
            else
            {
                Transition transition = currentAnimation.ContainsTransition(clip);
                if (transition != null)
                    TransitionAnimation(transition, clip);
                else
                    SetAnimation(clip);
            }
        }

        public void Hotswap(AnimationClip2D clip)
        {
            if (clip.Length != currentAnimation.Length)
            {
                Debug.LogError(
                    "Cannot hot swap to an animation with a different cell count or framerate"
                );
                return;
            }

            currentAnimation = clip;
            SetSprite(currentAnimation.Cells[currentFrame]);
        }

        public void QueueAnimation(AnimationClip2D clip)
        {
            clipQueue.Enqueue(clip);
        }

        public void ClearQueue()
        {
            clipQueue.Clear();
        }

        public AnimationEvent AddEvent(
            AnimationClip2D clip,
            int frame,
            Action<AnimationEvent, string> callback,
            string eventTag = ""
        )
        {
            AnimationEvent e = new(clip, frame, callback, eventTag);
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

        #endregion // Public Methods


        #region Private Methods

        private void UpdateAnimation()
        {
            if (!isPlaying)
                return;

            frameTime += DeltaTime * animationSpeed;
            elapsed += DeltaTime * animationSpeed;

            if (frameTime > secondsPerFrame)
            {
                OnFrameEnd();

                if (!AdvanceFrame())
                {
                    isPlaying = false;
                    return;
                }

                frameTime = 0;
                currentFrame = IMod(currentFrame, currentAnimation.Cells.Length);
                SetSprite(currentAnimation.Cells[currentFrame]);

                OnFrameStart();
            }
        }

        private void UpdateAnimationRange()
        {
            float maxRange = currentAnimation.RangeEnd;
            float minRange = currentAnimation.RangeStart;
            Assert.IsTrue(
                maxRange > minRange,
                "AnimationClip2D field rangeEnd must be larger than rangeStart"
            );

            // Clamp range value
            rangeValue = Mathf.Clamp(rangeValue, minRange, maxRange);

            // Map range to 0-1 space.
            float normalRange = Map(rangeValue, minRange, maxRange, 0, 1);

            if (currentAnimation.InvertRange)
                normalRange = 1 - normalRange;

            // Select frame based on amount of cells and normalRange
            currentFrame = Mathf.FloorToInt(normalRange * (currentAnimation.Cells.Length - 1));
            SetSprite(currentAnimation.Cells[currentFrame]);
        }

        private bool AdvanceFrame()
        {
            int spriteCount = currentAnimation.Cells.Length;

            inTransition = false;

            if (currentFrame == spriteCount - 1)
            {
                if (clipQueue.Count > 0)
                {
                    Play(clipQueue.Dequeue(), true);
                    return true;
                }

                if (!currentAnimation.Looping)
                    return false;
            }

            switch (currentAnimation.AnimationStyle)
            {
                case AnimationStyle.Normal:
                    currentFrame++;
                    break;
                case AnimationStyle.PingPong:
                    if (reverse)
                        currentFrame--;
                    else
                        currentFrame++;

                    if (
                        (!reverse && currentFrame == spriteCount - 1)
                        || (reverse && currentFrame == 0)
                    )
                    {
                        reverse = !reverse;
                    }
                    break;
                case AnimationStyle.Random:
                    currentFrame = UnityEngine.Random.Range(0, spriteCount - 1);
                    break;
                case AnimationStyle.Transition:
                    if (currentFrame == currentAnimation.Cells.Length - 1)
                        SetAnimation(currentTransitionTo);

                    currentFrame++;
                    inTransition = true;
                    break;
            }

            return true;
        }

        private void SetSprite(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
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
                // Debug.Log(currentFrame);
                // Debug.Log(e);
                if (currentAnimation != e.Clip)
                    continue;

                if (currentFrame != e.Frame)
                    continue;

                e.Callback?.Invoke(e, e.EventTag);
            }
        }

        private void OnFrameEnd() { }

        private void SetAnimation(AnimationClip2D clip)
        {
            currentAnimation = clip;
            isPlaying = true;
            reverse = false;
            currentFrame = 0;
            frameTime = 0;
            secondsPerFrame = 1.0f / currentAnimation.FrameRate;
            elapsed = 0;
            SetSprite(clip.Cells[0]);
            inTransition = false;
            animationSpeed = 1;
            OnFrameStart();
        }

        private void TransitionAnimation(Transition transition, AnimationClip2D toClip)
        {
            AnimationClip2D transitionClip = ScriptableObject.CreateInstance<AnimationClip2D>();
            transitionClip.Cells = transition.Sprites;
            transitionClip.FrameRate = transition.FrameRate;
            transitionClip.AnimationStyle = AnimationStyle.Transition;

            currentTransitionTo = toClip;

            SetAnimation(transitionClip);

            inTransition = true;
        }

        private int IMod(int x, int m)
        {
            return (x % m + m) % m;
        }

        private float Map(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        #endregion // Private Methods
    }

    public class AnimationEvent
    {
        public AnimationClip2D Clip;
        public int Frame;
        public Action<AnimationEvent, string> Callback;
        public string EventTag;

        public AnimationEvent(
            AnimationClip2D clip,
            int frame,
            Action<AnimationEvent, string> callback,
            string eventTag
        )
        {
            Clip = clip;
            Frame = frame;
            Callback = callback;
            EventTag = eventTag;
        }

        public override string ToString()
        {
            return $"Clip: {Clip.name}, Frame: {Frame}, Param: {EventTag}";
        }
    }

    public enum AnimationStyle
    {
        Normal,
        PingPong,
        Random,
        Range,

        [InspectorName(null)]
        Transition,
    }

    [Serializable]
    public class Transition
    {
        public AnimationClip2D Clip;
        public int FrameRate;
        public Sprite[] Sprites;
    }
}

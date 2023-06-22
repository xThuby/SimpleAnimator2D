# SimpleAnimator2D
A simple animator 2D sprite-based artwork for Unity3D.

## Installation
1. Open the Unity Package Manager.
2. Click the + icon and select "Add package from git URL..."
3. Paste in `https://github.com/xThuby/SimpleAnimator2D.git` and click add.

The package is now installed.

If you have any problems feel free to open an issue.

## How to use
Add the Animator2D component to a game object, just like the normal Unity animator.

Create an AnimationClip2D asset, and drag that asset into the Animator2D component's `startingAnimation` field.

## Animator2D

|Method|Explanation|
|------|-----------|
|`Play(AnimationClip2D clip, bool cancelSelf = false)`|Plays the given animation clip. If the currently playing animation clip has a transition to this one then it will play that transition first. If the currently playing animation clip is already set to `clip` it will be ignored, unless `cancelSelf` is true. `cancelSelf` is false by default.|
|`Hotswap(AnimationClip2D clip)`|Instantly swap out the animation playing without restarting the animation. Useful for changing between two different version of the same animation (I.E Aerial attack and grounded attack). `clip` must have the same cell count and framerate as the already playing clip.|
|`QueueAnimation(AnimationClip2D clip)`|Enqueues an animation. The animator will play this animation when the current animation finishes, even if the current animation is set to loop. When dequeued, the clip is played with `cancelSelf` as true.|
|`ClearQueue()`|Clears current animation queue.|
|`AddEvent(AnimationClip2D clip, int frame, Action<string> callback, string eventTag = "")`|Adds an animation event associated with the given `clip`. `callback` will be called, providing the tag as a string, when the animation reaches the given frame. Returns the `AnimationEvent` instance.|
|`AddEvent(AnimationEvent animationEvent)`|Same as above but you can pass in an instance of an `AnimationEvent` instead of the parameters.|
|`RemoveEvent(AnimationEvent animationEvent)`|Queues the given `animationEvent` for removal at the start of the next animation frame before events are called.|

## AnimationClip2D

|Field|Explanation|
|------|-----------|
|`cells`|The animation cells. Each cell is a frame of animation.|
|`frameRate`|Framerate of the animation.|
|`animationStyle`|How the animation will play. Can be one of `Normal`, `PingPong`, or `Random`.|
|`looping`|If the animation should loop.|
|`transition`|A list of animation clips that this animation clip can transition into. Each transition defines a target clip, the frameRate at which the transition should play at, and which cells (sprites) act as the transition between the this clip and the target clip.|
|`Length`|Not visible in the editor. Gets the length in seconds of the animation clip.|

|Methods|Explanation|
|------|-----------|
|`ContainsTransition(AnimationClip2D clip)`|Checks if this clip has a transition to the given clip.|

## TODO
* `range` animation style.
* Custom editor window for editing animation with preview.
* Aseprite file support.
* Example code snippets.
* Callback on animation finished.

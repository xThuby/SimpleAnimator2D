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
|`Play(AnimationClip2D clip, bool cancelSelf)`|Plays the given animation clip. If the currently playing animation clip has a transition to this one then it will play that transition first. If the currently playing animation clip is already set to `clip` it will be ignored, unless `cancelSelf` is true. `cancelSelf` is false by default.|

## AnimationClip2D

|Field|Explanation|
|------|-----------|
|`name`|String name of the animation clip.|
|`cells`|The animation cells. Each cell is a frame of animation.|
|`frameRate`|Framerate of the animation.|
|`animationStyle`|How the animation will play. Can be one of `Normal`, `PingPong`, or `Random`.|
|`looping`|If the animation should loop.|
|`transition`|A list of animation clips that this animation clip can transition into. Each transition defines a target clip, the frameRate at which the transition should play at, and which cells (sprites) act as the transition between the this clip and the target clip.|
|`Length`|Not visible in the editor. Gets the length in seconds of the animation clip.|

|Methods|Explanation|
|------|-----------|
|`ContainsTransition(AnimationClip2D clip)`|Checks if this clip has a transition to the given clip.|

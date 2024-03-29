using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Thuby.SimpleAnimator2D;

[CustomEditor(typeof(AnimationClip2D))]
public class AnimationClip2DEditor : Editor
{
    private AnimationClip2D anim;

    private bool isPlaying;
    private float frameTime;
    private int currentFrame;
    private bool reverse;

    private void OnEnable()
    {
        anim = (AnimationClip2D)target;
    }

    private void Update()
    {
    }

    public override void OnInspectorGUI()
    {
        // Anim cells
        // frame rate
        // animation style
        // looping
        // transitions array

        EditorGUI.BeginChangeCheck();
        anim.texture = (Texture2D)EditorGUILayout.ObjectField("Texture", anim.texture, typeof(Texture2D), false);
        if (EditorGUI.EndChangeCheck())
        {
            UpdateCells();
        }

        SerializedProperty cellsProperty = serializedObject.FindProperty("cells");
        serializedObject.Update();
        EditorGUILayout.PropertyField(cellsProperty, true);
        serializedObject.ApplyModifiedProperties();

        if (anim.animationStyle == AnimationStyle.Range)
        {
            anim.rangeStart = EditorGUILayout.FloatField("Start range", anim.rangeStart);
            anim.rangeEnd = EditorGUILayout.FloatField("End range", anim.rangeEnd);
            anim.invertRange = EditorGUILayout.Toggle("Invert range", anim.invertRange);
        }
        else
        {
            anim.frameRate = EditorGUILayout.FloatField("Frame Rate", anim.frameRate);
            anim.looping = EditorGUILayout.Toggle("Looping", anim.looping);
        }
        anim.animationStyle = (AnimationStyle)EditorGUILayout.EnumPopup("Animation Style", anim.animationStyle);

        SerializedProperty transitionsProperty = serializedObject.FindProperty("transitions");
        serializedObject.Update();
        EditorGUILayout.PropertyField(transitionsProperty, true);
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(anim);
        }

        // EditorGUI.BeginChangeCheck();
        // isPlaying = GUILayout.Toggle(isPlaying, "Preview", GUI.skin.button, GUILayout.Height(32));


        // if (!isPlaying)
        //     return;
        // PlayAnimation();
        // Repaint();
    }

    private void UpdateCells()
    {
        List<Sprite> sprites = new List<Sprite>();
        if (anim.texture != null)
        {
            Object[] data = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(anim.texture));
            if (data != null)
            {
                foreach (Object obj in data)
                {
                    if (obj.GetType() == typeof(Sprite))
                    {
                        sprites.Add(obj as Sprite);
                    }
                }
            }

            sprites.Sort((a, b) =>
            {
                return a.name.CompareTo(b.name);
            });

            if (sprites.Count > 0)
            {
                anim.cells = sprites.ToArray();
            }
        }
        // Clear cells if we set texture to none
        else
        {
            anim.cells = null;
        }
    }

    private void PlayAnimation()
    {
        var secondsPerFrame = 1.0f / anim.frameRate;
        frameTime += Time.deltaTime;

        if (frameTime > secondsPerFrame)
        {
            int spriteCount = anim.cells.Length;

            if (currentFrame == spriteCount - 1)
            {
                if (!anim.looping)
                    isPlaying = false;
            }

            switch (anim.animationStyle)
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
            }

            if (!isPlaying)
                return;

            frameTime = 0;
            currentFrame = Mod(currentFrame, anim.cells.Length);
            DrawOnGUISprite(anim.cells[currentFrame]);
        }
    }

    void DrawOnGUISprite(Sprite aSprite)
    {
    }

    private int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }
}

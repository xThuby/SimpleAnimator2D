using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Thuby.SimpleAnimator2D.Editor
{
    using Editor = UnityEditor.Editor;

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

        public override void OnInspectorGUI()
        {
            // Anim cells
            // frame rate
            // animation style
            // looping
            // transitions array

            EditorGUI.BeginChangeCheck();
            anim.Texture = (Texture2D)
                EditorGUILayout.ObjectField("Texture", anim.Texture, typeof(Texture2D), false);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateCells();
            }

            SerializedProperty cellsProperty = serializedObject.FindProperty("Cells");
            serializedObject.Update();
            EditorGUILayout.PropertyField(cellsProperty, true);
            serializedObject.ApplyModifiedProperties();

            if (anim.AnimationStyle == AnimationStyle.Range)
            {
                anim.RangeStart = EditorGUILayout.FloatField("Start range", anim.RangeStart);
                anim.RangeEnd = EditorGUILayout.FloatField("End range", anim.RangeEnd);
                anim.InvertRange = EditorGUILayout.Toggle("Invert range", anim.InvertRange);
            }
            else
            {
                anim.FrameRate = EditorGUILayout.FloatField("Frame Rate", anim.FrameRate);
                anim.Looping = EditorGUILayout.Toggle("Looping", anim.Looping);
            }
            anim.AnimationStyle = (AnimationStyle)
                EditorGUILayout.EnumPopup("Animation Style", anim.AnimationStyle);

            SerializedProperty transitionsProperty = serializedObject.FindProperty("Transitions");
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
            if (anim.Texture == null)
            {
                anim.Cells = null;
                return;
            }

            // Clear cells if we set texture to none
            Object[] data = AssetDatabase.LoadAllAssetsAtPath(
                AssetDatabase.GetAssetPath(anim.Texture)
            );

            if (data == null)
                return;

            List<Sprite> sprites = new();

            foreach (Object obj in data)
            {
                if (obj.GetType() == typeof(Sprite))
                {
                    sprites.Add(obj as Sprite);
                }
            }

            sprites.Sort(
                (a, b) =>
                {
                    return a.name.CompareTo(b.name);
                }
            );

            if (sprites.Count > 0)
            {
                anim.Cells = sprites.ToArray();
            }
        }
    }
}

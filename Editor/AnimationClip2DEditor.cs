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

        anim.texture = (Texture2D)EditorGUILayout.ObjectField("Texture", anim.texture, typeof(Texture2D), false);
        if (GUI.changed)
        {
            EditorUtility.SetDirty(anim.texture);
            UpdateCells();
        }

        SerializedProperty cellsProperty = serializedObject.FindProperty("cells");
        serializedObject.Update();
        EditorGUILayout.PropertyField(cellsProperty, true);
        serializedObject.ApplyModifiedProperties();

        anim.frameRate = EditorGUILayout.FloatField("Frame Rate", anim.frameRate);
        anim.animationStyle = (AnimationStyle)EditorGUILayout.EnumPopup("Animation Style", anim.animationStyle);
        anim.looping = EditorGUILayout.Toggle("Looping", anim.looping);

        SerializedProperty transitionsProperty = serializedObject.FindProperty("transitions");
        serializedObject.Update();
        EditorGUILayout.PropertyField(transitionsProperty, true);
        serializedObject.ApplyModifiedProperties();
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
                        Debug.Log((obj as Sprite).name);
                        sprites.Add(obj as Sprite);
                    }
                }
            }

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
}

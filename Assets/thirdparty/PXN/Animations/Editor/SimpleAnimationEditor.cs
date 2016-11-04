#if UNITY_EDITOR
// Copyright © 2016 Pixelnest Studio
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Pixelnest
{
  [CanEditMultipleObjects]
  [CustomEditor(typeof(SimpleAnimation))]
  public class SimpleAnimationEditor : Editor
  {
    private SimpleAnimation anim;

    void OnEnable()
    {
      anim = target as SimpleAnimation;
    }

    public override void OnInspectorGUI()
    {
      if (anim != null)
      {
        DrawDefaultInspector();

        if (Selection.objects.Length == 1)
        {
          // Magic button
          GUILayout.Label("Magic frames fill");

          if (anim.allowAutoUpdate && GUILayout.Button("Auto-fill"))
          {
            PXNMenuAssets.AutoFillAnimation(anim);
          }
          if (anim.allowAutoUpdate && GUILayout.Button("Reverse auto-fill"))
          {
            PXNMenuAssets.AutoFillAnimation(anim, true);
          }
          if (anim.allowAutoUpdate && GUILayout.Button("Cumulative auto-fill"))
          {
            PXNMenuAssets.AutoFillAnimation(anim, false, false);
          }
          if (anim.allowAutoUpdate && GUILayout.Button("Cumulative reverse auto-fill"))
          {
            PXNMenuAssets.AutoFillAnimation(anim, true, false);
          }

          GUILayout.Label("Quick name");
          GUILayout.BeginVertical();

          string[] options = { "default", "attack", "attackend", "hit", "action1", "action1end", "walk", "death" };
          int count = 0;
          const int max = 4;

          GUILayout.BeginHorizontal();

          foreach (var option in options)
          {
            if (GUILayout.Button(option))
            {
              anim.name = option;
              EditorUtility.SetDirty(anim);
            }

            count++;
            if (count == max)
            {
              count = 0;
              GUILayout.EndHorizontal();
              GUILayout.BeginHorizontal();
            }
          }

          // Fill with empty spaces
          for (int i = 0; i < max - count; i++)
          {
            GUILayout.Label("");
          }

          GUILayout.EndHorizontal();

          GUILayout.EndVertical();
        }
      }
    }
  }
}
#endif
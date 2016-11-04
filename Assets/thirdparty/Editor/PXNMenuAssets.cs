// Copyright © 2016 Pixelnest Studio
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

public class PXNMenuAssets
{
  [MenuItem("Assets/Create/Sprite Animations/Simple Sprite Animation")]
  public static void CreateSpriteFrameAnimation()
  {
    var anim = ScriptableObjectUtility.CreateAsset<SimpleAnimation>(Selection.activeObject.name);

    AutoFillAnimation(anim);
  }

  public static void AutoFillAnimation(SimpleAnimation anim, bool reverse = false, bool clean = true)
  {
    if (anim.allowAutoUpdate)
    {
      string path = AssetDatabase.GetAssetPath(anim.GetInstanceID());

      // Get the directory
      string dir = Path.GetDirectoryName(path);

      if (anim.name.ToLower().Contains("boucle") || anim.name.ToLower().Contains("static"))
      {
        anim.loop = true;
      }

      if (Directory.Exists(dir))
      {
        // List all sprites from the dir
        List<Sprite> sprites = new List<Sprite>();

        foreach (string file in Directory.GetFiles(dir))
        {
          foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(file))
          {
            if (asset is Sprite)
            {
              // Add them to the anim
              sprites.Add(asset as Sprite);
            }
          }
        }

        if (reverse)
        {
          sprites.Reverse();
        }

        if (clean == false)
        {
          sprites.InsertRange(0, anim.frames);
        }

        anim.frames = sprites.ToArray();

        EditorUtility.SetDirty(anim);

        AssetDatabase.SaveAssets();
        sprites = null;
      }
    }
  }


  [MenuItem("Assets/Sprite Animations/Generate or update animations", false, 7)]
  public static void CreateAnimForAllSubfolders()
  {
    string path = AssetDatabase.GetAssetPath(Selection.activeObject);
    string dir = path;

    GenerateOrUpdateAnimForDir(dir);
  }

  [MenuItem("PXN/Update animations")]
  public static void UpdateAllAnimations()
  {
    DirectoryInfo directory = new DirectoryInfo(Application.dataPath);
    FileInfo[] filesInfo = directory.GetFiles("*.asset", SearchOption.AllDirectories);

    foreach (var file in filesInfo)
    {
      string assetPath = file.FullName.Replace(@"\", "/").Replace(Application.dataPath, "Assets");

      var obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object)) as UnityEngine.Object;

      if (obj is SimpleAnimation)
      {
        SimpleAnimation anim = obj as SimpleAnimation;
        PXNMenuAssets.AutoFillAnimation(anim);

        anim = null;
      }

      obj = null;
    }
  }

  public static SimpleAnimation GenerateOrUpdateAnimForDir(string dir)
  {
    if (Directory.Exists(dir))
    {
      Debug.Log("Generating/updating animation for " + dir);

      // Extract filename
      string filename = Path.GetFileName(dir) + ".asset";

      // Existing?
      SimpleAnimation anim = (SimpleAnimation)AssetDatabase.LoadAssetAtPath(dir + "/" + filename, typeof(SimpleAnimation));

      // No, simply create
      if (anim == null)
      {
        // Make sure there is at least one image file
        bool spriteFound = false;
        foreach (var file in Directory.GetFiles(dir))
        {
          if (Path.GetExtension(file).ToLower().EndsWith("png"))
          {
            spriteFound = true;
            break;
          }
        }

        //foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(dir +"/"))
        //{
        //  Debug.Log("Asset found: " + asset);
        //  if (asset is Texture2D)
        //  {
        //    spriteFound = true;
        //    break;
        //  }
        //}

        if (spriteFound == false)
        {
          Debug.Log("No sprites found for " + dir + ", ignoring.");
        }
        else
        {
          Debug.Log("New anim: " + filename);

          anim = ScriptableObjectUtility.CreateAsset<SimpleAnimation>(dir, filename);
          PXNMenuAssets.AutoFillAnimation(anim);
        }
      }
      // Yes, win some time and update
      else
      {
        Debug.Log("Updating anim: " + filename);
        PXNMenuAssets.AutoFillAnimation(anim);
      }

      // Scan all subdirectories (ignore current one)
      foreach (string subdir in Directory.GetDirectories(dir))
      {
        GenerateOrUpdateAnimForDir(subdir);
      }

      return anim;
    }

    return null;
  }

  [MenuItem("Edit/Reset Playerprefs")]
  public static void DeletePlayerPrefs() { PlayerPrefs.DeleteAll(); }
}
#endif
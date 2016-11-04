// Copyright © 2013-2014 Pixelnest Studio
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.
using UnityEngine;

/// <summary>
/// Simple animation clip
/// </summary>
public class SimpleAnimation : ScriptableObject
{
  #region Members

  public new string name = "default";
  public float imagesPerSeconds = 25f;
  public bool loop = false;
  public bool randomFirstFrame = false;
  public Sprite[] frames;

  /// <summary>
  /// Allow global update via menu
  /// </summary>
  public bool allowAutoUpdate = true;

  #endregion

  #region Properties

  public float FrameDuration
  {
    get
    {
      return 1f / imagesPerSeconds;
    }
  }

  /// <summary>
  /// Duration, in seconds
  /// </summary>
  public float Duration
  {
    get
    {
      return FrameDuration * frames.Length;
    }
  }

  #endregion
}
#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using System.IO;

namespace Pixelnest.Steredenn
{
  /// <summary>
  /// The super useful screenshoting window
  /// </summary>
  public class ScreenshotWindow : EditorWindow
  {
    #region Menus
    
    [MenuItem("Window/Screenshoter")]
    public static void ShowWindow()
    {
      EditorWindow.GetWindow(typeof(ScreenshotWindow));
    }

    #endregion

    private bool firstFrameIngame = false;
    private bool enableScreenshoter = true;

    void OnEnable()
    {
      firstFrameIngame = true;
    }

    void OnGUI()
    {
      this.titleContent = new GUIContent("Screenshoter");

      EditorGUILayout.LabelField("Screenshoter", EditorStyles.boldLabel);

      bool screenshoterEnabler = EditorGUILayout.ToggleLeft("Enabled", enableScreenshoter);
      if (enableScreenshoter != screenshoterEnabler)
      {
        enableScreenshoter = screenshoterEnabler;
      }

      EditorGUI.indentLevel++;

      //--
      EditorGUILayout.Separator();
      //--

 
      EditorGUILayout.LabelField("Export", EditorStyles.boldLabel);

      EditorGUI.indentLevel++;

      GUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("Size");
      size = EditorGUILayout.IntField(size);
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("Path");
      screenshotLocation = EditorGUILayout.TextArea(screenshotLocation);
      GUILayout.EndHorizontal();

      if (Application.isPlaying)
      {
        if (GUILayout.Button("Take scene screenshot"))
        {
          TakeScreenshot();
        }
      }

      if (GUILayout.Button("Take rendered screenshot (1080p)"))
      {
        TakeRenderScreenshot();
      }

      EditorGUI.indentLevel--;

      //--
      EditorGUILayout.Separator();
      //--

      EditorGUI.indentLevel--;
    }
    
    private int size = 1;
    private string screenshotLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);

    private int resWidth = 1920;
    private int resHeight = 1080;

    private void TakeScreenshot()
    {
      string filename = ScreenShotName("unity");
      Application.CaptureScreenshot(filename, size);
      Debug.Log(string.Format("Screenshot saved to {0}", filename));
    }

    private void TakeRenderScreenshot()
    {
      Camera camera = Camera.main;
      if (camera == null) return;

      RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
      camera.targetTexture = rt;
      Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
      camera.Render();
      RenderTexture.active = rt;
      screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
      camera.targetTexture = null;
      RenderTexture.active = null;

      if (Application.isPlaying) Destroy(rt);
      else DestroyImmediate(rt);

      byte[] bytes = screenShot.EncodeToPNG();
      string filename = ScreenShotName("render");

      System.IO.File.WriteAllBytes(filename, bytes);
      Debug.Log(string.Format("Screenshot saved to {0}", filename));
    }

    public string ScreenShotName(string source)
    {
      return string.Format("{0}/screen_{1}_{2}.png",
                           screenshotLocation,
                           source,
                           System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }
  }

}
#endif
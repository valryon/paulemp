using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(QTEScript), true)]
public class QTEEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawPlayButton();

    base.OnInspectorGUI();

    DrawPlayButton();
  }

  private void DrawPlayButton()
  {
    if (Application.isPlaying)
    {
      GUILayout.Space(20);
      if (GUILayout.Button("Launch for debug"))
      {
        bool found = false;
        foreach (var p in FindObjectsOfType<PlayerScript>())
        {
          if (p.isLocalPlayer)
          {
            Debug.Log("QTE debug launch!", target);

            ((QTEScript)target).Launch(p, (r) =>
            {
              Debug.Log("QTE debug result: " + r, target);
            });

            found = true;
            break;
          }
        }

        if (!found)
        {
          Debug.LogError("Couldn't find a player for QTE...");
        }
      }
      GUILayout.Space(20);
    }
  }
}

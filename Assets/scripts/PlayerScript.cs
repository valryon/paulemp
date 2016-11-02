using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerScript : NetworkBehaviour
{
  public MeshRenderer model;
  public FirstPersonController fpsController;
  public Camera fpsCamera;

  void Start()
  {
    if (isLocalPlayer)
    {
      // Remove model
      Destroy(model.gameObject);
    }
    else
    {
      // Remove FPS Controller
      Destroy(fpsCamera.gameObject);
      Destroy(fpsController);
    }
  }
}

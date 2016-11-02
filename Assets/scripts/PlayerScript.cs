using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerScript : NetworkBehaviour
{
  #region Members

  public MeshRenderer model;
  public FirstPersonController fpsController;
  public Camera fpsCamera;

  #endregion

  #region Timeline

  void Start()
  {
    if (isLocalPlayer)
    {
      // Local player = remove model / FPS view
      Destroy(model.gameObject);
    }
    else
    {
      // Remote player = remove FPS Controller, keep model
      Destroy(fpsCamera.gameObject);
      Destroy(fpsController);
    }
  }

  void Update()
  {
    if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.E))
    {
      Interact();
    }
  }

  #endregion

  #region Methods

  private Ray raycast;

  private void Interact()
  {
    // Raycast from view
    raycast = new Ray(fpsCamera.transform.position, fpsCamera.transform.forward);

    foreach (var hit in Physics.RaycastAll(raycast))
    {
      Vector3 a = hit.collider.transform.position;
      a.y = 0;

      Vector3 b = this.transform.position;
      b.y = 0;

      if (Vector3.Distance(a, b) < 5f)
      {
        if (hit.collider.gameObject != this.gameObject)
        {
          Debug.Log("INTERACT " + hit.collider.gameObject);
          hit.collider.gameObject.SendMessage("Interact", this, SendMessageOptions.DontRequireReceiver);
        }
      }
    }
  }

  #endregion

  #region Debug

  void OnDrawGizmos()
  {
    Gizmos.DrawRay(raycast);
  }

  #endregion
}

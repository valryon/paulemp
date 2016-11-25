using UnityEngine;
using System.Collections;

public class CameraFacingBillboard : MonoBehaviour
{
  private SpriteRenderer sprite;
  private Camera cam;

  void Start()
  {
    sprite = GetComponent<SpriteRenderer>();
    if (sprite != null)
    {
      InvokeRepeating("FacePlayer", 1f, 0.1f);
    }
  }

  private void FacePlayer()
  {
    if (cam == null)
    {
      cam = Camera.main;
    }
    if (cam != null)
    {
      if (sprite != null && sprite.IsVisibleFrom(cam))
      {
        transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward,
            cam.transform.rotation * Vector3.up);
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
      }
    }
  }
}
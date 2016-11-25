using UnityEngine;
using System.Collections;

public class CameraFacingBillboard : MonoBehaviour
{
  private SpriteRenderer sprite;

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
    if (Camera.main != null && sprite != null && sprite.isVisible)
    {
      transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
          Camera.main.transform.rotation * Vector3.up);
      transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
    }
  }
}
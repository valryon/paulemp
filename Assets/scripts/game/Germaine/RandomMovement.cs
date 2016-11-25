using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class RandomMovement : NetworkBehaviour
{
  public float speed = 0.035f;

  private float targetRotation;
  private float nextRotationCooldown;
  private float collisionCooldown;

  [ServerCallback]
  void Start()
  {
    nextRotationCooldown = Random.Range(0, 5);

    targetRotation = transform.rotation.eulerAngles.y;
  }

  [ServerCallback]
  void Update()
  {
    transform.position += speed * transform.forward;

    collisionCooldown -= Time.deltaTime;
    nextRotationCooldown -= Time.deltaTime;
    if(nextRotationCooldown <0)
    {
      nextRotationCooldown = Random.Range(5, 15);

      targetRotation += Random.Range(30, 90);
    }
    targetRotation %= 360;

    transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, Mathf.Lerp(transform.rotation.eulerAngles.y, targetRotation, Time.deltaTime), transform.rotation.eulerAngles.z);
  }

  void OnCollisionEnter(Collision c)
  {
    if (collisionCooldown < 0)
    {
      collisionCooldown = 10;
      nextRotationCooldown = 5;
      targetRotation = targetRotation + Random.Range(90, 270);
    }
  }
}

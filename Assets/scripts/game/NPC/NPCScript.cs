using UnityEngine;

public class NPCScript : MonoBehaviour
{
  #region Members

  public float speed = 0.035f;
  public AudioClip[] randomSounds;
  public AudioClip[] interactSounds;

  private float targetRotation;
  private float nextRotationCooldown;
  private float collisionCooldown;

  private float randomSoundCooldown;

  #endregion

  #region Timeline

  void Start()
  {
    nextRotationCooldown = Random.Range(0, 5);
    randomSoundCooldown = Random.Range(5, 15);

    targetRotation = transform.rotation.eulerAngles.y;

    name = name.Replace("(Clone)", "");
  }

  void FixedUpdate()
  {
    transform.position += speed * transform.forward;

    collisionCooldown -= Time.deltaTime;
    nextRotationCooldown -= Time.deltaTime;
    if (nextRotationCooldown < 0)
    {
      nextRotationCooldown = Random.Range(5, 15);

      targetRotation += Random.Range(30, 90);
    }
    targetRotation %= 360;

    transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, Mathf.Lerp(transform.rotation.eulerAngles.y, targetRotation, Time.deltaTime), transform.rotation.eulerAngles.z);

    randomSoundCooldown -= Time.deltaTime;
    if (randomSoundCooldown < 0)
    {
      randomSoundCooldown = Random.Range(2, 15);
      if (randomSounds.Length > 0)
      {
        var s = randomSounds[Random.Range(0, randomSounds.Length)];
        AudioSource.PlayClipAtPoint(s, transform.position);
      }
    }
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

  void Interact(PlayerScript p)
  {
    if (interactSounds.Length > 0)
    {
      var s = interactSounds[Random.Range(0, interactSounds.Length)];
      AudioSource.PlayClipAtPoint(s, transform.position);
    }
  }

  #endregion
}

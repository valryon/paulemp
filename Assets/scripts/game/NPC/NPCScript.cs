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

  private AudioSource source;
  private float randomSoundCooldown;

  #endregion

  #region Timeline

  void Start()
  {
    source = GetComponent<AudioSource>();

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

    if (PlayerScript.LocalPlayer != null && Vector3.Distance(this.transform.position, PlayerScript.LocalPlayer.transform.position) < 5)
    {
      randomSoundCooldown -= Time.deltaTime;
      if (randomSoundCooldown < 0)
      {
        randomSoundCooldown = Random.Range(8, 20);
        if (randomSounds.Length > 0)
        {
          var s = randomSounds[Random.Range(0, randomSounds.Length)];
          source.clip = s;
          source.Play();
        }
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
    transform.LookAt(p.transform);
    transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

    targetRotation = transform.eulerAngles.y;
    nextRotationCooldown = 5;

    if (interactSounds.Length > 0)
    {
      var s = interactSounds[Random.Range(0, interactSounds.Length)];
      source.clip = s;
      source.Play();
    }
  }

  #endregion
}

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public abstract class QTEScript : MonoBehaviour
{
  public abstract QTEEnum Type
  {
    get;
  }

  public abstract void Launch(PlayerScript playerScript, System.Action<QTEResult> endQTE);
}

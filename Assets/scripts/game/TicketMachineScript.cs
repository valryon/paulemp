using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class TicketMachineScript : MonoBehaviour
{
  // This is not a networked script. Only booth is.

  #region Members

  [Header("Bindings")]
  public BoothScript booth;

  #endregion

  #region Timeline

  void Interact(PlayerScript p)
  {
    booth.RequestTicket();
  }

  #endregion
}

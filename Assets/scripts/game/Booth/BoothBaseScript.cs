using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public struct BoothData
{
  public int boothId;
  public string boothName;
  public bool isFirst;
  public bool isLast;
}

public class BoothBaseScript : MonoBehaviour
{
  #region Members

  [Header("Network spawns position")]
  public Transform pnjLocation;

  [Header("Bindings")]
  public TicketMachineScript ticketMachine;
  public Text ticketDisplay;
  public Text display;

  // ID set by the level generator so we can get back the booth later
  public int GeneratedID = 0;
  public int Floor = 0;

  #endregion

  #region Methods

  public void ClearSpawns()
  {
    Destroy(pnjLocation.gameObject);

    foreach (var p in FindObjectsOfType<DecorationLocation>())
    {
      Destroy(p.gameObject);
    }
  }

  #endregion
}

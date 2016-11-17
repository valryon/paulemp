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

  #endregion
}

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class BoothScript : NetworkBehaviour
{
  #region Members
  
  [Header("Bindings")]
  public Transform pnjLocation;
  public GameObject pnjPrefab;
  public TicketMachineScript ticketMachine;
  public Text ticketDisplay;
  public Text boothNumberDisplay;

  [Header("Data")]
  [SyncVar]
  public int currentNumber;

  [SyncVar]
  public int boothId;

  #endregion

  #region Timeline

  [Server]
  void Start()
  {
    // Create PNJ
    var pnj = Instantiate(pnjPrefab, pnjLocation.position, pnjLocation.rotation) as GameObject;
    NetworkServer.Spawn(pnj);

    // Link ticket machine to booth
    ticketMachine.booth = this;

    // Set random ticket number
    currentNumber = Random.Range(0, 99);
    UpdateNumber();

    // Set booth ID
    boothId = (int)netId.Value;
    boothNumberDisplay.text = "E-" + boothId.ToString("00");
  }

  #endregion

  #region Methods

  private void UpdateNumber()
  {
    ticketDisplay.text = currentNumber.ToString("00");
  }

  #endregion
}

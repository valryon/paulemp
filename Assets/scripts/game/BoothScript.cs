using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class BoothScript : NetworkBehaviour
{
  #region Members
  
  [Header("Bindings")]
  public Transform pnjLocation;
  public GameObject pnjPrefab;
  public GameObject ticketPrefab;
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

    // Set booth ID
    boothId = (int)netId.Value;
    boothNumberDisplay.text = "E-" + boothId.ToString("00");
  }

  void Update()
  {
    UpdateNumber();
  }

  #endregion

  #region Methods

  private void UpdateNumber()
  {
    ticketDisplay.text = currentNumber.ToString("00");
  }

  [Client]
  public void RequestTicket()
  {
    // Player requested a ticket
    // Send a command to the server
    CmdPrintTicket();
  }

  [Command]
  private void CmdPrintTicket()
  {
    PrintTicket();
  }

  [ServerCallback]
  public void PrintTicket()
  {
    var ticket = Instantiate(ticketPrefab, ticketMachine.transform.position + new Vector3(0, 1, 0), Quaternion.identity) as GameObject;

    TicketScript tscript = ticket.GetComponent<TicketScript>();
    tscript.booth = boothId;
    tscript.number = currentNumber;

    Rigidbody rbody = ticket.GetComponent<Rigidbody>();
    rbody.AddForce(new Vector3(Random.Range(100f, 250f), 0, Random.Range(100f, 250f)));

    NetworkServer.Spawn(ticket);

    currentNumber++;
    
  }

  #endregion
}

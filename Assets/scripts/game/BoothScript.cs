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

  [Header("Gameplay")]
  public float waitBetweenTickets = 10f;

  [Header("Data")]
  [SyncVar]
  public int lastTicketNumber;

  [SyncVar]
  public int currentTicketNumber;

  [SyncVar]
  public int boothId;

  [SyncVar]
  public float ticketWaitCooldown;

  #endregion

  #region Timeline

  [ServerCallback]
  void Start()
  {
    // Create PNJ
    var pnj = Instantiate(pnjPrefab, pnjLocation.position, pnjLocation.rotation) as GameObject;
    NetworkServer.Spawn(pnj);

    // Link ticket machine to booth
    ticketMachine.booth = this;

    // Set random ticket number
    lastTicketNumber = Random.Range(10, 99);

    // Agent always makes player wait
    currentTicketNumber = lastTicketNumber - Random.Range(1, 10);
    ticketWaitCooldown = waitBetweenTickets;

    // Set booth ID
    boothId = (int)netId.Value;
    boothNumberDisplay.text = "E-" + boothId.ToString("00");
  }

  void Update()
  {
    UpdateNumber();

    UpdateServer();
  }

  [ServerCallback]
  void UpdateServer()
  {
    ticketWaitCooldown -= Time.deltaTime;
    if(ticketWaitCooldown < 0)
    {
      // Next ticket
      NextTicket();
    }
  }

  #endregion

  #region Methods
  
  private void UpdateNumber()
  {
    ticketDisplay.text = currentTicketNumber.ToString("00");
  }

  [Server]
  private void NextTicket()
  {
    ticketWaitCooldown = waitBetweenTickets;

    if(currentTicketNumber < lastTicketNumber)
    {
      currentTicketNumber++;
    }
  }

  [Server]
  public void PrintTicket()
  {
    lastTicketNumber += Random.Range(1, 3);

    var ticket = Instantiate(ticketPrefab, ticketMachine.transform.position + new Vector3(0, 1, 0), Quaternion.identity) as GameObject;

    TicketScript tscript = ticket.GetComponent<TicketScript>();
    tscript.booth = boothId;
    tscript.number = lastTicketNumber;

    Rigidbody rbody = ticket.GetComponent<Rigidbody>();
    rbody.AddForce(new Vector3(Random.Range(100f, 250f), 0, Random.Range(100f, 250f)));

    NetworkServer.Spawn(ticket);    
  }

  #endregion
}

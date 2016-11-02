using System.Collections;
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

  [Header("QTE List")]
  public QTEScript qteList;

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
  public string boothName;

  [SyncVar]
  public float ticketWaitCooldown;

  [SyncVar]
  public bool busy;

  #endregion

  #region Timeline

  [ServerCallback]
  void Start()
  {
    // Create PNJ
    var pnj = Instantiate(pnjPrefab, pnjLocation.position, pnjLocation.rotation) as GameObject;
    AgentScript agent = pnj.GetComponent<AgentScript>();
    agent.booth = this.gameObject;
    NetworkServer.Spawn(pnj);

    // Link ticket machine to booth
    ticketMachine.booth = this;

    // Set random ticket number
    lastTicketNumber = Random.Range(10, 99);

    // Agent always makes player wait
    currentTicketNumber = lastTicketNumber - Random.Range(1, 10);
    ticketWaitCooldown = waitBetweenTickets;

    boothId = (int)(netId.Value * 100) + Random.Range(0, 99);
    boothName = ((char)('A' + Random.Range(0, 26))).ToString() + "-" + boothId.ToString("000");
  }

  void Update()
  {
    UpdateDisplays();

    UpdateServer();
  }

  [ServerCallback]
  void UpdateServer()
  {
    if (busy == false)
    {
      ticketWaitCooldown -= Time.deltaTime;
      if (ticketWaitCooldown < 0)
      {
        // Next ticket
        NextTicket();
      }
    }
  }

  #endregion

  #region Methods

  private void UpdateDisplays()
  {
    ticketDisplay.text = currentTicketNumber.ToString("00");
    boothNumberDisplay.text = boothName;
  }

  [Server]
  private void NextTicket()
  {
    busy = false;
    ticketWaitCooldown = waitBetweenTickets;

    if (currentTicketNumber < lastTicketNumber)
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
    tscript.data.booth = boothId;
    tscript.data.name = boothName;
    tscript.data.number = lastTicketNumber;

    Rigidbody rbody = ticket.GetComponent<Rigidbody>();
    rbody.AddForce(new Vector3(Random.Range(100f, 250f), 0, Random.Range(100f, 250f)));

    NetworkServer.Spawn(ticket);
  }

  #endregion
}

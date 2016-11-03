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

  [SyncVar]
  public QTEEnum qte;

  #endregion

  #region Timeline

  [ServerCallback]
  void Start()
  {
    // Link ticket machine to booth
    ticketMachine.booth = this;
  }

  [Server]
  public void EnableBooth(int id, QTEEnum qte)
  {
    this.qte = qte;

    // Set booth id and name from GameServer
    boothId = (int)(id * 100) + Random.Range(0, 99);
    boothName = ((char)('A' + Random.Range(0, 26))).ToString() + "-" + boothId.ToString("000");

    // Create PNJ
    var pnj = Instantiate(pnjPrefab, pnjLocation.position, pnjLocation.rotation) as GameObject;
    AgentScript agent = pnj.GetComponent<AgentScript>();
    agent.booth = this.gameObject;
    NetworkServer.Spawn(pnj);

    // Set random ticket number
    lastTicketNumber = Random.Range(10, 99);

    // Agent always makes player wait
    currentTicketNumber = lastTicketNumber - Random.Range(1, 10);
    ticketWaitCooldown = waitBetweenTickets;
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

      foreach (var p in FindObjectsOfType<PlayerScript>())
      {
        p.RpcPlaySound("agent_number_changed", this.transform.position);
      }
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

  [Server]
  public void Accept(PlayerScript playerScript)
  {
    busy = true;

    playerScript.RpcPlayQTE(playerScript.netId.Value, qte);
  }

  #endregion
}

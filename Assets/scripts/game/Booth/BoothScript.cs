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
  public BoothData data;

  [SyncVar]
  public GameObject agent;

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
  public void EnableBooth(int id, bool first, bool last, QTEEnum qte)
  {
    this.qte = qte;

    // Set booth id and name from GameServer
    data.boothId = (int)(id * 1000) + Random.Range(0, 999);

    var a = ((char)('A' + Random.Range(0, 26))).ToString();
    var b = ((char)('A' + Random.Range(0, 26))).ToString();
    var c = ((char)('A' + Random.Range(0, 26))).ToString();

    data.boothName = a + b + c + "-" + data.boothId.ToString("000");
    data.isFirst = first;
    data.isLast = last;

    // Create PNJ
    var agentGameObject = Instantiate(pnjPrefab, pnjLocation.position, pnjLocation.rotation) as GameObject;
    var agentScript = agentGameObject.GetComponent<AgentScript>();
    agentScript.booth = this.gameObject;
    agentGameObject.transform.parent = this.transform.parent;
    NetworkServer.Spawn(agentGameObject);

    agent = agentGameObject;

    // Set random ticket number
    lastTicketNumber = Random.Range(10, 99);

    // Agent always makes player wait
    currentTicketNumber = lastTicketNumber - Random.Range(1, 10);
    ticketWaitCooldown = Random.Range(0, 3f * waitBetweenTickets);
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
    boothNumberDisplay.text = data.boothName;
  }

  [Server]
  private void NextTicket()
  {
    busy = false;
    ticketWaitCooldown = waitBetweenTickets;

    if (currentTicketNumber < lastTicketNumber)
    {
      currentTicketNumber++;

      GameServer.PlaySound("agent_number_changed", this.transform.position);
    }
  }

  [Server]
  public void PrintTicket()
  {
    lastTicketNumber += Random.Range(1, 3);

    var ticket = Instantiate(ticketPrefab, ticketMachine.transform.position + new Vector3(0, 1, 0), Quaternion.identity) as GameObject;

    TicketScript tscript = ticket.GetComponent<TicketScript>();
    tscript.data.booth = data.boothId;
    tscript.data.name = data.boothName;
    tscript.data.number = lastTicketNumber;

    Rigidbody rbody = ticket.GetComponent<Rigidbody>();
    rbody.AddForce(new Vector3(Random.Range(100f, 250f), 0, Random.Range(100f, 250f)));

    NetworkServer.Spawn(ticket);
  }

  [Server]
  public void Accept(PlayerScript playerScript)
  {
    busy = true;

    playerScript.RpcPlayQTE(netId.Value, playerScript.netId.Value, qte);
  }

  #endregion
}

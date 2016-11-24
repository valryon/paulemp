using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AgentScript : NetworkBehaviour
{

  #region Members

  public const float WAIT_BETWEEN_TICKETS = 2f;

  [Header("Prefabs")]
  public GameObject ticketPrefab;

  [Header("Data")]
  [SyncVar]
  public int lastTicketNumber;

  [SyncVar]
  public int currentTicketNumber;

  [SyncVar]
  public BoothData data;

  [SyncVar]
  public int boothGeneratedID;

  [SyncVar]
  public float ticketWaitCooldown;

  [SyncVar]
  public bool busy;

  [SyncVar]
  public QTEEnum qte;

  // NON-NETWORKED reference to a CLIENT obejct
  private BoothBaseScript localBoothRef;
  private Rigidbody rbody;

  #endregion

  #region Timeline

  [ClientCallback]
  void Start()
  {
    rbody = GetComponent<Rigidbody>();

    Link();
  }

  [Server]
  public void EnableBooth(int id, bool first, bool last, QTEEnum qte, int floor)
  {
    this.qte = qte;

    // Set booth id and name from GameServer
    data.boothId = (int)(id * 1000) + Random.Range(0, 999);

    var a = ((char)('A' + Random.Range(0, 26))).ToString();
    var b = ((char)('A' + Random.Range(0, 26))).ToString();
    var c = ((char)('A' + Random.Range(0, 26))).ToString();

    data.boothName = floor.ToString("00") + "-" + a + b + c + "-" + data.boothId.ToString("000");
    data.isFirst = first;
    data.isLast = last;

    this.name = "Agent " + data.boothName;

    // Set random ticket number
    lastTicketNumber = Random.Range(10, 99);

    // Agent always makes player wait
    currentTicketNumber = lastTicketNumber - Random.Range(1, 10);
    ticketWaitCooldown = Random.Range(0, 3f * WAIT_BETWEEN_TICKETS);

    Link();
  }

  void Update()
  {
    UpdateClient();

    UpdateServer();
  }

  [ClientCallback]
  private void UpdateClient()
  {
    rbody.isKinematic = !(PlayerScript.HasGeneratedLevel || NetworkServer.active);
    rbody.useGravity = PlayerScript.HasGeneratedLevel || NetworkServer.active;

    if (PlayerScript.HasGeneratedLevel)
    {
      Booth.ticketDisplay.text = currentTicketNumber.ToString("00");
      Booth.display.text = data.boothName;
    }
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

  void Interact(PlayerScript p)
  {
    p.RequestCheckTicket(this);
  }

  #endregion

  #region Methods

  private void Link()
  {
    foreach (var b in FindObjectsOfType<BoothBaseScript>())
    {
      if (b.GeneratedID == boothGeneratedID)
      {
        localBoothRef = b;

        // Link local stuff to this agent
        b.ticketMachine.agent = this;

        break;
      }
    }
  }

  /// <summary>
  /// Look at something rotating Z axis only
  /// </summary>
  [Server]
  public void LookAt(Transform t)
  {
    transform.LookAt(t);
    transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
  }

  /// <summary>
  /// Look at something rotating Z axis only
  /// </summary>
  [Server]
  public void LookAtTicketMachine()
  {
    Link();

    LookAt(Booth.ticketMachine.transform);
  }

  [Server]
  private void NextTicket()
  {
    busy = false;
    ticketWaitCooldown = WAIT_BETWEEN_TICKETS;

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

    var ticket = Instantiate(ticketPrefab, Booth.ticketMachine.ticketSpawn.position, Booth.ticketMachine.ticketSpawn.rotation) as GameObject;

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

  private BoothBaseScript Booth
  {
    get
    {
      if (localBoothRef == null)
      {
        Link();
      }

      if (localBoothRef == null)
      {
        Debug.LogError("Agent cannot find local booth!");
      }
      return localBoothRef;
    }
  }
}

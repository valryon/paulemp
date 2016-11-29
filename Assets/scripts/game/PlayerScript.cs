using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections.Generic;
using System.Collections;

public class PlayerScript : NetworkBehaviour
{
  public static bool HasGeneratedLevel;

  #region Members

  public const int QUESTS_COUNT = 4;

  [Header("Bindings")]
  public GameObject sprites;
  public RigidbodyFirstPersonController fpsController;
  public Camera fpsCamera;

  [Header("SFX")]
  public List<AudioClip> sounds;
  public List<GameObject> effects;

  [Header("Data")]
  [SyncVar]
  public TicketList tickets = new TicketList();

  [SyncVar]
  public QuestList quests = new QuestList();

  [SyncVar]
  public float elapsedTime;

  private PlayerUIScript ui;

  private Ray raycast;
  private bool isPlayingQTE;

  private uint qteBoothId;

  private bool wasJumping;
  private bool isGameOver;

  #endregion

  #region Timeline

  void Start()
  {
    ui = PlayerUIScript.Instance;

    this.name = "Player #" + netId.Value.ToString();

    if (isLocalPlayer)
    {
      // Local player = remove sprite / FPS view
      Destroy(sprites);

      ui.Player = this;
      ui.ShowHUD();

      this.name += " (Local)";

      SetBoothOrder();
    }
    else
    {
      // Remote player = remove FPS Controller, keep model
      Destroy(fpsCamera.gameObject);
      Destroy(fpsController);
    }
  }

  public void SetBoothOrder()
  {
    System.Random r = new System.Random();

    var agents = FindObjectsOfType<AgentScript>()
      .OrderBy(b => r.NextDouble())
      .ToList();

    if (agents.Count == 0)
    {
      Debug.LogError("No booth found... wth, network issue?");
    }
    if (agents.Count == 1)
    {
      Debug.LogError("Only one booth found?! wth, network issue?");
    }

    var first = agents.Where(b => b.data.isFirst).First();
    agents.Remove(first);
    var last = agents.Where(b => b.data.isLast).First();
    agents.Remove(last);
    var availableAgents = agents.Take(QUESTS_COUNT).ToList();

    var questsList = new List<Quest>();

    // First
    var q = new Quest(first, availableAgents[0], true, false, availableAgents.Count + 1);
    q.revealed = true;
    questsList.Add(q);

    // Between: random booth visit
    for (int i = 0; i < availableAgents.Count - 1; i++)
    {
      q = new Quest(availableAgents[i], availableAgents[i + 1], false, false, availableAgents.Count - i);
      questsList.Add(q);
    }

    questsList.Add(new Quest(availableAgents[availableAgents.Count - 1], null, false, false, 0));

    // Last
    q = new Quest(last, first, false, true, availableAgents.Count + 2);
    q.revealed = true;
    questsList.Add(q);

    var questsToSync = questsList.OrderBy(qu => qu.order).ToArray();

    CmdSetBoothOrder(questsToSync);
  }

  void Update()
  {
    UpdateClient();
    UpdateServer();
  }

  [ClientCallback]
  void UpdateClient()
  {
    if (isGameOver) return;

    if (isLocalPlayer)
    {
      if (wasJumping == false && fpsController.Jumping)
      {
        CmdPlaySound("jump", transform.position);
      }
      wasJumping = fpsController.Jumping;

      if (isPlayingQTE)
      {
        ui.SetCrosshairColor(Color.white * 0f);
      }
      else
      {
        // Raycast from view
        //--------------------------------------------------------------------------------
        raycast = new Ray(fpsCamera.transform.position, fpsCamera.transform.forward);
        ui.SetCrosshairColor(Color.white);

        foreach (var hit in Physics.RaycastAll(raycast))
        {
          var go = hit.collider.gameObject;

          Vector3 a = go.transform.position;
          a.y = 0;

          Vector3 b = this.transform.position;
          b.y = 0;

          if (Vector3.Distance(a, b) < 5f)
          {
            if (go != this.gameObject && go.tag == "Interactable")
            {
              ui.SetCrosshairColor(Color.green);

              if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.E))
              {
                Debug.Log("INTERACT " + go);
                go.SendMessage("Interact", this, SendMessageOptions.DontRequireReceiver);
              }
              break;
            }

          }
        }

        if(Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Return))
        {
          ui.ZoomArm();
        }
      }

#if !UNITY_EDITOR
      if(Input.GetKeyDown(KeyCode.Escape))
      {
        NetworkManager.singleton.StopClient();
        Application.Quit();
      }
#endif
      //--------------------------------------------------------------------------------


      // Quests all completed?
      bool gameOver = quests.Count > 0;
      for (int i = 0; i < quests.Count; i++)
      {
        gameOver &= quests[i].completed;
      }

      if (gameOver)
      {
        isGameOver = true;
        fpsController.enabled = false;
        ui.ShowGameOver(elapsedTime);
      }
    }
  }

  [ServerCallback]
  void UpdateServer()
  {
    elapsedTime += Time.deltaTime;
  }

  void OnDestroy()
  {
    if (NetworkServer.active == false)
    {
      NetworkManager.singleton.StopClient();
    }
  }

  #endregion

  #region Methods

  #endregion

  #region RPC

  [ClientRpc]
  public void RpcGenerateLevel(int seed)
  {
    if (HasGeneratedLevel == false)
    {
      if (NetworkServer.active)
      {
        // Host has a different generation
        HasGeneratedLevel = true;
      }
      else if (isLocalPlayer)
      {
        Debug.Log("CLIENT generate level");

        LevelGenerator l = FindObjectOfType<LevelGenerator>();
        l.Generate(seed);
        HasGeneratedLevel = true;

        l.GenerateProps(false);
      }
    }
  }

  [Client]
  public void RequestTicket(AgentScript agent)
  {
    // Player requested a ticket
    // Send a command to the server
    // Needs to be done HERE as a Command can only be called for local authority / player
    CmdPrintTicket(agent.data.boothId);
  }

  [Command]
  private void CmdPrintTicket(int boothId)
  {
    var agents = FindObjectsOfType<AgentScript>();
    var agent = agents.Where(b => b.data.boothId == boothId).FirstOrDefault();

    StartCoroutine(PrintTickets(agent));
  }

  private IEnumerator PrintTickets(AgentScript agent)
  {
    if (agent != null)
    {
      int count = 1;

      if (Random.Range(0, 10) > 8)
      {
        count = Random.Range(3, 11);
      }

      for (int i = 0; i < count; i++)
      {
        agent.PrintTicket();
        RpcPlaySound("ticket_print", this.transform.position);

        yield return new WaitForSeconds(Random.Range(0.1f, 0.25f));
      }

    }
  }

  [Client]
  public void RequestPickTicket(TicketScript t)
  {
    CmdPickTicket(t.netId.Value);
  }

  [Command]
  private void CmdPickTicket(uint ticketId)
  {
    var t = NetworkServer.FindLocalObject(new NetworkInstanceId(ticketId));

    if (t != null)
    {
      var ticket = t.GetComponent<TicketScript>();
      if (ticket != null)
      {
        // Add ticket to inventory

        // -- Make sure we only have one ticket for a given booth
        bool recycle = false;
        TicketData recycleData = new TicketData();

        foreach (var it in tickets)
        {
          if (it.booth == ticket.data.booth)
          {
            recycle = true;
            recycleData = it;

            tickets.Remove(it);
            break;
          }
        }

        // Add current ticket data
        tickets.Add(ticket.data);

        // Recycle old ticket?
        if (recycle)
        {
          ticket.Recycle(recycleData, transform.position + (transform.forward), transform.forward);
        }
        else
        {
          ticket.Destroy();
        }

        RpcPlaySound("ticket_picked", this.transform.position);
      }
    }
  }

  [Client]
  public void RequestCheckTicket(AgentScript agent)
  {
    CmdCheckTicket(agent.netId);
  }

  [Command]
  private void CmdCheckTicket(NetworkInstanceId id)
  {
    GameObject b = NetworkServer.FindLocalObject(id);
    if (b != null)
    {
      AgentScript agent = b.GetComponent<AgentScript>();

      if (agent != null && agent.busy == false)
      {
        // Make the agent look at player
        agent.LookAt(this.transform);

        // Make agent talk
        NetworkIdentity agentNet = agent.GetComponent<NetworkIdentity>();
        RpcPlayAnimation("talk", agentNet.netId);

        int number = tickets.GetFor(agent.data.boothId);
        if (number == agent.currentTicketNumber)
        {
          // Yeepee!
          RpcPlaySound("agent_ticket_ok", this.transform.position);

          tickets.RemoveFor(agent.data.boothId);

          // Quest dependencies satisfied?
          if (quests.CanBeCompleted(agent))
          {
            RpcPlaySound("agent_hello", this.transform.position);
            agent.Accept(this);
          }
          else
          {
            RpcPlaySound("agent_miss_document", this.transform.position);
            quests.Reveal((int)agent.netId.Value);
          }
        }
        else
        {
          RpcPlaySound("agent_ticket", this.transform.position);
        }
      }
      else
      {
        RpcPlaySound("agent_busy", this.transform.position);
      }
    }
  }

  [Command]
  public void CmdPlayEffect(string effectName, Vector3 effectPosition)
  {
    RpcPlayEffect(effectName, effectPosition);
  }

  [ClientRpc]
  public void RpcPlayEffect(string effectName, Vector3 effectPosition)
  {
    if (effects == null) return;

    foreach (var e in effects)
    {
      if (e.name.Equals(effectName, System.StringComparison.InvariantCultureIgnoreCase))
      {
        Instantiate(e, effectPosition, Quaternion.identity);
        break;
      }
    }
  }

  [Command]
  public void CmdPlaySound(string sound, Vector3 position)
  {
    RpcPlaySound(sound, position);
  }

  [ClientRpc]
  public void RpcPlaySound(string sound, Vector3 position)
  {
    if (sounds == null) return;

    // Find all clips starting with the given name
    List<AudioClip> clips = new List<AudioClip>();
    foreach (var s in sounds)
    {
      if (s != null && s.name.ToLower().StartsWith(sound.ToLower()))
      {
        clips.Add(s);
      }
    }

    // Play a random clip among the ones found
    if (clips.Count > 0)
    {
      AudioSource.PlayClipAtPoint(clips[Random.Range(0, clips.Count)], position);
    }
  }

  [ClientRpc]
  public void RpcPlayQTE(uint boothId, uint playerNetId, QTEEnum qte)
  {
    if (isLocalPlayer && netId.Value == playerNetId && isPlayingQTE == false)
    {
      var qteScript = FindObjectsOfType<QTEScript>().Where(q => q.Type == qte).FirstOrDefault();

      if (qteScript != null)
      {
        // Start QTE
        isPlayingQTE = true;
        fpsController.enabled = false;
        qteBoothId = boothId;

        Debug.Log("CLIENT QTE starting " + qteScript);

        qteScript.Launch(this, EndQTE);
      }
      else
      {
        Debug.LogError("CLIENT Unknow QTE " + qte);
      }
    }
  }

  [Client]
  private void EndQTE(QTEResult result)
  {
    isPlayingQTE = false;
    fpsController.enabled = true;

    Debug.Log("CLIENT QTE ended " + result);

    CmdEndQTE(result);
  }

  [Command]
  private void CmdEndQTE(QTEResult result)
  {
    // Get booth and make agent talk
    var o = NetworkServer.FindLocalObject(new NetworkInstanceId(qteBoothId));
    if (o != null)
    {
      AgentScript agent = o.GetComponent<AgentScript>();
      if (agent != null)
      {
        RpcPlayAnimation("talk", agent.netId);
      }
    }

    // Tell players result
    if (result == QTEResult.TimeOut)
    {
      RpcPlaySound("qte_notcompleted", this.transform.position);
      RpcPlaySound("agent_timeout", this.transform.position);
    }
    else if (result == QTEResult.Failure)
    {
      RpcPlaySound("qte_failure", this.transform.position);
      RpcPlaySound("agent_error", this.transform.position);
    }
    else if (result == QTEResult.Success)
    {
      RpcPlaySound("qte_success", this.transform.position);
      RpcPlaySound("agent_ok", this.transform.position);

      // Update quests!
      for (int i = 0; i < quests.Count; i++)
      {
        var q = quests[i];
        if (q.agentID == qteBoothId)
        {
          q.completed = true;
        }
        quests[i] = q;
      }
    }
  }

  [Command]
  private void CmdSetBoothOrder(Quest[] q)
  {
    this.quests.Clear();
    for (int i = 0; i < q.Length; i++)
    {
      this.quests.Add(q[i]); ;
    }
  }

  [ClientRpc]
  public void RpcPlayAnimation(string animName, NetworkInstanceId id)
  {
    GameObject b = ClientScene.FindLocalObject(id);
    if (b != null)
    {
      foreach (SimpleAnimator anim in b.GetComponentsInChildren<SimpleAnimator>())
      {
        anim.Play(animName);
      }
    }
  }

  #endregion

  #region Debug

  void OnDrawGizmos()
  {
    Gizmos.DrawRay(raycast);
  }

  #endregion
}

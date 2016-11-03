using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections.Generic;
using System.Collections;

public class PlayerScript : NetworkBehaviour
{
  #region Members

  [Header("Bindings")]
  public MeshRenderer model;
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

  private PlayerUIScript ui;
  private Ray raycast;
  private bool isPlayingQTE;

  #endregion

  #region Timeline

  void Start()
  {
    ui = PlayerUIScript.Instance;

    this.name = "Player #" + netId.Value.ToString();

    if (isLocalPlayer)
    {
      // Local player = remove model / FPS view
      Destroy(model.gameObject);

      ui.player = this;
      ui.gameObject.SetActive(true);

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
    var booths = FindObjectsOfType<BoothScript>().OrderBy(b => Random.Range(0, 100)).ToList();

    var first = booths.Where(b => b.data.isFirst).First();
    booths.Remove(first);
    var last = booths.Where(b => b.data.isLast).First();
    booths.Remove(last);

    var questsList = new List<Quest>();
    questsList.Add(new Quest(first, booths[0], true, false));

    for (int i = 0; i < booths.Count - 1; i++)
    {
      questsList.Add(new Quest(booths[i], booths[i + 1], false, false));
    }

    questsList.Add(new Quest(booths[booths.Count - 1], null, false, false));
    questsList.Add(new Quest(last, null, false, true));

    CmdSetBoothOrder(questsList);
  }

  void Update()
  {
    UpdateClient();
  }

  [ClientCallback]
  void UpdateClient()
  {
    if (isLocalPlayer)
    {
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
      }
      //--------------------------------------------------------------------------------
    }
  }

  #endregion

  #region Methods

  #endregion

  #region RPC

  [Client]
  public void RequestTicket(BoothScript booth)
  {
    // Player requested a ticket
    // Send a command to the server
    // Needs to be done HERE as a Command can only be called for local authority / player
    CmdPrintTicket(booth.data.boothId);
  }

  [Command]
  private void CmdPrintTicket(int boothId)
  {
    var booths = FindObjectsOfType<BoothScript>();
    var booth = booths.Where(b => b.data.boothId == boothId).FirstOrDefault();

    StartCoroutine(PrintTickets(booth));
  }

  private IEnumerator PrintTickets(BoothScript booth)
  {
    if (booth != null)
    {
      int count = 1;

      if (Random.Range(0, 10) > 8)
      {
        count = Random.Range(3, 11);
      }

      for (int i = 0; i < count; i++)
      {
        booth.PrintTicket();
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
  public void RequestCheckTicket(BoothScript booth)
  {
    CmdCheckTicket(booth.netId);
  }

  [Command]
  private void CmdCheckTicket(NetworkInstanceId id)
  {
    GameObject b = NetworkServer.FindLocalObject(id);
    if (b != null)
    {
      BoothScript booth = b.GetComponent<BoothScript>();

      if (booth != null && booth.busy == false)
      {
        int number = tickets.GetFor(booth.data.boothId);
        if (number == booth.currentTicketNumber)
        {
          // Yeepee!
          RpcPlaySound("agent_ticket_ok", this.transform.position);
          Debug.Log("Ticket used!");

          tickets.RemoveFor(booth.data.boothId);

          booth.Accept(this);
        }
        else
        {
          RpcPlaySound("agent_nope", this.transform.position);
          Debug.Log("Wrong or missing ticket!");
        }
      }
      else
      {
        RpcPlaySound("agent_nope", this.transform.position);
        Debug.Log("Booth is busy");
      }
    }
  }

  [ClientRpc]
  public void RpcPlayEffect(string effectName, Vector3 effectPosition)
  {
    PlayEffect(effectName, effectPosition);
  }

  public void PlayEffect(string effectName, Vector3 effectPosition)
  {
    foreach (var e in effects)
    {
      if (e.name.Equals(effectName, System.StringComparison.InvariantCultureIgnoreCase))
      {
        Instantiate(e, effectPosition, Quaternion.identity);
        break;
      }
    }
  }

  [ClientRpc]
  public void RpcPlaySound(string sound, Vector3 position)
  {
    PlaySound(sound, position);
  }

  public void PlaySound(string sound, Vector3 position)
  {
    foreach (var s in sounds)
    {
      if (s.name.Equals(sound, System.StringComparison.InvariantCultureIgnoreCase))
      {
        AudioSource.PlayClipAtPoint(s, position);
      }
    }
  }

  [ClientRpc]
  public void RpcPlayQTE(uint playerNetId, QTEEnum qte)
  {
    if (netId.Value == playerNetId && isPlayingQTE == false)
    {
      var qteScript = FindObjectsOfType<QTEScript>().Where(q => q.Type == qte).FirstOrDefault();

      if (qteScript != null)
      {
        // Start QTE
        isPlayingQTE = true;
        fpsController.enabled = false;

        Debug.Log("QTE starting " + qteScript);

        qteScript.Launch(this, EndQTE);
      }
      else
      {
        Debug.LogError("Unknow QTE " + qte);
      }
    }
  }

  [Client]
  private void EndQTE(QTEResult result)
  {
    isPlayingQTE = false;
    fpsController.enabled = true;

    Debug.Log("QTE ended " + result);

    CmdEndQTE(result);
  }

  [Command]
  private void CmdEndQTE(QTEResult result)
  {
    // Tell players result
    if (result == QTEResult.NotCompleted) GameServer.PlaySound("qte_notcompleted", this.transform.position);
    else if (result == QTEResult.Failure) GameServer.PlaySound("qte_failure", this.transform.position);
    else if (result == QTEResult.Success) GameServer.PlaySound("qte_success", this.transform.position);
  }

  [Command]
  private void CmdSetBoothOrder(List<Quest> q)
  {
    this.quests.Clear();
    for (int i = 0; i < q.Count; i++)
    {
      this.quests.Add(q[i]); ;
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

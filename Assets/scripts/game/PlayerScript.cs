using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using UnityStandardAssets.Characters.FirstPerson;
using System;
using System.Collections.Generic;

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
    }
    else
    {
      // Remote player = remove FPS Controller, keep model
      Destroy(fpsCamera.gameObject);
      Destroy(fpsController);
    }
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
    CmdPrintTicket(booth.boothId);
  }

  [Command]
  private void CmdPrintTicket(int boothId)
  {
    var booths = FindObjectsOfType<BoothScript>();
    var booth = booths.Where(b => b.boothId == boothId).FirstOrDefault();

    if (booth != null)
    {
      booth.PrintTicket();
      RpcPlaySound("ticket_print", this.transform.position);
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
        int number = tickets.GetFor(booth.boothId);
        if (number == booth.currentTicketNumber)
        {
          // Yeepee!
          RpcPlaySound("agent_ticket_ok", this.transform.position);
          Debug.Log("Ticket used!");

          tickets.RemoveFor(booth.boothId);

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
      if (e.name.Equals(effectName, StringComparison.InvariantCultureIgnoreCase))
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
      if (s.name.Equals(sound, StringComparison.InvariantCultureIgnoreCase))
      {
        AudioSource.PlayClipAtPoint(s, position);
      }
    }
  }

  [ClientRpc]
  public void RpcPlayQTE(uint playerNetId, QTEEnum qte)
  {
    if (netId.Value == playerNetId)
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
    CmdEndQTE(result);
  }

  [Command]
  private void CmdEndQTE(QTEResult result)
  {

  }

  #endregion

  #region Debug

  void OnDrawGizmos()
  {
    Gizmos.DrawRay(raycast);
  }

  #endregion
}

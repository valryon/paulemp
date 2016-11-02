using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerScript : NetworkBehaviour
{
  #region Members

  [Header("Bindings")]
  public MeshRenderer model;
  public RigidbodyFirstPersonController fpsController;
  public Camera fpsCamera;

  [Header("Data")]
  [SyncVar]
  public TicketList tickets = new TicketList();

  private PlayerUIScript ui;

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
      if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.E))
      {
        DoInteract();
      }
    }
  }

  #endregion

  #region Methods

  private Ray raycast;

  private void DoInteract()
  {
    // Raycast from view
    raycast = new Ray(fpsCamera.transform.position, fpsCamera.transform.forward);

    foreach (var hit in Physics.RaycastAll(raycast))
    {
      Vector3 a = hit.collider.transform.position;
      a.y = 0;

      Vector3 b = this.transform.position;
      b.y = 0;

      if (Vector3.Distance(a, b) < 5f)
      {
        if (hit.collider.gameObject != this.gameObject)
        {
          Debug.Log("INTERACT " + hit.collider.gameObject);
          hit.collider.gameObject.SendMessage("Interact", this, SendMessageOptions.DontRequireReceiver);

          break;
        }
      }
    }
  }
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

using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using FishNet.Component.Transforming;

public class Scroll : NetworkBehaviour
{
    private readonly SyncVar<NetworkObject> carriedByNetObject = new SyncVar<NetworkObject>();
    private Transform targetScrollBox = null;

    private Rigidbody rb;
    private Collider col;
    private NetworkTransform networkTransform;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>(); 
        networkTransform = GetComponent<NetworkTransform>();
        carriedByNetObject.OnChange += OnCarrierChanged;
    }

    void LateUpdate()
    {
        if (carriedByNetObject.Value != null)
        {
            if (targetScrollBox == null)
            {
                PlayerScrollBox scrollBoxComponent = carriedByNetObject.Value.GetComponentInChildren<PlayerScrollBox>();
                if (scrollBoxComponent != null)
                {
                    targetScrollBox = scrollBoxComponent.transform;
                }
            }

            if (targetScrollBox != null)
            {
                transform.position = targetScrollBox.position;
                transform.rotation = targetScrollBox.rotation;
            }
        }
    }



    [ServerRpc(RequireOwnership = false)]
    public void RpcPickup(NetworkConnection conn = null)
    {
        if (carriedByNetObject.Value != null) return;
        Debug.Log("RpcPickup on server");
        NetworkObject playerObject = conn.FirstObject;

        if (playerObject != null)
        {
            NetworkObject.GiveOwnership(conn);
            carriedByNetObject.Value = playerObject;
            RpcTogglePhysics(false);
        }

    }

    [ServerRpc]
    public void RpcDrop()
    {
        if (carriedByNetObject.Value == null) return;
        Debug.Log("RpcDrop on server");

        Vector3 dropVelocity = transform.TransformDirection(new Vector3(0, 2, 3));

        carriedByNetObject.Value = null;
        targetScrollBox = null;
        NetworkObject.RemoveOwnership();

        RpcTogglePhysics(true);
        rb.linearVelocity = dropVelocity; 
    }

    [ObserversRpc]
    private void RpcTogglePhysics(bool doEnable)
    {
        Debug.Log("RpcTogglePhysics " + doEnable);
        if (doEnable)
        {
            networkTransform.enabled = true;
            rb.useGravity = true;
            rb.isKinematic = false; 
            col.enabled = true;
        }
        else
        {
            networkTransform.enabled = false;
            rb.useGravity = false;
            rb.isKinematic = true; 
            col.enabled = false;
        }
    }

    private void OnCarrierChanged(NetworkObject prev, NetworkObject next, bool asServer)
    {
        if (next == null)
        {
            targetScrollBox = null;
        }
    }
    private void OnDestroy()
    {
        carriedByNetObject.OnChange -= OnCarrierChanged;
    }
}

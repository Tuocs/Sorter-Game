using UnityEngine;
using FishNet.Object;
using FishNet.Connection;

public class Scroll : NetworkBehaviour
{
    private bool isCarried = false;
    private Rigidbody rb;
    private Collider col;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>(); 
    }

    [ServerRpc(RequireOwnership = false)]
    public void RpcPickup(NetworkConnection conn = null)
    {
        if (!isCarried && base.IsController)
        {
            isCarried = true;
            NetworkObject playerObject = conn.FirstObject;

            if (playerObject != null)
            {
                Transform scrollbox = playerObject.GetComponentInChildren<PlayerScrollBox>().gameObject.transform;
                transform.SetParent(scrollbox);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                RpcTogglePhysics(false);
            }
            NetworkObject.GiveOwnership(conn);
        }
    }

    [ServerRpc]
    public void RpcDrop()
    {
        if (isCarried)
        {
            isCarried = false;
            NetworkObject.RemoveOwnership();
            //NetworkObject.UnsetParent();
            transform.SetParent(null);
            RpcTogglePhysics(true);
        }
    }

    [ObserversRpc]
    private void RpcTogglePhysics(bool doEnable)
    {
        if (doEnable)
        {
            rb.useGravity = true;
            rb.isKinematic = false; 
            col.enabled = true;
        }
        else
        {
            rb.useGravity = false;
            rb.isKinematic = true; 
            col.enabled = false;
        }
    }
}

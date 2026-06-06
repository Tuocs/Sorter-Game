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
        Debug.Log("RpcPickup");
        if (!isCarried)
        {
            isCarried = true;
            NetworkObject playerObject = conn.FirstObject;
            Debug.Log("RpcPickup|" + playerObject.name);
            if (playerObject != null)
            {
                NetworkObject scrollbox = playerObject.GetComponentInChildren<PlayerScrollBox>().gameObject.GetComponent<NetworkObject>();
                NetworkObject.SetParent(scrollbox);
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
        Debug.Log("RpcDrop");
        if (isCarried)
        {
            isCarried = false;
            RpcTogglePhysics(true);
            rb.linearVelocity = transform.TransformDirection(new Vector3(0,2,3)); 

            NetworkObject.RemoveOwnership();
            NetworkObject.UnsetParent();
        }
    }

    [ObserversRpc]
    private void RpcTogglePhysics(bool doEnable)
    {
        Debug.Log("RpcTogglePhysics " + doEnable);
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

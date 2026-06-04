using UnityEngine;
using FishNet.Object;
using FishNet.Connection;

public class Scroll : NetworkBehaviour
{
    private bool isCarried = false;

    [ServerRpc(RequireOwnership = false)]
    public void RpcPickup(NetworkConnection conn = null)
    {
        if (!isCarried && base.IsController)
        {
            NetworkObject.GiveOwnership(conn);
            NetworkObject playerObject = conn.FirstObject;

            if (playerObject != null)
            {
                base.NetworkObject.SetParent(playerObject);
            }
        }
    }

    [ServerRpc]
    public void RpcDrop()
    {
        
    }
}

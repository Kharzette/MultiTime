using UnityEngine;
using Unity.Netcode;


public class RpcTest : NetworkBehaviour
{
	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		//Only send an RPC to the server on the client that owns the
		//NetworkObject that owns this NetworkBehaviour instance
		if(!IsServer && IsOwner)
		{
			TestServerRPC(0, NetworkObjectId);
		}
	}


	[ClientRpc]
	void	TestClientRPC(int value, ulong sourceNetObjID)
	{
		Debug.Log("Client Received the RPC " + value + " on NetworkObject " + sourceNetObjID);
		
		if(IsOwner)
		{
			TestServerRPC(value + 1, sourceNetObjID);
		}
	}

	[ServerRpc]
	void	TestServerRPC(int value, ulong sourceNetObjID)
	{
		Debug.Log("Server Received the RPC " + value + " on NetworkObject " + sourceNetObjID);

		TestClientRPC(value, sourceNetObjID);
	}
}

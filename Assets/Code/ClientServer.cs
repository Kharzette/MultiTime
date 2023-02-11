using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Steamworks;


public class ClientServer : NetworkBehaviour
{
	List<Mobile>	mMobs	=new List<Mobile>();

	//client id to steam id
	Dictionary<ulong, ulong>	mPlayerIDs	=new Dictionary<ulong, ulong>();


	void	Start()
	{
		Debug.Log("ClientServer Start() " + IsServer);

		NetworkManager.OnServerStarted	+=OnServerStarted;
	}


	void	OnServerStarted()
	{
		Debug.Log("ClientServer OnServerStarted() " + IsServer);
		if(IsServer)
		{
			NetworkManager.OnClientConnectedCallback	+=OnClientConnected;
			NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler(
				"HeresMySteamID", OnHeresMySteamID);
		}
	}


	void	OnClientConnected(ulong clientID)
	{
		Debug.Log("Client: " + clientID + " connected, IsServer: " + IsServer + ", IsSpawned: " + IsSpawned);

		if(!IsServer)
		{
			return;
		}

		GimmeSteamIDClientRpc(69, clientID);
	}


	void	OnHeresMySteamID(ulong clientID, FastBufferReader fbr)
	{
		Debug.Log("OnHeresMySteamID Client: " + clientID + " connected, " + IsServer + ", " + fbr.Length);

		if(!IsServer)
		{
			return;
		}

		ulong	clientSteamID;
		string	clientSteamName;
		
		fbr.ReadValueSafe<ulong>(out clientSteamID);
		fbr.ReadValueSafe(out clientSteamName);

		if(!mPlayerIDs.ContainsKey(clientID))
		{
			mPlayerIDs.Add(clientID, clientSteamID);
		}

		NetworkObject	playerObj	=NetworkManager.SpawnManager.GetPlayerNetworkObject(clientID);

		playerObj.gameObject.name	=clientSteamName;
	}


	[ClientRpc]
	void	GimmeSteamIDClientRpc(int value, ulong sourceNetObjID)
	{
		Debug.Log("GimmeSteamID value: " + value + " , " + IsServer + ", " + sourceNetObjID);

		if(!SteamMan.Initialized)
		{
			return;
		}

		string	myName	=SteamFriends.GetPersonaName();

		FastBufferWriter	fbw	=new FastBufferWriter(32, Unity.Collections.Allocator.Temp, 256);

		fbw.WriteValueSafe<ulong>(SteamUser.GetSteamID().m_SteamID);
		fbw.WriteValueSafe(myName);

		NetworkManager.CustomMessagingManager.SendNamedMessage(
			"HeresMySteamID", 0, fbw);
	}
}

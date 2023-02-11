using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class Login : MonoBehaviour
{
	public string	mSteamName;

	void Start()
	{		
		if(SteamMan.Initialized)
		{
			string	name	=SteamFriends.GetPersonaName();

			Debug.Log("Steam Name: " + name);

			mSteamName	=name;

//			SteamGameServer.SetDedicatedServer(true);

//			SteamGameServer.SetGameDescription("Goblinoids");

//			SteamGameServer.LogOnAnonymous();
		}
	}

	void Update()
	{
	}
}

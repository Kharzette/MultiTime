using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Steamworks;
using TMPro;


public class Player : Mobile
{
	ulong	mLocalID;	//steam id valid on client


	void Start()
	{
		Debug.Log("Player Start() " + IsClient + ", " + IsLocalPlayer);

		if(this.IsClient)
		{
			if(SteamMan.Initialized)
			{
				name		=SteamFriends.GetPersonaName();
				mLocalID	=SteamUser.GetSteamID().m_SteamID;

				Debug.Log("Player Start() " + name + ", " + mLocalID
					+ ", " + IsLocalPlayer);

			}
			else
			{
				//maybe running multiple instances on the same machine
				name		=GetRandomName();
				mLocalID	=GetRandomID();
			}
		}
	}


	void Update()
	{
		base.Update();

		if(IsServer || !IsSpawned)
		{
			return;
		}
	}


	string	GetRandomName()
	{
		Color	part0	=Random.ColorHSV();
		Color	part1	=Random.ColorHSV();
		Color	part2	=Random.ColorHSV();
		Color	part3	=Random.ColorHSV();

		string	ret	="" + (char)((part0.r * 26f) + 65);

		ret	+=(char)((part0.g * 26f) + 97);
		ret	+=(char)((part0.b * 26f) + 97);
		ret	+=(char)((part0.a * 26f) + 97);

		ret	+=(char)((part1.r * 26f) + 97);
		ret	+=(char)((part1.g * 26f) + 97);
		ret	+=(char)((part1.b * 26f) + 97);
		ret	+=(char)((part1.a * 26f) + 97);

		ret	+=(char)((part2.r * 26f) + 97);
		ret	+=(char)((part2.g * 26f) + 97);
		ret	+=(char)((part2.b * 26f) + 97);
		ret	+=(char)((part2.a * 26f) + 97);

		ret	+=(char)((part3.r * 26f) + 97);
		ret	+=(char)((part3.g * 26f) + 97);
		ret	+=(char)((part3.b * 26f) + 97);
		ret	+=(char)((part3.a * 26f) + 97);

		return	ret;
	}


	ulong	GetRandomID()
	{
		//lazy, not a great steamID
		return	(ulong)Random.Range(1, int.MaxValue);
	}
}

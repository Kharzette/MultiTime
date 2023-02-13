using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;


public class Player : Mobile
{
	ulong	mLocalID;	//steam id valid on client

	PlayerMove	mPM;
	PlayerCam	mMNC;


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

			//hunt for noggin bone
			Transform	noggin	=null;

			int	count	=transform.childCount;
			for(int i=0;i < count;i++)
			{
				Transform	t	=transform.GetChild(i);

				//will eventually replace this with real skeleton stuff
				if(t.name == "FakeNogginBone")
				{
					noggin	=t;
					break;
				}
			}

			CharacterController	cc	=GetComponent<CharacterController>();
			PlayerInputs		pi	=GetComponent<PlayerInputs>();

			mPM		=new PlayerMove(cc, transform, pi);
			mMNC	=new PlayerCam(cc, noggin, transform, pi);
		}
	}


	internal override void Update()
	{
		base.Update();

		if(!IsClient || !IsSpawned)
		{
			return;
		}
	}

	void FixedUpdate()
	{
		if(!IsClient || !IsSpawned)
		{
			return;
		}

		bool	bSnapCam;
		mMNC.UpdateTurn(out bSnapCam);

		Vector3	moveVec;
		mPM.Update(out moveVec);

		mMNC.UpdateCam(moveVec, bSnapCam);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;


public class Mobile : NetworkBehaviour
{
	TMP_Text	mNameText;


	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		Debug.Log("Network spawn! " + name + ", " + ", " + IsLocalPlayer);

		mNameText	=GetComponentInChildren<TMP_Text>();

		//attach camera
		if(IsLocalPlayer)
		{
			GameObject	go	=GameObject.Find("Main Camera");

			go.transform.SetParent(transform);
		}
	}


	void Start()
	{
	}


	internal virtual void Update()
	{
		if(!IsSpawned || !IsClient)
		{
			return;
		}

		//update overhead text facing and contents
		if(mNameText != null)
		{
			if(mNameText.text != name)
			{
				mNameText.text	=name;
			}
			mNameText.transform.rotation	=Camera.main.transform.rotation;
		}
		else
		{

		}
	}
}

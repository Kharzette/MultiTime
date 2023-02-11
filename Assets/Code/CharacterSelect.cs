using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CharacterSelect : MonoBehaviour
{
	public Button	mLeftArrow, mRightArrow;
	public Button	mEnterWorld;


	void Start()
	{
		mLeftArrow.onClick.AddListener(OnLeftClick);
		mRightArrow.onClick.AddListener(OnRightClick);

		mEnterWorld.onClick.AddListener(OnEnterWorld);
	}


	void Update()
	{
	}


	void	OnLeftClick()
	{
		Debug.Log("Left Click!");
	}


	void	OnRightClick()
	{
		Debug.Log("Right Click!");
	}


	void	OnEnterWorld()
	{
		Debug.Log("Enter World!");
	}
}

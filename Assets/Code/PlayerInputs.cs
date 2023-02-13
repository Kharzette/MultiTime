using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInputs : MonoBehaviour
{
	//variables set by input
	internal Vector2	mMoveVec, mMouseDelta;
	internal bool		mbJump, mbWalk, mbAscend, mbDescend;


	void	OnMove(InputValue val)
	{
		mMoveVec	=val.Get<Vector2>();
	}

	void OnJump(InputValue val)
	{
		mbJump	=val.isPressed;
	}

	void OnToggleMouseLook(InputValue val)
	{
		if(val.isPressed)
		{
			Cursor.lockState	=CursorLockMode.Locked;
		}
		else
		{
			Cursor.lockState	=CursorLockMode.None;
		}
	}

	void OnLook(InputValue val)
	{
		mMouseDelta	=val.Get<Vector2>();
	}

	void OnAscend(InputValue val)
	{
		mbAscend	=val.isPressed;
	}

	void OnDescend(InputValue val)
	{
		mbDescend	=val.isPressed;
	}
}

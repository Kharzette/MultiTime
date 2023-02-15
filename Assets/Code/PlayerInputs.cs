using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInputs : MonoBehaviour
{
	//polled mouse values
	Vector2	mPrevMouse, mCurMouse;

	internal Vector2	mMouseDelta;	

	//variables set by input
	internal Vector2	mMoveVec;
	internal bool		mbMouseLook;
	internal bool		mbJump, mbWalk, mbAscend, mbDescend;


	internal void	PollMouse()
	{
		mPrevMouse	=mCurMouse;

		mCurMouse	=Mouse.current.position.ReadValue();

		mMouseDelta	=mCurMouse - mPrevMouse;

		if(mbMouseLook)
		{
			//reset cursor pos to center
			Cursor.lockState	=CursorLockMode.Locked;
			Cursor.lockState	=CursorLockMode.None;
			Cursor.visible		=false;
		}
		else
		{
			Cursor.visible	=true;
		}
	}


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
		mbMouseLook	=val.isPressed;
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

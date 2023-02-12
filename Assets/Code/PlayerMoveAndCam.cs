using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMoveAndCam : MonoBehaviour
{
	Rigidbody	mRigidBody;
	Camera		mMyCam;
	float		mCamDist;

	//variables set by input
	Vector2	mMoveVec, mMouseDelta;
	bool	mbJump, mbWalk;


	const float	MoveSpeed	=700f;
	const float	ZoomSpeed	=1f;
	const float	TurnSpeed	=7f;
	const float	JumpSpeed	=50f;
	const float	MinZoom		=5f;
	const float	MaxZoom		=25;
	const float	WalkSpeed	=0.5f;
	const float	RunSpeed	=2f;


	void Start()
	{
		mRigidBody	=GetComponent<Rigidbody>();

		GameObject	go	=GameObject.Find("Main Camera");

		mMyCam	=go.GetComponent<Camera>();

		mCamDist	=mMyCam.transform.localPosition.magnitude;
	}
	

	void Update()
	{
		//no binding options for this
		Vector2	wheel	=Mouse.current.scroll.ReadValue();

		bool	bSnapCameraLocal	=false;

		if(wheel.y != 0f)
		{
			mCamDist	-=ZoomSpeed * wheel.y * Time.deltaTime;
			mCamDist	=Mathf.Clamp(mCamDist, MinZoom, MaxZoom);

			bSnapCameraLocal	=true;
		}

		//rotate camera if right mouse held
		if(Cursor.lockState == CursorLockMode.Locked)
		{
			Vector3	ang	=mMyCam.transform.eulerAngles;

			ang.y	+=(mMouseDelta.x * TurnSpeed * Time.deltaTime);

			//do camera look up down
			ang.x	-=(mMouseDelta.y * TurnSpeed * Time.deltaTime);

			ang.x	=Mathf.Clamp(ang.x, 0.01f, 65f);

			mMyCam.transform.eulerAngles	=ang;

			bSnapCameraLocal	=true;
		}
		else
		{
			Cursor.lockState	=CursorLockMode.None;
		}

		Vector3	move	=mMyCam.transform.forward * mMoveVec.y;

		//flatten
		move.y	=0f;

		move	+=mMoveVec.x * mMyCam.transform.right;

		if(move.magnitude > 0f)
		{
			//grab a copy of the world camera direction
			Quaternion	camCopy	=mMyCam.transform.rotation;

			move.Normalize();

			float	speed	=RunSpeed;
			if(mbWalk)
			{
				speed	=WalkSpeed;
			}

			mRigidBody.AddForce(move * Time.deltaTime * (MoveSpeed * speed));

//			transform.position	+=move * Time.deltaTime * (MoveSpeed * sprint);

			//rotate player to face camera direction if moving
			Vector3	dir	=mMyCam.transform.position - transform.position;

			//flatten
			dir.y	=0f;
			dir.Normalize();

			//when moving, rotate the player to face
			transform.rotation	=Quaternion.LookRotation(-dir);

			//adjust camera position for new rotation
			mMyCam.transform.rotation	=camCopy;

			//this can cause roll sometimes to build up slowly
			Vector3		eul	=mMyCam.transform.localEulerAngles;

			//clear the roll
			eul.z	=0;

			mMyCam.transform.localEulerAngles	=eul;

			bSnapCameraLocal	=true;
		}

		if(mbJump)
		{
			mRigidBody.AddRelativeForce(Vector3.up
				* Time.deltaTime * JumpSpeed, ForceMode.Impulse);
		}

		if(bSnapCameraLocal)
		{
			Vector3	dir	=mMyCam.transform.localRotation * -Vector3.forward;

			mMyCam.transform.localPosition	=dir * mCamDist;
		}

//		print("Sprint: " + sprint);
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
}
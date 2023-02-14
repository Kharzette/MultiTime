using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


internal class PlayerCam
{
	//references from owner object
	CharacterController	mCC;
	Camera				mMyCam;
	Transform			mNogginBone, mObjXForm;

	float			mCamDist, mCamTargetDist, mLerpTime;
	PlayerInputs	mPI;


	const float TurnSpeed			=3f;
	const float	ZoomSpeed			=1f;
	const float	MinZoom				=5f;
	const float	MaxZoom				=25;
	const float	MaxUpDownLookAngle	=55f;
	const float	CamSphereRadius		=0.15f;	//size of collision sphere around camera
	const float	ZoomLerpTime		=3f;	//time to smooth out zooms


	internal PlayerCam(CharacterController cc, Transform head, Transform obj, PlayerInputs pi)
	{
		mCC			=cc;
		mPI			=pi;
		mNogginBone	=head;
		mObjXForm	=obj;

		GameObject	go	=GameObject.Find("Main Camera");

		mMyCam	=go.GetComponent<Camera>();

		mCamDist		=mMyCam.transform.localPosition.magnitude;
		mCamTargetDist	=mCamDist;
	}


	internal void UpdateTurn(out bool bSnapCameraLocal)
	{
		//no binding options for this
		Vector2	wheel	=Mouse.current.scroll.ReadValue();

		bSnapCameraLocal	=false;

		if(wheel.y != 0f)
		{
			mCamTargetDist	-=ZoomSpeed * wheel.y * Time.deltaTime;
			mCamTargetDist	=Mathf.Clamp(mCamTargetDist, MinZoom, MaxZoom);

			bSnapCameraLocal	=true;
			mLerpTime			=ZoomLerpTime;
		}

		//rotate camera if right mouse held
		if(Cursor.lockState == CursorLockMode.Locked)
		{
			Vector3	ang	=mMyCam.transform.eulerAngles;

			ang.y	+=(mPI.mMouseDelta.x * TurnSpeed * Time.deltaTime);

			//do camera look up down
			ang.x	-=(mPI.mMouseDelta.y * TurnSpeed * Time.deltaTime);

			//wrap into negatives for easier clamping
			if(ang.x > 180f)
			{
				ang.x	=-(360f - ang.x);
			}
			ang.x	=Mathf.Clamp(ang.x, -MaxUpDownLookAngle, MaxUpDownLookAngle);

			mMyCam.transform.eulerAngles	=ang;

			bSnapCameraLocal	=true;
		}
		else
		{
			Cursor.lockState	=CursorLockMode.None;
		}
	}

	internal void UpdateCam(Vector3 moveVec, bool bSnapCameraLocal)
	{
		bool	bGrounded	=mCC.isGrounded;

		if(mLerpTime > 0f)
		{
			float	lerpPercent	=mLerpTime / ZoomLerpTime;

			mCamDist	=Mathf.Lerp(mCamTargetDist, mCamDist, lerpPercent);
		}

		if(moveVec != Vector3.zero)
		{
			//grab a copy of the world camera direction
			Quaternion	camCopy	=mMyCam.transform.rotation;

			moveVec.Normalize();

			//rotate player to face camera direction if moving
			Vector3	pdir	=mMyCam.transform.position - mObjXForm.position;

			//flatten
			pdir.y	=0f;
			pdir.Normalize();

			//when moving, rotate the player to face
			mObjXForm.rotation	=Quaternion.LookRotation(-pdir);

			//adjust camera position for new rotation
			mMyCam.transform.rotation	=camCopy;

			//this can cause roll sometimes to build up slowly
			Vector3		eul	=mMyCam.transform.localEulerAngles;

			//clear the roll
			eul.z	=0;

			mMyCam.transform.localEulerAngles	=eul;

			bSnapCameraLocal	=true;
		}

		Vector3	dir			=mMyCam.transform.localRotation * -Vector3.forward;
		Vector3	worldDir	=mMyCam.transform.rotation * Vector3.forward;

		if(bSnapCameraLocal)
		{
			mMyCam.transform.localPosition	=dir * mCamDist;
		}


		//see if camera is in a bad spot
		RaycastHit	hit;
		if(Physics.SphereCast(mNogginBone.position, CamSphereRadius, -worldDir,
					out hit, mCamTargetDist))
		{
			//hit something, shorten camDist
			mCamDist	=hit.distance;
			mCamDist	=Mathf.Clamp(mCamDist, MinZoom, MaxZoom);

			//redo position
			mMyCam.transform.localPosition	=dir * mCamDist;
		}
		else
		{
			//no cam collision, see if a lerp needed
			if(mCamDist != mCamTargetDist)
			{
				mLerpTime	=ZoomLerpTime;
			}
		}

		mLerpTime	-=Time.deltaTime;
		mLerpTime	=Mathf.Clamp(mLerpTime, 0f, ZoomLerpTime);
	}
}
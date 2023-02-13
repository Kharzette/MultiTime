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

	float			mCamDist;
	PlayerInputs	mPI;
	Vector3			mVelocity;


	const float TurnSpeed			=3f;
	const float	ZoomSpeed			=1f;
	const float	MinZoom				=5f;
	const float	MaxZoom				=25;
	const float	MaxUpDownLookAngle	=55f;
	const float	CamSphereRadius		=0.15f;	//size of collision sphere around camera

	//physics
	const float	JogMoveForce		=1.7f;	//Fig Newtons
	const float	FlyMoveForce		=1f;	//Fig Newtons
	const float	FlyUpMoveForce		=3f;	//Fig Newtons
	const float	MidAirMoveForce		=0.4f;	//Slight wiggle midair
	const float	SwimMoveForce		=0.9f;	//Swimmery
	const float	SwimUpMoveForce		=0.9f;	//Swimmery
	const float	StumbleMoveForce	=0.7f;	//Fig Newtons
	const float	JumpForce			=400;	//leapometers
	const float	GravityForce		=19.6f;	//Gravitons
	const float	BouyancyForce		=70f;	//Gravitons
	const float	GroundFriction		=10f;	//Frictols
	const float	StumbleFriction		=6f;	//Frictols
	const float	AirFriction			=0.1f;	//Frictols
	const float	FlyFriction			=2f;	//Frictols
	const float	SwimFriction		=10f;	//Frictols


	internal PlayerCam(CharacterController cc, Transform head, Transform obj, PlayerInputs pi)
	{
		mCC			=cc;
		mPI			=pi;
		mNogginBone	=head;
		mObjXForm	=obj;

		GameObject	go	=GameObject.Find("Main Camera");

		mMyCam	=go.GetComponent<Camera>();

		mCamDist	=mMyCam.transform.localPosition.magnitude;
	}
	

	internal void Update()
	{
		bool	bGrounded	=mCC.isGrounded;
		if(bGrounded && mVelocity.y < 0)
		{
			mVelocity.y	=0f;
		}

		Vector3	startPos	=mObjXForm.position;

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

		//TODO: check contents / move modes
		bool	bGroundMove	=false;
		Vector3	move	=UpdateGround(out bGroundMove);

		//flip ground move as it returns as a jump bool
		bGroundMove	=!bGroundMove;

		//run collision
		mCC.Move(move - startPos);

		//get resulting move vec
		move	=mObjXForm.position - startPos;

		if(move != Vector3.zero)
		{
			//grab a copy of the world camera direction
			Quaternion	camCopy	=mMyCam.transform.rotation;

			move.Normalize();

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
		if(Physics.SphereCast(mNogginBone.position, CamSphereRadius, -worldDir, out hit, mCamDist))
		{
			//hit something, shorten camDist
			mCamDist	=hit.distance;
			mCamDist	=Mathf.Clamp(mCamDist, MinZoom, MaxZoom);

			//redo position
			mMyCam.transform.localPosition	=dir * mCamDist;
		}

		//debug camera raycast
//		Debug.DrawLine(mNogginBone.position, mNogginBone.position - (worldDir * mCamDist));

//		print("Sprint: " + sprint);
	}


	void AccumulateVelocity(Vector3 moveVec)
	{
		mVelocity	+=moveVec * 0.5f;
	}

	void ApplyFriction(float secDelta, float friction)
	{
		mVelocity	-=(friction * mVelocity * secDelta * 0.5f);
	}

	void ApplyForce(float force, Vector3 direction, float secDelta)
	{
		mVelocity	+=direction * force * (secDelta * 0.5f);
	}


	Vector3 UpdateFly()
	{
		Vector3	startPos	=mObjXForm.position;

		Vector3	moveVec	=mMyCam.transform.forward * mPI.mMoveVec.y;

		moveVec	+=mPI.mMoveVec.x * mMyCam.transform.right;

		moveVec	*=FlyMoveForce;

		AccumulateVelocity(moveVec);
		ApplyFriction(Time.deltaTime, FlyFriction);

		Vector3	pos	=startPos;

		if(mPI.mbAscend)
		{
			ApplyForce(FlyUpMoveForce, Vector3.up, Time.deltaTime);
		}
		pos	+=mVelocity * Time.deltaTime;

		AccumulateVelocity(moveVec);
		ApplyFriction(Time.deltaTime, FlyFriction);

		if(mPI.mbAscend)
		{
			ApplyForce(FlyUpMoveForce, Vector3.up, Time.deltaTime);
		}

		return	pos;
	}


	Vector3 UpdateGround(out bool bJumped)
	{
		bool	bGravity	=false;
		float	friction	=GroundFriction;

		bJumped	=false;

		if(!mCC.isGrounded)
		{
			bGravity	=true;
			friction	=AirFriction;
		}
		else
		{
			if(mPI.mbJump)
			{
				friction	=AirFriction;
				bJumped		=true;
			}
		}

		Vector3	startPos	=mObjXForm.position;
		
		Vector3	moveVec	=mMyCam.transform.forward * mPI.mMoveVec.y;

		//flatten
		moveVec.y	=0f;
		moveVec		+=mPI.mMoveVec.x * mMyCam.transform.right;

		if(mCC.isGrounded)
		{
			moveVec	*=JogMoveForce;
		}
		else
		{
			moveVec	*=MidAirMoveForce;
		}

		AccumulateVelocity(moveVec);
		ApplyFriction(Time.deltaTime, friction);

		Vector3	pos	=startPos;

		if(bGravity)
		{
			ApplyForce(GravityForce, Vector3.down, Time.deltaTime);
		}
		if(bJumped)
		{
			ApplyForce(JumpForce, Vector3.up, Time.deltaTime);

			//jump use a 60fps delta time for consistency
			pos	+=mVelocity * (1f/60f);
		}
		else
		{
			pos	+=mVelocity * Time.deltaTime;
		}

		AccumulateVelocity(moveVec);
		ApplyFriction(Time.deltaTime, friction);
		if(bGravity)
		{
			ApplyForce(GravityForce, Vector3.down, Time.deltaTime);
		}
		if(bJumped)
		{
			ApplyForce(JumpForce, Vector3.up, Time.deltaTime);
		}

		return	pos;
	}
}
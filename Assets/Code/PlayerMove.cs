using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove
{
	//references from owner object
	CharacterController	mCC;
	Camera				mMyCam;
	ClientTransform		mObjXForm;

	PlayerInputs	mPI;
	Vector3			mVelocity;


	//physics
	const float	JogMoveForce		=1.7f;	//Fig Newtons
	const float	FlyMoveForce		=1f;	//Fig Newtons
	const float	FlyUpMoveForce		=3f;	//Fig Newtons
	const float	MidAirMoveForce		=0.1f;	//Slight wiggle midair
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


	internal PlayerMove(CharacterController cc, ClientTransform obj, PlayerInputs pi)
	{
		mCC			=cc;
		mPI			=pi;
		mObjXForm	=obj;

		GameObject	go	=GameObject.Find("Main Camera");

		mMyCam	=go.GetComponent<Camera>();
	}
	

	internal void Update(out Vector3 moveVec)
	{
		bool	bGrounded	=mCC.isGrounded;
		if(bGrounded && mVelocity.y < 0)
		{
			mVelocity.y	=0f;
		}

		Vector3	startPos	=mObjXForm.transform.position;

		//TODO: check contents / move modes
		bool	bGroundMove	=false;
		Vector3	move	=UpdateGround(out bGroundMove);

		//flip ground move as it returns as a jump bool
		bGroundMove	=!bGroundMove;

		//run collision
		mCC.Move(move - startPos);

		//get resulting move vec
		moveVec	=mObjXForm.transform.position - startPos;
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
		Vector3	startPos	=mObjXForm.transform.position;

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

		Vector3	startPos	=mObjXForm.transform.position;
		
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

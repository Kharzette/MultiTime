using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove
{
	//references from owner object
	CharacterController	mCC;
	Camera				mMyCam;
	Transform			mObjXForm;
	Transform			mNoggin;

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


	internal PlayerMove(CharacterController cc, Transform obj, Transform noggin, PlayerInputs pi)
	{
		mCC			=cc;
		mPI			=pi;
		mObjXForm	=obj;
		mNoggin		=noggin;

		GameObject	go	=GameObject.Find("Main Camera");

		mMyCam	=go.GetComponent<Camera>();
	}
	

	internal void Update(float secDelta, out Vector3 moveVec)
	{
		bool	bGrounded	=mCC.isGrounded;
		if(bGrounded && mVelocity.y < 0)
		{
			mVelocity.y	=0f;
		}

		Vector3	startPos	=mObjXForm.transform.position;

		//TODO: check contents / move modes
		bool	bGroundMove	=false;
		Vector3	move	=UpdateGround(secDelta, out bGroundMove);

		//flip ground move as it returns as a jump bool
		bGroundMove	=!bGroundMove;

		//run collision
		mCC.Move(move - startPos);

		//get resulting move vec
		moveVec	=mObjXForm.transform.position - startPos;
	}


	//check to see if a bipedal player can climb out of liquid
	//onto some solid ground.  Assumes player in water
	bool	bCanClimbOut()
	{
		Vector3	midPos	=mCC.center;
		Vector3	eyePos	=mNoggin.localToWorldMatrix.MultiplyPoint(Vector3.zero);

		//not sure how one obtains volume contents in unity yet
		/*
		UInt32	contents	=mZone.GetWorldContents(eyePos);
		if(contents != 0)
		{
			mST.ModifyStringText(mFonts[0], "Eye Contents: " + contents, "ClimbStatus");
			return	false;
		}

		//noggin is in empty space
		//Not to be confused with empty contents
		BoundingBox	crouchBox	=Misc.MakeBox(PlayerBoxWidth,
			PlayerBoxCrouching, PlayerBoxWidth);

		//trace upward to about crouch height above the eye
		Vector3	traceStart	=eyePos;
		Vector3	traceTarget	=eyePos + Vector3.Up * PlayerBoxCrouching;

		Collision	col;
		bool	bHit	=mZone.TraceAll(null, crouchBox,
			traceStart, traceTarget, out col);
		if(bHit)
		{
			mST.ModifyStringText(mFonts[0], "Eye Contents: " + contents +
				", Head hit something...", "ClimbStatus");
			return	false;	//banged into something
		}

		//get a horizon leveled view direction
		//cam direction backward
		Vector3	flatLookVec	=-mGD.GCam.Forward;
		flatLookVec.Y	=0f;
		flatLookVec.Normalize();

		//trace forward about 1.5 box widths
		traceStart	=traceTarget;
		traceTarget	+=flatLookVec * (PlayerBoxWidth * 1.5f);

		bHit	=mZone.TraceAll(null, crouchBox,
			traceStart, traceTarget, out col);
		if(bHit)
		{
			mST.ModifyStringText(mFonts[0], "Eye Contents: " + contents +
				", Forward trace hit something...", "ClimbStatus");
			return	false;	//banged into something
		}

		//trace down to check for solid ground
		traceStart	=traceTarget;
		traceTarget	+=Vector3.Down * (PlayerBoxCrouching * 2f);

		bHit	=mZone.TraceAll(null, crouchBox,
			traceStart, traceTarget, out col);
		if(!bHit)
		{
			mST.ModifyStringText(mFonts[0], "Eye Contents: " + contents +
				", Down trace empty...", "ClimbStatus");
			return	false;	//nothing to climb onto
		}

		//see if the ground is good
		return	col.mPlaneHit.IsGround();
*/
		return	false;
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


	Vector3 UpdateSwimming(float secDelta)
	{
		Vector3	startPos	=mObjXForm.transform.position;

		Vector3	moveVec	=mMyCam.transform.forward * mPI.mMoveVec.y;

		moveVec	+=mPI.mMoveVec.x * mMyCam.transform.right;

		moveVec	*=SwimMoveForce;

		AccumulateVelocity(moveVec);

		Vector3	pos	=startPos;

		if(mPI.mbAscend)
		{
			if(bCanClimbOut())
			{
				ApplyForce(JumpForce, Vector3.up, secDelta);
			}
			else
			{
				ApplyForce(SwimUpMoveForce, Vector3.up, secDelta);
			}
		}

		//friction / gravity / bouyancy
		ApplyFriction(secDelta, SwimFriction);
		ApplyForce(GravityForce, Vector3.down, secDelta);
		ApplyForce(BouyancyForce, Vector3.up, secDelta);

		pos	+=mVelocity * secDelta;

		mVelocity	+=moveVec * 0.5f;

		if(mPI.mbAscend)
		{
			ApplyForce(SwimUpMoveForce, Vector3.up, secDelta);
		}

		//friction / gravity / bouyancy
		ApplyFriction(secDelta, SwimFriction);
		ApplyForce(GravityForce, Vector3.down, secDelta);
		ApplyForce(BouyancyForce, Vector3.up, secDelta);

		return	pos;
	}

	Vector3 UpdateFly(float secDelta)
	{
		Vector3	startPos	=mObjXForm.transform.position;

		Vector3	moveVec	=mMyCam.transform.forward * mPI.mMoveVec.y;

		moveVec	+=mPI.mMoveVec.x * mMyCam.transform.right;

		moveVec	*=FlyMoveForce;

		AccumulateVelocity(moveVec);
		ApplyFriction(secDelta, FlyFriction);

		Vector3	pos	=startPos;

		if(mPI.mbAscend)
		{
			ApplyForce(FlyUpMoveForce, Vector3.up, secDelta);
		}
		pos	+=mVelocity * secDelta;

		AccumulateVelocity(moveVec);
		ApplyFriction(secDelta, FlyFriction);

		if(mPI.mbAscend)
		{
			ApplyForce(FlyUpMoveForce, Vector3.up, secDelta);
		}

		return	pos;
	}


	Vector3 UpdateGround(float secDelta, out bool bJumped)
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
		ApplyFriction(secDelta, friction);

		Vector3	pos	=startPos;

		if(bGravity)
		{
			ApplyForce(GravityForce, Vector3.down, secDelta);
		}
		if(bJumped)
		{
			ApplyForce(JumpForce, Vector3.up, secDelta);

			//jump use a 60fps delta time for consistency
			pos	+=mVelocity * (1f/60f);
		}
		else
		{
			pos	+=mVelocity * secDelta;
		}

		AccumulateVelocity(moveVec);
		ApplyFriction(secDelta, friction);
		if(bGravity)
		{
			ApplyForce(GravityForce, Vector3.down, secDelta);
		}
		if(bJumped)
		{
			ApplyForce(JumpForce, Vector3.up, secDelta);
		}

		return	pos;
	}
}

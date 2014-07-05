using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spear : Weapon 
{	
	public Vector3 flyingDir;

	public float stabLength = 3.3f;

	private List<WorldObject>impaleTargets = new List<WorldObject>();
	
	private float impaleVelocity;
	private float imapleDeceleration = 45f;
	private float drawnTimer;

	public override bool isFlying{ get{ return state == FlyingState;} }
	public override bool isGrabbed{ get{ return state == GrabbedState;} }
	public override bool isAttacking{ get{return state == AttackState;} }
	public override bool isDrawn{ get{return state == DrawnState;} }
	public override bool isImpaling{ get{return state == ImpaleState;} }

	public void SetDrawnState()
	{
		drawnTimer = 0;
		state = DrawnState;
	}

	private void DrawnState()
	{
		drawnTimer += Time.deltaTime;
	}

	public override void SetGrabbedState(Character character)
	{
		WorldObject impaleObject = GetImpaleObject();

		owner = character;

		if(impaleObject)
			impaleObject.PickupObject(this, character);

		rigidbody.isKinematic = true;

		state = GrabbedState;
	}
		
	protected override void GrabbedState()
	{
	}

	private void SetFlyingState(float throwForce = 0)
	{
		transform.parent = null;		
		rigidbody.isKinematic = false;

		flyingDir = transform.forward;
		rigidbody.AddForce(flyingDir * throwForce, ForceMode.Impulse);

		state = FlyingState;
	}

	void FlyingState ()
	{	
	}

	public void SetAttackState(Vector3 stabPos)
	{		
		Ray ray = new Ray(stabPos, transform.forward);
		RaycastHit hit = new RaycastHit();
		
		if(Physics.Raycast(ray.origin, ray.direction, out hit, stabLength, LayerManager.GetSpearStab()))
		{
			StabCollision(hit.collider);
		}
		
		//Debug.DrawLine(stabPos, stabPos + transform.forward * stabLength, Color.red, 0.5f);

		state = AttackState;
	}

	public void AttackState ()
	{
	}

	public void SetImpaleState(WorldObject target)
	{
		target.Impale(this, GetImpaleImpulse());		
		impaleTargets.Add(target);
		transform.parent = target.transform;

		impaleVelocity = 7f;
		rigidbody.velocity = Vector3.zero;
		rigidbody.isKinematic = true;
		owner = null;

		state = ImpaleState;
	}

	public void SetImpaleState()
	{		
		impaleVelocity = 7f;
		rigidbody.velocity = Vector3.zero;
		rigidbody.isKinematic = true;
		owner = null;
		
		state = ImpaleState;
	}

	void ImpaleState()
	{
		impaleVelocity -= imapleDeceleration * Time.deltaTime;

		if(impaleVelocity > 0)
		{
			Vector3 pos = transform.position;
			pos += transform.forward * Time.deltaTime * impaleVelocity;
			transform.position = pos;
		}
	}

	public override float GetDrawnTimer()
	{
		return drawnTimer;
	}

	public override void Attack()
	{
		if(!isDrawn)
			return;
		
		Throw(15f);

		owner.RangedAttack(true);
	}

	public override void Draw ()
	{
		if(isAttacking || isGrabbed || isDrawn)
			return;

		owner.Draw();

		SetDrawnState();
	}

	public override void MeleeAttack()
	{
		if(isDrawn || isAttacking)
			return;

		Vector3 pos = owner.hand.position;
		owner.MeleeAttack(transform.forward * 150f);
		SetAttackState(pos);
	}

	public override void MeleeReturn()
	{
		if(!isAttacking)
			return;
		
		Pickup();

		//***Replaced with animations
		transform.parent = owner.hand.transform;
		transform.position = owner.hand.position;
		transform.rotation = owner.hand.rotation;

		owner.MeleeAttackReturn();

		SetCarryState();
	}


	public override Vector3 AdjustMovement(Vector3 moveForce)
	{
		if(IsStatic())
			return Vector3.zero;

		if(isGrabbed)
			return moveForce * 0.2f;

		//Stand still on drawn
		//if(isDrawn)
		//	return Vector3.zero;

		//Vector3 adjustedMove = moveForce - moveForce * 2f; //***Get weight here
		//Debug.Log(Vector3.Dot(adjustedMove, moveForce));

		return moveForce;
	}

	public override float AdjustRotation (float rotationForce)
	{
		if(IsStatic())
			return 0f;

		if(isGrabbed)		
			return rotationForce * 0.4f;

		return rotationForce;
	}

	public Vector3 GetImpaleImpulse ()
	{
		return rigidbody.velocity.normalized * 175f;
	}

	public float GetScratchImpulse()
	{
		return 70f;
	}

	public void Throw(float throwForce)
	{		
		transform.parent = null;		
		rigidbody.isKinematic = false;
		
		flyingDir = transform.forward;
		rigidbody.AddForce(flyingDir * throwForce, ForceMode.Impulse);

		SetFlyingState(throwForce);		
	}

	public WorldObject GetImpaleObject()
	{
		//if((state == ImpaleState || state == GrabbedState || state == StabState) && impaleTargets.Count > 0)//***do we need this state check?
		if(impaleTargets.Count > 0)
			return impaleTargets[0];

		return null;
	}

	public override void Pickup (Character character = null)
	{
		if(character)
			owner = character;


		WorldObject impaleObject = GetImpaleObject();
		
		if(impaleObject)
			impaleObject.Withdraw(this, Vector3.zero);
		
		impaleTargets.Clear();

		transform.parent = owner.hand.transform;
		transform.position = owner.hand.position;
		transform.rotation = owner.hand.rotation;

		SetCarryState();		
	}

	public override bool GrabCheck(Character character)
	{
		if(isImpaling || (character.catchMidair && isFlying && owner != character))
		{
			//Cannot pull weapon from self
			WorldObject obj = GetImpaleObject();

			if(obj && obj == character && !character.pullWeaponFromSelf)
				return false;

			return true;
		}
		else
		{
			return false;
		}
	}

	public override bool FreeCheck(Character character)
	{
		if(!isImpaling && !isLying)
			return false;

		//Not attracted by spears in self
		WorldObject obj = GetImpaleObject();
		if(obj && obj == character && !character.pullWeaponFromSelf)
			return false;

		return true;
	}

	public bool IsStatic()
	{
		for(int i = 0, count = impaleTargets.Count; i < count; i++)
		{
			WorldObject target = impaleTargets[i];
			
			if(target.IsStatic())
				return true;
		}

		return false;
	}

	//Collision
	void StabCollision(Collider col)
	{
		//Hitting a character? 
		CharacterCollider characterCollider = col.GetComponent<CharacterCollider>();
		if(!characterCollider)
			return;

		
		//Hitting self?
		Character characterTarget = characterCollider.GetCharacter();	
		if(characterTarget == owner)
			return;

		//Calculate impact distance
		Vector3 targetPos = characterTarget.transform.position;
		targetPos.y = transform.position.y;
		
		Vector3 targetDir = targetPos - transform.position;
		Vector3 projectPos = Vector3.Project(targetDir, transform.forward) + transform.position;
		Vector3 impactDist = targetPos - projectPos;

		if(impactDist.magnitude > characterTarget.hitDistance)
		{
			//Scratch
			characterTarget.Scratch(-impactDist, GetScratchImpulse());
		}
		else
		{
			//Impale character
			characterTarget.Impale(this, Vector3.zero); // add stab force here		
			impaleTargets.Add(characterTarget);
			transform.parent = characterTarget.transform;
						
			//Pickup character
			WorldObject impaleObject = GetImpaleObject();
			Character c = (Character)owner;
			
			if(impaleObject)
				impaleObject.PickupObject(this, c);
		}
	}
	
	void OnTriggerEnter(Collider col)
	{
		if(state != FlyingState)
			return;

		//--//Impale environment//--//
		EnvironmentCollider target = col.GetComponent<EnvironmentCollider>();
		if(target)
		{
			WorldObject worldObject = target.GetWorldObject();
			SetImpaleState(worldObject);
			return;
		}
		
		
		//--//Impale Characters//--//
		CharacterCollider characterCollider = col.GetComponent<CharacterCollider>();
		if(characterCollider)
		{
			Character characterTarget = characterCollider.GetCharacter();
			
			//Hit owner?
			if(characterTarget == owner)
				return;
			
			//Calculate impact distance
			Vector3 targetPos = characterTarget.transform.position;
			targetPos.y = transform.position.y;
			
			Vector3 targetDir = targetPos - transform.position;
			Vector3 projectPos = Vector3.Project(targetDir, transform.forward) + transform.position;
			Vector3 impactDist = targetPos - projectPos;
			
			if(impactDist.magnitude > characterTarget.hitDistance)
			{
				//Scratch
				characterTarget.Scratch(-impactDist, GetScratchImpulse());
			}
			else
			{
				//Impale character
				SetImpaleState(characterTarget);
			}
			
			return;
		}
		
		//Impale regular colliders
		if(!col.isTrigger)
			SetImpaleState();
	}

}

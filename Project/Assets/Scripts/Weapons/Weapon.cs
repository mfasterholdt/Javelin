using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour 
{
	public bool hasRangedAttack;
	public float rangedAttackDist;

	public bool hasMeleeAttack;
	public float meleeAttackDist;

	[HideInInspector]
	public Character owner;

	public virtual bool isFlying{ get{ return false;} }
	public virtual bool isGrabbed{ get{ return false;} }
	public virtual bool isAttacking{ get{return false;} }
	public virtual bool isDrawn{ get{return false;} }
	public virtual bool isFullyDrawn{ get{return false;} }	
	public virtual bool isImpaling{ get{return false;} }

	public virtual bool isCarried{ get{ return state == CarryState;} }	
	public virtual bool isLying {get{return state == LyingState;} }

	public string stateName;
	protected delegate void State();
	protected State state;

	protected virtual void Start()
	{
		if(state == null)
			SetLyingState(Vector3.zero);
	}

	protected virtual void OnEnable()
	{
		WeaponManager.Instance.AddWeapon(this);
	}
	
	protected virtual void OnDisable()
	{
		WeaponManager.Instance.RemoveSpear(this);
	}

	//States	

	public virtual void SetCarryState()
	{
		rigidbody.isKinematic = true;

		state = CarryState;
	}
	
	protected virtual void CarryState()
	{
	}

	public virtual void SetGrabbedState(Character character)
	{	
		owner = character;

		rigidbody.isKinematic = true;
		
		state = GrabbedState;
	}
	
	protected virtual void GrabbedState()
	{
	}

	public virtual void SetLyingState(Vector3 force)
	{
		transform.parent = null;
		
		rigidbody.isKinematic = false;
		rigidbody.velocity = Vector3.zero;
		rigidbody.AddForce(force, ForceMode.Impulse);
		
		owner = null;
		state = LyingState;
	}
	
	protected virtual void LyingState()
	{	
		//Friction
		rigidbody.AddForce(-rigidbody.velocity * 200f * Time.deltaTime);
	}

	protected virtual void FixedUpdate () 
	{
		if(state != null){
			state();
			stateName = state.Method.Name;
		}
		else
		{
			stateName = "null";
		}
	}

	//Methods
	public virtual Vector3 AdjustMovement(Vector3 moveForce)
	{
		return moveForce;
	}

	public virtual float AdjustRotation(float rotationForce)
	{
		return rotationForce;
	}

	public virtual void MeleeAttack(){}

	public virtual void MeleeReturn(){}

	public virtual void Draw(){}
	
	public virtual float GetDrawnTimer()
	{
		return 0f;
	}

	public virtual void Attack(){}
	
	public virtual void Drop(Vector3 force)
	{
		SetLyingState(force);
	}
	
	public virtual void Grab(Character character)
	{
		SetGrabbedState(character);
	}

	public virtual bool GrabCheck(Character character)
	{
		return false;
	}

	public virtual void Pickup(Character character = null)
	{
		if(character)
			owner = character;

		transform.parent = owner.hand.transform;
		transform.position = owner.hand.position;
		transform.rotation = owner.hand.rotation;
		
		SetCarryState();	
	}

	public virtual bool PickupCheck(Character character)
	{
		if(!isGrabbed || owner != character)
			return false;
		else
			return true;	
	}

	public virtual bool FreeCheck(Character character)
	{
		return isLying;
	}

}

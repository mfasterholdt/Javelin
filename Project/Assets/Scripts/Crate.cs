using UnityEngine;
using System.Collections;

public class Crate : WorldObject {

	public bool isCarriable;

	private delegate void State();
	private State state;

	private Rigidbody crateRigidbody;

	void Start()
	{
		crateRigidbody = rigidbody;
		SetIdleState();
	}

	private void SetIdleState()
	{
		state = IdleState;
	}

	void IdleState ()
	{
		transform.parent = null;
		
		if(!crateRigidbody)
		{
			crateRigidbody = gameObject.AddComponent<Rigidbody>();
			crateRigidbody.drag = 0.5f;
			crateRigidbody.mass = 35f;
			crateRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		}
		
		state = IdleState;
	}

	private void SetCarryState()
	{
		if(crateRigidbody)
			Destroy(crateRigidbody);

		state = CarryState;
	}

	void CarryState ()
	{
	}

	void FixedUpdate()
	{
		if(state != null)
			state();
	}
	
	public override void Withdraw(Spear spear, Vector3 force)
	{
		impaleObjects.Remove(spear);
		
		bool currentlyCarried = false;
		
		for(int i = 0, count = impaleObjects.Count; i < count; i++)
		{
			Spear impaleObject = impaleObjects[i];
			
			if(impaleObject.isCarried || impaleObject.isGrabbed)
			{
				currentlyCarried = true;
				break;
			}
		}
		
		if(!currentlyCarried)
			SetIdleState();
	}

	public override bool IsStatic()
	{
		if(isCarriable)
			return false;
		else 
			return true;
	}

	public override void PickupObject(Weapon holdingSpear, Character holdingCharacter)
	{
		if(state != CarryState && isCarriable)
		{
			SetCarryState();
			
			//Snap to target
			Transform t = holdingSpear.transform;
			Vector3 pos = transform.position - ((t.position - t.forward * 0.7f - t.right * 0.4f) - holdingCharacter.pos);
			transform.position = pos;
			
			transform.parent = holdingCharacter.handHolder;
		}
	}
}

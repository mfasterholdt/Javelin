using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hammer : Weapon 
{
	public GameObject idle;
	public GameObject attack;
	public GameObject lying;
	public GameObject ready;

	public override bool isGrabbed{ get{ return state == GrabbedState;} }
	public override bool isAttacking{ get{return state == AttackState;} }
	public override bool isDrawn{ get{return state == DrawnState;} }	
	public override bool isFullyDrawn{ get{return state == DrawnState && drawTimer <= 0;} }	

	public float attackCooldown = 0.5f;
	public float attackDangerTime = 0.1f;
	public float drawDelay = 0.25f;

	private float attackTimer;
	private float drawTimer;

	private List<WorldObject> squashTargets = new List<WorldObject>();

	//States
	public override void SetLyingState(Vector3 force)
	{
		base.SetLyingState(force);

		idle.SetActive(false);
		lying.SetActive(true);
		attack.SetActive(false);
		ready.SetActive(false);
	}

	public override void SetCarryState ()
	{
		idle.SetActive(true);
		lying.SetActive(false);
		attack.SetActive(false);
		ready.SetActive(false);

		squashTargets.Clear();

		rigidbody.isKinematic = true;
		
		state = CarryState;
	}

	public void SetDrawnState()
	{
		idle.SetActive(false);
		lying.SetActive(false);
		attack.SetActive(false);
		ready.SetActive(true);

		drawTimer = drawDelay;
			 
		state = DrawnState;
	}

	public void DrawnState()
	{
		if(drawTimer > 0)
			drawTimer -= Time.deltaTime;
	}

	public void SetAttackState()
	{
		idle.SetActive(false);
		lying.SetActive(false);
		attack.SetActive(true);
		ready.SetActive(false);

		attackTimer = attackCooldown;

		state = AttackState;
	}

	void AttackState ()
	{
		if(attackTimer > 0)
			attackTimer -= Time.deltaTime;
		else
			SetCarryState();
	}

	//Methods
	public override Vector3 AdjustMovement(Vector3 moveForce)
	{
		if(isAttacking || isGrabbed)
			return Vector3.zero;

		if(isDrawn)
			return moveForce * 0.05f;
		else
			return moveForce * 0.5f; 
	}
	
	public override float AdjustRotation (float rotationForce)
	{
		if(isAttacking)
			return 0f;

		return rotationForce * 0.2f;
	}

	public override void MeleeAttack()
	{
		if(isAttacking || !isDrawn)
			return;

		if(isFullyDrawn)
		{
			owner.rigidbody.velocity /= 3f;
			owner.AddForce(transform.forward * 100f);

			SetAttackState();
		}
		else
		{
			SetCarryState();
		}
	}

	public override void MeleeReturn()
	{
		if(!isAttacking || attackTimer > 0)
			return;
		
		SetCarryState();
	}

	public override void Draw ()
	{
		if(isAttacking || isDrawn)
			return;
		
		SetDrawnState();
	}


	//Collision
	void OnTriggerEnter(Collider col)
	{
		if(!isAttacking || attackTimer < attackCooldown - attackDangerTime)
			return;

		//--//Smash Characters//--//
		CharacterCollider characterCollider = col.GetComponent<CharacterCollider>();
		if(characterCollider)
		{
			Character target = characterCollider.GetCharacter();
			
			//Hit owner?
			if(target == owner)
				return;

			//Already hit?
			if(squashTargets.Contains(target))
				return;

			squashTargets.Add (target);
			target.Damage(24);
			target.AddForce(transform.forward * 400f);
		}
	}
}

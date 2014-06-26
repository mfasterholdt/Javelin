using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hammer : Weapon 
{
	public GameObject idle;
	public GameObject attack;
	public GameObject lying;

	public override bool isCarried{ get{ return state == CarryState;} }
	public override bool isGrabbed{ get{ return state == GrabbedState;} }
	public override bool isAttacking{ get{return state == AttackState;} }
	public override bool isLying {get{return state == LyingState;} }

	private float attackCooldown = 0.5f;
	private float attackDangerTime = 0.1f;
	private float attackTimer;

	private List<WorldObject> squashTargets = new List<WorldObject>();

	void Start()
	{
		if(state == null)
			SetLyingState(Vector3.zero);
	}

	//States
	public override void SetLyingState(Vector3 force)
	{
		base.SetLyingState(force);

		idle.SetActive(false);
		lying.SetActive(true);
		attack.SetActive(false);
	}

	public override void SetCarryState ()
	{
		idle.SetActive(true);
		lying.SetActive(false);
		attack.SetActive(false);

		squashTargets.Clear();

		state = CarryState;
	}

	public void SetAttackState()
	{
		idle.SetActive(false);
		lying.SetActive(false);
		attack.SetActive(true);

		attackTimer = attackCooldown;

		state = AttackState;
	}

	void AttackState ()
	{
		if(attackTimer > 0)
			attackTimer -= Time.deltaTime;
	}

	//Methods
	public override bool GrabCheck(Character character)
	{
		return isLying;
	}

	public override Vector3 AdjustMovement(Vector3 moveForce)
	{
		if(isAttacking)
			return Vector3.zero;

		return moveForce * 0.3f;
	}
	
	public override float AdjustRotation (float rotationForce)
	{
		if(isAttacking)
			return 0f;

		return rotationForce * 0.2f;
	}

	public override void MeleeAttack()
	{
		if(isAttacking)
			return;

		owner.rigidbody.velocity /= 3f;
		owner.AddForce(transform.forward * 100f);

		SetAttackState();
	}

	public override void MeleeReturn()
	{
		if(!isAttacking || attackTimer > 0)
			return;
		
		SetCarryState();
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

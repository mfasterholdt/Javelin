using UnityEngine;
using System.Collections;

public class Laser : Weapon 
{
	public Transform chargeVisuals;
	public Transform beam;

	public float attackDelay = 0.5f;
	public float maxChargeTime = 1.5f;
	public float cooldownDelay = 2f;

	public override bool isAttacking{ get{return state == AttackState;} }
	public override bool isDrawn{ get{return state == DrawnState;} }
	public bool isCoolingdown{ get{return state == CooldownState;} }

	private float reach = 30f;
	private float drawnTimer;
	private float attackTimer;
	private Vector3 attackDir;
	private Vector3 impactPos;
	
	private float cooldownTimer;	

	public override void SetLyingState (Vector3 force)
	{
		beam.gameObject.SetActive(false);
		base.SetLyingState (force);
	}

	public void SetDrawnState()
	{
		drawnTimer = 0;
		chargeVisuals.renderer.enabled = true;
		state = DrawnState;
	}
	
	private void DrawnState()
	{
		if(drawnTimer < maxChargeTime)
			drawnTimer += Time.deltaTime;
		else
			drawnTimer = maxChargeTime;

		chargeVisuals.localScale = Vector3.one * (drawnTimer / maxChargeTime); 
	}
		
	public void SetAttackState()
	{	
		chargeVisuals.renderer.enabled = false;
		beam.gameObject.SetActive(true);

		attackDir = transform.forward;
		float charge = drawnTimer / maxChargeTime;

		//Shooter force
		Vector3 force = attackDir;
		force.y = -0.2f;
		
		if(charge >= 1f)
			force *= -1000f;			
		else
			force *= -(250f + charge * 250f);
		
		owner.AddForce(force);

		//Hit single
		Ray ray = new Ray(beam.transform.position, transform.forward);

		if(charge >= 1f && false)
		{
			//Hit all *** 
		}
		else
		{
			RaycastHit hit = new RaycastHit();
			if(Physics.SphereCast(ray, 0.75f, out hit, reach, LayerManager.GetLaserBeam()))
			{
				impactPos = hit.point;
				CharacterCollider characterCollider = hit.collider.GetComponent<CharacterCollider>();

				if(characterCollider)
				{
					Character character = characterCollider.GetCharacter();
					character.ExplosiveImpact(-force, 20);
				}
			}
			else
			{
				impactPos = beam.transform.position + transform.forward * reach;
			}
		}

		//Target force ***

		attackTimer = attackDelay;
		state = AttackState;
	}
	
	public void AttackState ()
	{		
		Vector3 distPath = impactPos - beam.transform.position;
		distPath.y = 0;
		Vector3 newScale = beam.localScale;
		newScale.z = distPath.magnitude + 0.25f;
		beam.localScale = newScale;

		if(attackTimer > 0)
		{
			attackTimer -= Time.deltaTime;
		}
		else
		{
			SetCooldownState();
		}
	}


	public void SetCooldownState()
	{
		beam.gameObject.SetActive(false);		
		cooldownTimer = cooldownDelay;
		state = CooldownState;
	}

	void CooldownState ()
	{
		if(cooldownTimer > 0)
		{
			cooldownTimer -= Time.deltaTime;
		}
		else
		{
			SetCarryState();
		}
	}

	public override void Attack()
	{
		if(!isDrawn)
			return;

		//rigidbody.AddForce(flyingDir * throwForce, ForceMode.Impulse);
		
		SetAttackState();		
	}

	public override void Draw ()
	{
		if(isAttacking || isGrabbed || isDrawn || isCoolingdown)
			return;
		
		//owner.Draw();//***Animation or visualise here
		
		SetDrawnState();
	}


	public override Vector3 AdjustMovement(Vector3 moveForce)
	{		
		if(isDrawn)
			return moveForce * 0.2f;
		else
			return moveForce * 0.7f; 
	}
	
	public override float AdjustRotation (float rotationForce)
	{
		if(isAttacking)
			return 0f;
		if(isDrawn)
			return rotationForce * 0.7f;
		else
			return rotationForce * 0.3f;
	}
}

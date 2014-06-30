using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Character : WorldObject
{	
	public bool isCarriable = true;
	public bool catchMidair;
	public bool pullWeaponFromSelf;
	public int health = 12;

	public Collider mainCollider;

	private List<Weapon> weaponsInReach = new List<Weapon>();

	public Transform aim;
	public Transform handHolder;
	public Transform hand;

	public CollisionEvents grabTrigger;	

	public Renderer visuals;
	public Color deathColor;
	public float hitDistance = 0.63f;

	public float moveSpeed = 3f;
	public float breakSpeed = 0.5f;
	public float maxMoveSpeed = 10f;
	public float accelerationBoost = 0.2f;//Added for start acceleration and direction change
	public float minSpeed = 4f;//Add boost while slower than minSpeed
	public float rotationSpeed = 15f;

	private Transform characterTransform;
	private float currentRotationSpeed;
	private Weapon currentWeapon;
	private float minInput = 0.35f;
	private Rigidbody characterRigidbody;
	private Vector3 aimTarget;

	private delegate void State();
	private State state;

	private bool isDead;

	BloodSplatter bloodSplatter;
	private Vector3 spearHandOffset;

	void Start () 
	{
		characterRigidbody = rigidbody;
		characterTransform = transform;
		currentRotationSpeed = rotationSpeed;

		spearHandOffset = hand.localPosition;

		if(grabTrigger)
		{
			grabTrigger.onTriggerEnter += TriggerEnter;
			grabTrigger.onTriggerExit += TriggerExit;
		}

		SetIdleState();
	}

	void SetIdleState()
	{
		transform.parent = null;

		if(!rigidbody)
		{
			characterRigidbody = gameObject.AddComponent<Rigidbody>();
			characterRigidbody.drag = 0.5f;
			characterRigidbody.mass = 35f;
			characterRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
		}

		state = IdleState;
	}

	void IdleState()
	{
	}

	void SetCarryState()
	{
		if(characterRigidbody)
			Destroy(characterRigidbody);

		state = CarryState;
	}

	void CarryState()
	{
	}

	void FixedUpdate () 
	{
		if(state != null)
		{
			state();

			if(isDead)
				visuals.material.color = Color.Lerp (visuals.material.color, deathColor, Time.deltaTime * 2f);
		}

		/*if(spearReady && aimTimer < 7.5f)
			aimTimer += Time.deltaTime * aimSpeed;*/
	}

	public void Damage(int value)
	{
		health -= value;

		if(health <= 0)
			Die();
	}

	public void Explode(Vector3 force)
	{
		health = 0;
		Die();
		Destroy(gameObject);
	}

	void Die()
	{
		DropWeapon();

		isDead = true;
	}

	public void Move(float inputX, float inputZ)
	{
		Vector3 vel = characterRigidbody.velocity;
		float currentVelocity = vel.magnitude;		
		Vector3 moveForce = Vector3.zero;

		//Input
		moveForce.x = InputToSpeed(inputX, vel.x, currentVelocity);
		moveForce.z = InputToSpeed(inputZ, vel.z, currentVelocity);

		//Adjust movement
		if(currentWeapon)
			moveForce = currentWeapon.AdjustMovement(moveForce);

		characterRigidbody.AddForce(moveForce * Time.deltaTime, ForceMode.Acceleration);
	}

	private float InputToSpeed(float input, float vel, float currentVelocity)
	{
		input = Mathf.Clamp(input, -1f, 1f);

		bool directionChange = Mathf.Sign(input) != Mathf.Sign (vel);
		
		//Base Movement
		float speed = input * moveSpeed;
		
		//Boost start acceleration and direction change
		//***Add dot product check instead of individual velocity checks
		if(directionChange || currentVelocity < minSpeed)
			speed *= (1 + accelerationBoost);
		
		//Max speed
		if(currentVelocity > maxMoveSpeed && !directionChange)
			speed = 0;
		
		//Friction
		if(Mathf.Abs(input) < minInput && Mathf.Abs(vel) > 0.1f)
			speed += Mathf.Sign(vel) * -breakSpeed;

		return speed;
	}
	
	public void Aim(Vector3 target)
	{
		if(target.magnitude > 0.4f)
			aimTarget = target;

		if(aim)
		{
			Vector3 newAim = spearHandOffset;

			float z = 3f;
			/*if(currentWeapon)
				z += currentWeapon.GetDrawnTimer();
			z = Mathf.Min (z, 6f);*/
			    
			newAim.z += z; 

			aim.localPosition = newAim;

			//Debug.DrawLine(hand.position, aim.position, Color.red);
		}

		//Adjust rotation
		currentRotationSpeed = rotationSpeed;		
		if(currentWeapon)
			currentRotationSpeed = currentWeapon.AdjustRotation(currentRotationSpeed);
		
		Vector3 nextAim = Vector3.Slerp(characterTransform.forward, aimTarget, Time.deltaTime * currentRotationSpeed); 
		nextAim.y = 0;
		characterRigidbody.MoveRotation(Quaternion.LookRotation(nextAim));
	}

	public Weapon GetCurrentWeapon()
	{
		return currentWeapon;
	}

	public Weapon GetCarriedWeapon()
	{
		if(currentWeapon && currentWeapon.isCarried)
			return currentWeapon;
		else
			return null;
	}

	public List<Weapon> GetWeaponsInReach()
	{
		return weaponsInReach;
	}

	public bool GetIsDead()
	{
		return isDead;
	}

	public Weapon GetGrabbedWeapon()
	{
		if(currentWeapon && currentWeapon.isGrabbed)
			return currentWeapon;
		else
			return null;
	}

	public override Vector3 pos{ get{ return characterTransform.position;} }

	public void GrabWeapon()
	{
		if(currentWeapon)
			return;
		
		Weapon optimalWeapon = null;
		
		for(int i = 0, count = weaponsInReach.Count; i < count; i++)
		{
			Weapon weapon = weaponsInReach[i];
			
			if(weapon.GrabCheck(this))
				optimalWeapon = weapon;
		}

		if(optimalWeapon)
		{
			currentWeapon = optimalWeapon;
										
			characterRigidbody.velocity = Vector3.zero;
			characterRigidbody.transform.rotation = currentWeapon.transform.rotation; //Look at spear
			currentWeapon.Grab(this);
		}
	}

	public void PickupWeapon ()
	{
		if(currentWeapon)
		{
			if(currentWeapon.PickupCheck(this))
				currentWeapon.Pickup();
		}
		else
		{
			Weapon optimalWeapon = null;
			
			for(int i = 0, count = weaponsInReach.Count; i < count; i++)
			{
				Weapon weapon = weaponsInReach[i];
				
				if(weapon.isLying)
					optimalWeapon = weapon;
			}

			if(optimalWeapon)
			{
				currentWeapon = optimalWeapon;

				optimalWeapon.Pickup(this);
			}
		}

		//if(!currentWeapon || !currentWeapon.PickupCheck(this))
		//	return;

		//***Animations? or controlled  by parameter		

	}

	public void DropWeapon()
	{
		if(currentWeapon == null)
			return;

		currentWeapon.Drop(currentWeapon.transform.right * 3.5f);

		ResetSpearHand();

		currentWeapon = null;
	}

	private void ResetSpearHand()
	{
		Vector3 pos = hand.localPosition;
		pos.z = 0;
		hand.localPosition = pos;
	}

	public override bool IsStatic()
	{
		if(isCarriable)
			return false;
		else 
			return true;
	}

	public override void Impale(Spear spear, Vector3 force)
	{
		impaleObjects.Add(spear);

		bloodSplatter = BloodManager.Instance.CreateBloodSplatter(transform, transform.position, -transform.forward);
		bloodSplatter.Splatter();

		if(rigidbody)
		{
			rigidbody.AddForce(force, ForceMode.Impulse);
		}
		else
		{	
			if(mainCollider.attachedRigidbody)
				mainCollider.attachedRigidbody.AddForce(force, ForceMode.Impulse); //***Propegate force upwards, should probably be handle differently*/
		}

		Damage(12);
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

	public override void PickupObject(Weapon holdingSpear, Character holdingCharacter)
	{		
		if(state != CarryState && isCarriable)
		{
			SetCarryState();
			//if(holdingSpear.isStabbing)
			//***Snapping to spear removed for now
			/*{
				//Snap to target
				Transform t = holdingSpear.transform;
				Vector3 pos = transform.position - ((t.position - t.forward * 0.7f - t.right * 0.4f) - holdingCharacter.characterTransform.position);
				transform.position = pos;
			}*/

			characterTransform.parent = holdingCharacter.handHolder;
		}
	}

	public void MeleeAttack(Vector3 force)
	{
		AddForce(force, ForceMode.Impulse); 

		//***Animations? or controlled  by parameter
		Vector3 pos = hand.localPosition;
		pos.z = 1.7f;
		hand.localPosition = pos;
	}

	public void MeleeAttackReturn()
	{
		ResetSpearHand();
	}

	public void Draw()
	{
		//***Animations? or controlled  by parameter
		Vector3 pos = hand.localPosition;
		pos.z = -0.7f;
		hand.localPosition = pos;
	}

	public void RangedAttack(bool thrown)
	{
		//***Animations? or controlled  by parameter

		if(thrown)
			currentWeapon = null;

		ResetSpearHand();
	}

	public void ExplosiveImpact(Vector3 force, int damage)
	{
		if(health > damage)
			Damage(damage);
		else
			Explode(force);
	}

	public void Scratch(Vector3 dir, float force)
	{
		BloodSplatter newSplatter = BloodManager.Instance.CreateBloodSplatter(characterTransform, transform.position + dir, dir);
		newSplatter.Splatter();
		newSplatter.Remove();

		Damage(4);

		if(rigidbody)
		{
			rigidbody.AddForce(-dir.normalized * force, ForceMode.Impulse);
		}
		else
		{
			if(mainCollider.attachedRigidbody)
				mainCollider.attachedRigidbody.AddForce(-dir.normalized * force, ForceMode.Impulse); //***Propegate force upwards, should probably be handle differently
		}
	}
	
	public void AddForce(Vector3 force, ForceMode mode = ForceMode.Impulse)
	{
		if(characterRigidbody)
			characterRigidbody.AddForce(force, ForceMode.Impulse); 
	}

	public void SpawnWeapon(GameObject weaponPrefab)
	{
		GameObject newWeaponObj = Instantiate(weaponPrefab, hand.position, hand.rotation) as GameObject;
		newWeaponObj.transform.parent = hand;
		
		currentWeapon = newWeaponObj.GetComponent<Weapon>();
		currentWeapon.owner = this;

		currentWeapon.Pickup();
	}

	private void TriggerEnter(Collider col)
	{
		Rigidbody r = col.attachedRigidbody;

		if(!r)
			return;

		Weapon weapon = r.GetComponent<Weapon>();

		if(!weapon)
			return;

		if(!weaponsInReach.Contains(weapon))
			weaponsInReach.Add(weapon);
	}

	private void TriggerExit(Collider col)
	{
		Rigidbody r = col.attachedRigidbody;
		
		if(!r)
			return;
		
		Weapon weapon = r.GetComponent<Weapon>();
		
		if(!weapon)
			return;
		
		weaponsInReach.Remove(weapon);	
	}
}

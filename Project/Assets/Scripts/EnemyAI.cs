using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour {

	public GameObject weaponPrefab;
	public bool hasWeapon;
	public bool isPatrolling;

	public Transform[] patrolPath;
	public int pathIndex;

	public float chaseSpeed = 1f;
	public float findSpearSpeed = 1f;

	public float cooldownDelay = 0.5f;
	public float meleeDelay = 1f;
	
	public string stateName;
	public bool debug = false;

	private Vector3 currentPathTarget;

	private Character character;
	private Character target; 

	public delegate void State();
	public State state;

	private float timer;
	private float cooldownTimer;

	private float meleeTimer;

	private Path path;

	void Start () 
	{
		character = GetComponent<Character>();
		path = new Path(character);

		if(hasWeapon)
			character.SpawnWeapon(weaponPrefab);	

		target = Player.Instance.character;

		if(isPatrolling)
			SetPatrolState();
		else
			SetIdleState();
	}

	void SetIdleState()
	{
		timer = Random.Range(0.1f, 0.2f);
		state = IdleState;
	}
	
	void IdleState()
	{
		if(!Player.IsActive)
			return;
		
		//if(PlayerSpotted())
			SetChaseState();
	}

	void SetPatrolState()
	{
		//currentPathTarget = patrolPath[pathIndex].position;
		state = PatrolState;
	}

	void PatrolState()
	{
		//***Disabled for now
		return;

		/*Vector3 dir = currentPathTarget - character.pos;
		float dist = dir.magnitude;

		if(dist < 0.2f)
		{
			pathIndex++;
			if(pathIndex >= patrolPath.Length)
				pathIndex = 0;

			currentPathTarget = patrolPath[pathIndex].position;
		}
		else
		{
			dir.Normalize();
			character.Move(dir.x * 0.15f, dir.z * 0.15f);
		}

		character.Aim(dir.normalized);

		if(!Player.IsActive)
			return;

		if(PlayerSpotted())
			SetChaseState();*/
	}


	void SetChaseState()
	{
		if(target.GetIsDead())
		{
			SetTargetDeadState();
		}
		else if(character.GetCurrentWeapon())
		{
			state = ChaseState;
		}
		else
		{
			SetFindSpearState();
		}
	}

	void ChaseState()
	{
		if(!Player.IsActive)
		{
			AimAtPlayer();
			return;
		}

		//***insert interval here to not update path constantly and not shoot rays at player constantly

		path.UpdatePath(target.pos);

		if(debug)
			path.DrawCurrentPath();

		Vector3 dir = path.GetDirection();
		character.Move(dir.x * chaseSpeed, dir.z * chaseSpeed);

		if(PlayerSpotted())
		{
			float dist = Vector3.Distance(target.pos, character.pos);
			AimAtPlayer();

			Weapon weapon = character.GetCurrentWeapon();

			if(weapon.hasRangedAttack)
			{
				if(dist < weapon.rangedAttackDist || weapon.rangedAttackDist == 0)
				{
					if(dist < 20f)
						SetReadyState();
				}
			}
			else if(weapon.hasMeleeAttack)
			{
				if(dist < weapon.meleeAttackDist || weapon.meleeAttackDist == 0)
				{
					SetMeleeState();
				}
			}
		}
	}

	void SetMeleeState ()
	{
		Weapon weapon = character.GetCurrentWeapon();
		weapon.Draw();

		state = MeleeState;
	}

	void MeleeState ()
	{
		Weapon weapon = character.GetCurrentWeapon();

		if(weapon.isDrawn)
		{
			if(Vector3.Distance(character.pos, target.pos) > 8f)
			{
				SetChaseState();
			}
			else
			{
				float targetAngle = AimAtPlayer();

				if(targetAngle < 50f && weapon.isFullyDrawn)
				{
					weapon.MeleeAttack();

					meleeTimer = meleeDelay;
				}
			}
		}
		else
		{
			if(meleeTimer > 0)
				meleeTimer -= Time.deltaTime;
			else
				SetCooldownState(cooldownDelay);
		}
	}

	void SetCooldownState(float delay)
	{
		cooldownTimer = delay;
		state = CooldownState;
	}

	void CooldownState ()
	{
		if(cooldownTimer > 0)
			cooldownTimer -= Time.deltaTime;
		else
			SetChaseState();
	}	

	void SetReadyState()
	{
		timer = Random.Range(1f, 1.5f);

		state = ReadyState;
	}

	void ReadyState()
	{
		Weapon currentWeapon = character.GetCurrentWeapon();

		if(currentWeapon.isDrawn)
		{
			AimAtPlayer();

			//*** wait for aim to lign up

			if(timer > 0)
				timer -= Time.deltaTime;
			else
				SetThrowState();
		}
		else
		{
			currentWeapon.Draw();
		}
	}

	void SetThrowState()
	{
		Weapon currentWeapon = character.GetCurrentWeapon();
		currentWeapon.Attack();

		state = ThrowState;
	}

	void ThrowState()
	{
		SetFindSpearState();
	}

	void SetFindSpearState()
	{
		state = FindSpearState;
	}

	void FindSpearState()
	{
		Weapon grabbedWeapon = character.GetGrabbedWeapon();
		
		if(grabbedWeapon)
		{
			//Pickup spear and chase
			character.PickupWeapon();
			Weapon currentWeapon = character.GetCurrentWeapon();

			if(currentWeapon && currentWeapon.isCarried)			
				SetChaseState();
		}
		else
		{
			//Find spear
			Weapon nearestWeapon = WeaponManager.Instance.GetNearestFreeWeapon(character.pos, character);
			
			if(nearestWeapon)
			{
				Vector3 spearPos = nearestWeapon.transform.position - nearestWeapon.transform.forward;
				path.UpdatePath(spearPos);

				Vector3 dir = path.GetDirection();
				character.Move(dir.x * findSpearSpeed, dir.z * findSpearSpeed);
			}

			if(debug)
				path.DrawCurrentPath();

			//Grab spear
			if(character.GetWeaponsInReach().Count > 0)
			{
				character.GrabWeapon();
			}
		}
	}

	void SetTargetDeadState()
	{
		state = TargetDeadState;
	}

	void TargetDeadState ()
	{
	}
	
	void FixedUpdate () 
	{
		if(character.GetIsDead())
			return;

		if(state != null)
		{
			state();

			stateName = state.Method.Name;
		}
		else
		{
			stateName = "null";
		}
	}

	private bool PlayerSpotted()
	{
		//***add aditional spot logic here
		Vector3 lookDir = target.pos - character.pos;
		Ray ray = new Ray(character.pos + lookDir.normalized, lookDir);
		RaycastHit hit = new RaycastHit();

		if(Physics.Raycast(ray, out hit))
		{
			Rigidbody r = hit.collider.attachedRigidbody;

			if(r && r.gameObject.GetComponent<Player>())
			{
				return true;
			}
		}
		
		return false;
	}
	
	protected float AimAtPlayer()
	{
		Vector3 aim = target.pos - character.hand.transform.position;
		character.Aim(aim);

		return Vector3.Angle(aim, character.handHolder.transform.forward);
	}

	void OnDrawGizmos()
	{
		if(hasWeapon)
		{
			Character enemy = GetComponent<Character>();

			if(enemy)
			{
				//Debug.DrawLine(enemy.spearHand.position - enemy.spearHand.forward, enemy.spearHand.position + enemy.spearHand.forward, Color.black);
			}
		}
	}
}

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

	private Character enemy;
	private Character target; 

	public delegate void State();
	public State state;

	private float timer;
	private float cooldownTimer;

	private float meleeTimer;

	private Path path;

	void Start () 
	{
		enemy = GetComponent<Character>();
		path = new Path(enemy);

		if(hasWeapon)
			enemy.SpawnWeapon(weaponPrefab);	

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
		currentPathTarget = patrolPath[pathIndex].position;
		state = PatrolState;
	}

	void PatrolState()
	{
		Vector3 dir = currentPathTarget - enemy.pos;
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
			enemy.Move(dir.x * 0.15f, dir.z * 0.15f);
		}

		enemy.Aim(dir.normalized);

		if(!Player.IsActive)
			return;

		if(PlayerSpotted())
			SetChaseState();
	}


	void SetChaseState()
	{
		if(target.GetIsDead())
		{
			SetTargetDeadState();
		}
		else if(enemy.GetCarriedWeapon())
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
		enemy.Move(dir.x * chaseSpeed, dir.z * chaseSpeed);

		if(PlayerSpotted())
		{
			float dist = Vector3.Distance(target.pos, enemy.pos);
			AimAtPlayer();

			Weapon weapon = enemy.GetCurrentWeapon();

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
		state = MeleeState;
	}

	void MeleeState ()
	{
		Weapon weapon = enemy.GetCurrentWeapon();

		if(weapon.isAttacking)
		{
			if(meleeTimer > 0)
			{
				meleeTimer -= Time.deltaTime;
			}
			else
			{
				weapon.MeleeReturn();
				SetCooldownState(cooldownDelay);
			}
		}
		else
		{
			if(Vector3.Distance(enemy.pos, target.pos) > 8f)
			{
				SetChaseState();
			}
			else
			{
				float targetAngle = AimAtPlayer();

				if(targetAngle < 20f)
				{
					weapon.MeleeAttack();
					meleeTimer = meleeDelay;
				}
			}
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
		Weapon currentWeapon = enemy.GetCurrentWeapon();

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
		Weapon currentWeapon = enemy.GetCurrentWeapon();
		currentWeapon.RangedAttack();

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
		Weapon grabbedWeapon = enemy.GetGrabbedWeapon();
		
		if(grabbedWeapon)
		{
			//Pickup spear and chase
			enemy.PickupWeapon();
			Weapon currentWeapon = enemy.GetCurrentWeapon();

			if(currentWeapon && currentWeapon.isCarried)			
				SetChaseState();
		}
		else
		{
			//Find spear
			Weapon nearestWeapon = WeaponManager.Instance.GetNearestFreeWeapon(enemy.pos, enemy);
			
			if(nearestWeapon)
			{
				Vector3 spearPos = nearestWeapon.transform.position - nearestWeapon.transform.forward;
				path.UpdatePath(spearPos);

				Vector3 dir = path.GetDirection();
				enemy.Move(dir.x * findSpearSpeed, dir.z * findSpearSpeed);
			}

			if(debug)
				path.DrawCurrentPath();

			//Grab spear
			if(enemy.GetWeaponsInReach().Count > 0)
			{
				enemy.GrabWeapon();
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
		if(enemy.GetIsDead())
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
		Vector3 lookDir = target.pos - enemy.pos;
		Ray ray = new Ray(enemy.pos + lookDir.normalized, lookDir);
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
		Vector3 aim = target.pos - enemy.hand.transform.position;
		enemy.Aim(aim);

		return Vector3.Angle(aim, enemy.handHolder.transform.forward);
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

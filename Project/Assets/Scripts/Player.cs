using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour 
{
	[HideInInspector]
	public Character character;

	public GameObject weaponPrefab;
	public bool hasWeapon = true;

	public static Player Instance;

	public static bool IsActive;

	void Awake()
	{
		Player.Instance = this;
		character = GetComponent<Character>();
	}

	void Start () 
	{
		if(hasWeapon)
			character.SpawnWeapon(weaponPrefab);
	}

	void FixedUpdate () 
	{
		if(character.GetIsDead())
			return;

		MovementInput();
		AimInput();
		WeaponInput();

		if(character.GetIsDead())
			Player.IsActive = false;
	}

	private void MovementInput()
	{
		Vector3 input = Vector3.zero;
		input.x = Input.GetAxis("Horizontal");
		input.z = Input.GetAxis("Vertical");

		character.Move(input.x, input.z);
	}

	private void AimInput()
	{
		Vector3 inputAim = Vector3.zero;
		inputAim.x = Input.GetAxis("HorizontalRight");
		inputAim.z = Input.GetAxis("VerticalRight");

		character.Aim(inputAim);
	}

	void WeaponInput ()
	{
		float inputRightTrigger = Input.GetAxis("RightTrigger");
		float inputLeftTrigger = Input.GetAxis("LeftTrigger");

		Weapon currenWeapon = character.GetCurrentWeapon();

		if(currenWeapon)
		{
			if(currenWeapon.isGrabbed)
			{
				//Pickup
				if(inputRightTrigger > 0.8f)
					character.PickupWeapon();
			}
			else
			{
				//Throwing
				if(inputRightTrigger < -0.5f)
				{
					currenWeapon.Draw();
				}
				else if(inputRightTrigger > 0.8f)
				{
					currenWeapon.RangedAttack();
				}
								
				//Stab
				if(inputLeftTrigger < -0.5f)
				{					
					currenWeapon.MeleeAttack();
				}
				else if(inputLeftTrigger > 0.8f)
				{
					currenWeapon.MeleeReturn();
				}
			}
		}
		else
		{
			//Grab weapon
			if(inputRightTrigger < -0.5f)
				character.GrabWeapon();
		}
	

		//Debug spawn spear
		/*
		private float spearSpawnTimer = 2f;
		if(character.GetCurrentSpear() == null)
		{
			if(spearSpawnTimer > 0)
			{
				spearSpawnTimer -= Time.deltaTime;
			}
			else
			{
				character.SpawnSpear();
				spearSpawnTimer = 2f;
			}
		}*/
	}
}

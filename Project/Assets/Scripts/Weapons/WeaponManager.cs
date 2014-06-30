using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour 
{

	public static WeaponManager Instance;
	private List<Weapon> allWeapons = new List<Weapon>();

	void Awake() 
	{
		WeaponManager.Instance = this;
	}

	public void AddWeapon(Weapon weapon)
	{
		allWeapons.Add(weapon);
	}

	public void RemoveSpear (Weapon weapon)
	{
		allWeapons.Remove(weapon);
	}

	public Weapon GetNearestFreeWeapon(Vector3 pos, Character character)
	{
		Weapon nearestWeapon = null;
		float minDist = float.MaxValue;

		for(int i = 0, count = allWeapons.Count; i < count; i++)
		{
			Weapon weapon = allWeapons[i];

			if(weapon.FreeCheck(character))
			{
				float dist = (weapon.transform.position - pos).sqrMagnitude;

				if(dist < minDist)
				{
					nearestWeapon = weapon;
					minDist = dist;
				}
			}
		}

		return nearestWeapon;
	}
}

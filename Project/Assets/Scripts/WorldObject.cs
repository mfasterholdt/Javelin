using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldObject : MonoBehaviour {
	
	protected List<Spear> impaleObjects = new List<Spear>();

	public virtual Vector3 pos{ get{ return transform.position;} }

	public virtual void Impale(Spear spear, Vector3 force)
	{
		impaleObjects.Add(spear);
	}

	public virtual void Withdraw(Spear spear, Vector3 force)
	{
		impaleObjects.Remove(spear);
	}

	public virtual void PickupObject(Weapon holdingSpear, Character holdingCharacter)
	{
	}

	public virtual bool IsStatic(){
		return true;
	}
}

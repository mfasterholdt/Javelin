using UnityEngine;
using System.Collections;

public class CollisionEvents : MonoBehaviour 
{
	public delegate void TriggerEvent(Collider col);
	public TriggerEvent onTriggerEnter;
	public TriggerEvent onTriggerExit;

	public delegate void CollisionEvent(Collision col);	
	public CollisionEvent onCollisionEnter;
	public CollisionEvent onCollisionExit;

	void OnTriggerEnter (Collider col) 
	{
		if(onTriggerEnter != null)
			onTriggerEnter(col);
	}

	void OnTriggerExit (Collider col) 
	{
		if(onTriggerExit != null)
			onTriggerExit(col);
	}

	void OnCollisionEnter(Collision collision)
	{
		if(onCollisionEnter != null)
			onCollisionEnter(collision);
	}

	void OnCollisionExit(Collision collision)
	{
		if(onCollisionExit != null)
			onCollisionExit(collision);
	}
}

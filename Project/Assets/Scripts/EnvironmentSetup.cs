using UnityEngine;
using System.Collections;

public class EnvironmentSetup : MonoBehaviour 
{
	void Awake () 
	{
		Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();

		for(int i = 0, count = colliders.Length; i < count; i++)
		{
			Collider c = colliders[i];

			if(!c.GetComponent<EnvironmentCollider>())
			{
				//Holder
				GameObject holder = new GameObject(c.gameObject.name);
				holder.transform.parent = transform;
				Environment e = holder.AddComponent<Environment>();

				//Collider
				c.transform.parent = holder.transform;
				EnvironmentCollider ec = c.gameObject.AddComponent<EnvironmentCollider>();
				ec.worldObject = e;
			}
		}
	}

	void Update () {
	
	}
}

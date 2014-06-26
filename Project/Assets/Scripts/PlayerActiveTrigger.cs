using UnityEngine;
using System.Collections;

public class PlayerActiveTrigger : MonoBehaviour 
{	

	public bool startActive = false;

	void Start()
	{
		if(startActive)
			Player.IsActive = true;
	}

	void OnTriggerExit (Collider col) {
		Rigidbody r = col.attachedRigidbody;

		if(!r)
			return;

		Player player = r.GetComponent<Player>();

		if(player)
			Player.IsActive = true;
	}
}

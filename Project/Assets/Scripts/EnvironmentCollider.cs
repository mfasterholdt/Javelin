using UnityEngine;
using System.Collections;

public class EnvironmentCollider : MonoBehaviour {
	public WorldObject worldObject;

	public WorldObject GetWorldObject(){
		
		if(worldObject == null)
			Debug.LogError("Missing worldObject on environment collider", gameObject);
		
		return worldObject;
	}
}

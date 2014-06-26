using UnityEngine;
using System.Collections;

public class BloodManager : MonoBehaviour {

	public static BloodManager Instance;

	public GameObject prefabBloodSplatter;

	void Awake () 
	{
		BloodManager.Instance = this;
	}

	public BloodSplatter CreateBloodSplatter(Transform target, Vector3 pos, Vector3 dir)
	{
		GameObject newObject = Instantiate(prefabBloodSplatter, pos, Quaternion.LookRotation(dir)) as GameObject;
		BloodSplatter newSplatter = newObject.GetComponent<BloodSplatter>();
		newSplatter.Init(target);

		return newSplatter;
	}
}

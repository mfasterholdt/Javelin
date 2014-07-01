using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class TiltedObjekt : MonoBehaviour 
{
	public float tilt = 30f;
	public Transform parent;
	public Transform child;

	void Update () 
	{
		transform.rotation = Quaternion.Euler(tilt, 0, 0);

		if(parent && child)
			child.transform.localRotation = parent.rotation;
	}
}

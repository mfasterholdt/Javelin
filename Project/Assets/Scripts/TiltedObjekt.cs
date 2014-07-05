using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class TiltedObjekt : MonoBehaviour 
{
	private static float tilt = 10;

	public float customTilt = 0;

	public Transform parent;
	public Transform child;

	void Update () 
	{
		if(customTilt != 0)
			transform.rotation = Quaternion.Euler(customTilt, 0, 0);
		else
			transform.rotation = Quaternion.Euler(tilt, 0, 0);

		if(parent && child)
			child.transform.localRotation = parent.rotation;
	}
}

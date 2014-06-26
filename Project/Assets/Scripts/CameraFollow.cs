using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour 
{
	public Transform target;
	public float speed = 1f;

	void FixedUpdate () 
	{
		Vector3 targetPos = target.position;
		Vector3 nextPos = transform.position;
		targetPos.y = nextPos.y;

		Vector3 diff = targetPos - nextPos;

		if(diff.sqrMagnitude >0.2f)
		{
			nextPos += diff * Time.deltaTime * speed;
			transform.position = nextPos;
		}
	}
}

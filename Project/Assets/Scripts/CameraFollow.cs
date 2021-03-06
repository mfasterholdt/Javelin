﻿using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour 
{
	public Transform target;
	public float speed = 1f;

	void FixedUpdate () 
	{
		if(!target)
		{
			Player player = FindObjectOfType<Player>();
			
			if(player)
				target = player.transform;
		}

		if(!target)
			return;

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

	void OnDrawGizmos()
	{
		if(!Application.isPlaying && target != null)
		{
			Vector3 nextPos = target.position;
			nextPos.y = transform.position.y;
			transform.position = nextPos;
		}
	}
}

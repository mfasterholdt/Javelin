using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Path 
{
	List<Vector3> path;

	Character character;

	private PathNode currentGoalNode;
	private Vector3 currentGoalPos;

	public Path (Character character) 
	{
		this.character = character;
	}

	public void UpdatePath(Vector3 goalPos)
	{
		currentGoalPos = goalPos;
		PathNode newGoalNode = PathManager.Instance.WorldToPathNode(goalPos);

		if(newGoalNode != currentGoalNode)
		{
			this.currentGoalNode = newGoalNode;
			path = PathManager.Instance.FindPath(character.pos, goalPos);
		}
	}

	public Vector3 GetDirection()
	{
		if(path == null || path.Count == 0)
			return currentGoalPos - character.pos;

		Vector3 diff = path[0] - character.pos;

		if(diff.magnitude < 1.5f)
		{
			path.RemoveAt(0);
			return GetDirection();
		}
		else
		{
			Vector3 dir = (path[0] - character.pos).normalized;
			return dir;
		}
	}

	public void DrawCurrentPath()
	{
		PathManager.Instance.DrawPath(path);
	}
}

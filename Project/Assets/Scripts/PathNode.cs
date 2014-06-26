
using System;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
	public PathNode parent;
	public List<PathNode> connections = new List<PathNode>();
	public Vector3 pos;

	public float pastCost;
	public float cost;

	public PathNode(Vector3 pos)
	{
		this.pos = pos;
	}

	public void AddConnection(PathNode connection)
	{
		connections.Add(connection);
	}

	public void DebugDrawConnections()
	{
		for(int i = 0, count = connections.Count;  i < count; i ++)
		{
			Color c = Color.white;
			c.a = 0.2f;
			Debug.DrawLine(pos, connections[i].pos, c);
		}


	}

	public void DebugDrawPoint(Color color)
	{
		Gizmos.color = color;
		Gizmos.DrawSphere(pos, 0.2f);
	}

	public void DebugDrawPoint()
	{
		DebugDrawPoint(Color.white);
	}
}
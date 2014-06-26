using UnityEngine;
using System.Collections;

public class Dump : MonoBehaviour 
{
	//--//Breath First Pathfinding//----//
	/*public class Node
	{
		public Node parent;
		public int x, y;
		
		public Node(int x, int y, Node parent = null)
		{
			this.x = x;
			this.y = y;
			this.parent = parent;
		}
		
		public Node(Vector2int pos, Node parent = null)
		{
			this.x = pos.x;
			this.y = pos.y;
			this.parent = parent;
		}
		
		public override bool Equals (object obj)
		{
			Node n = obj as Node;
			
			if(n == null)
				return false;
			
			return n.x == x && n.y == y;
		}
		
		public override int GetHashCode ()
		{
			return x ^ y;
		}
	}
	
	public List<Node> FindPathBFS(Vector3 startPos, Vector3 goalPos)
	{
		Node startNode = new Node(WorldToGridPos(startPos));
		Node goalNode = new Node(WorldToGridPos(goalPos));
		
		Node goal = CalculatePathBFS(startNode, goalNode);
		
		if(goal != null)
			return PathFromParent(goal);
		else
			return null;
	}
	
	public Node CalculatePathBFS(Node startNode, Node goalNode)
	{
		List<Node> visitedNodes = new List<Node>();
		Queue<Node> queue = new Queue<Node>();
		
		queue.Enqueue(startNode);
		
		int steps = 0;
		
		while(queue.Count > 0)
		{
			steps++;
			
			if(steps > 1000)
			{
				Debug.LogError ("Overflowing...");
				return null;
			}
			
			//Remove current node from queue and add to visited
			Node current = queue.Dequeue();
			visitedNodes.Add(current);
			
			//Find neighbors
			List<Node> neighborNodes = new List<Node>();
			AddNeighborNode(current, neighborNodes, 1, 0);
			AddNeighborNode(current, neighborNodes, -1, 0);
			AddNeighborNode(current, neighborNodes, 0, 1);
			AddNeighborNode(current, neighborNodes, 0, -1);
			
			AddNeighborNode(current, neighborNodes, 1, 1);
			AddNeighborNode(current, neighborNodes, -1, -1);
			AddNeighborNode(current, neighborNodes, 1, -1);
			AddNeighborNode(current, neighborNodes, -1, 1);
			
			for(int i =  0, count = neighborNodes.Count; i < count; i++)
			{
				Node node = neighborNodes[i];
				
				if(visitedNodes.Contains(node) == false)
				{
					//Goal?
					if(node.x == goalNode.x && node.y == goalNode.y)
					{
						Debug.Log ("found goal in "+steps+" steps");
						return node;
					}
					else
					{
						if(!queue.Contains(node))
							queue.Enqueue(node);
					}
				}
			}
		}
		
		return null;
	}
    */

	//----//Depth First Pathfinding//----//
    /*
	List<Vector2int> visitedNodes;
	List<Vector2int> path;
	
	public List<Vector2int> FindPathDFS(Vector3 startPos, Vector3 goalPos)
	{
		Vector2int startNode = WorldToGridPos(startPos);
		Vector2int goalNode = WorldToGridPos(goalPos);
		visitedNodes = new List<Vector2int>();
		
		path = new List<Vector2int>();
		
		return StepDFS(startNode, goalNode) ? path : null;
	}
	
	public bool StepDFS(Vector2int current, Vector2int goal)
	{
		visitedNodes.Add(current);
		
		//Find neighbors
		List<Vector2int> neighborNodes = new List<Vector2int>();
		
		AddNeighborNode(current.x + 1, current.y, neighborNodes);
		AddNeighborNode(current.x - 1, current.y, neighborNodes);
		AddNeighborNode(current.x, current.y + 1, neighborNodes);
		AddNeighborNode(current.x, current.y - 1, neighborNodes);
		
		//Debug.Log (neighborNodes.Count);
		
		for(int i = 0, count = neighborNodes.Count; i < count; i++)
		{
			Vector2int node = neighborNodes[i];
			if(visitedNodes.Contains(node))
				continue;
			
			if(node == goal)
			{
				
				return true;
			}
			else if(StepDFS (node, goal))
			{
				path.Add(node);
				return true;
			}
		}
		
		return false;
	}*/
}

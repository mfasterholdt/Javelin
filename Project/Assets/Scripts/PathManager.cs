using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathManager : MonoBehaviour 
{
	//List<PathNode> nodes;
	PathNode[,] nodes;

	public int worldWidth = 15;
	public int worldHeight = 10;

	public LayerMask nodeSetupMask;
	public LayerMask connectionMask;	

	public Transform testStart;
	public Transform testGoal;
	public bool debug = false;

	private float rayWorldHeight = 10;
	private Transform managerTransform;

	private float timer;
	//private float updateDelay = 0.5f;
	private List<PathNode> debugOpenNodes;
	private List<PathNode> debugClosedNode;
	private PathNode lastPlayerPos;
	private List<Vector3> lastPlayerPath;

	public static PathManager Instance;
	public static float gridSpacing = 1f;

	public static Vector2int[] directions = new Vector2int[]{new Vector2int(1,1), new Vector2int(0,1), new Vector2int(-1,1), new Vector2int(1,0),new Vector2int(-1,0), new Vector2int(1,-1), new Vector2int(0,-1), new Vector2int(-1,-1)};

	void Awake()
	{
		Instance = this;
	}

	void Start () 
	{
		managerTransform = transform;

		CreateStaticNodes();
	}

	void Update()
	{
		//Debug
		/*PathNode currentPlayerPos = WorldToPathNode(testGoal.position);

		if(currentPlayerPos != lastPlayerPos)
		{
			lastPlayerPos = currentPlayerPos;
			lastPlayerPath = FindPath(testStart.position, testGoal.position);
		}

		if(lastPlayerPath != null)
			DrawPath(lastPlayerPath);*/
	}

	void CreateStaticNodes()
	{
		//Generate nodes
		nodes = new PathNode[worldWidth,worldHeight];

		for(int x = 0; x < worldWidth; x++)
		{
			for(int y = 0; y < worldHeight; y++)
			{
				PathNode newNode = RayCheck(x,y);

				if(newNode != null)
					nodes[x,y] = newNode;
			}
		}

		//Setup connections
		for(int x = 0; x < worldWidth; x++)
		{
			for(int y = 0; y < worldHeight; y++)
			{
				PathNode node = nodes[x,y];
				
				if(node != null)
				{
					for(int i = 0, count = directions.Length; i < count; i++)
					{
						Vector2int dir = directions[i];
						ConnectionCheck(node, x + dir.x, y + dir.y);
					}
				}
			}
		}

		//Remove nodes without connections
		for(int x = 0; x < worldWidth; x++)
		{
			for(int y = 0; y < worldHeight; y++)
			{
				PathNode node = nodes[x, y];

				if(node != null && node.connections.Count == 0)
					nodes[x, y] = null;
			}
		}
	}

	PathNode RayCheck(int x, int y)
	{
		Vector3 origin = transform.position;
		origin.x += x * gridSpacing;
		origin.y += rayWorldHeight;
		origin.z += y * gridSpacing;
		
		Ray ray = new Ray(origin, Vector3.down);
		RaycastHit hit = new RaycastHit();

		if(Physics.Raycast(ray, out hit, rayWorldHeight * 2f, nodeSetupMask))
		{
			//***Adjust here for pathfinding with platforms
			if(hit.distance > rayWorldHeight - 1)
			{
				return new PathNode(hit.point);
			}
		}

		return null; 
	}

	void ConnectionCheck(PathNode node, int x, int y)
	{
		//Out of bounds
		if(InsideBounds(x,y) == false)
			return;

		PathNode connection = nodes[x,y];

		if(connection != null)
		{
			if(Physics.CheckCapsule(node.pos, connection.pos, 1, connectionMask) == false)
				node.AddConnection(connection);
		}
	}

	private bool InsideBounds(int x, int y)
	{
		if(x < 0 || x >= worldWidth || y < 0 || y >= worldHeight)
			return false;
		else
			return true;
	}

	public List<Vector3> FindPath(Vector3 startPos, Vector3 goalPos)
	{
		//Direct raycast
		Vector3 dir = goalPos - startPos;

		if(Physics.Raycast(startPos, dir, dir.magnitude, connectionMask) == false)
		{
			return new List<Vector3>{startPos, goalPos};
		}

		//Do pathfinding
		PathNode startNode = WorldToPathNode(startPos);
		PathNode goalNode = WorldToPathNode(goalPos);

		if(startNode == null || goalNode == null)
			return null;

		startNode.parent = null;
		PathNode goal = CalculatePath(startNode, goalNode);
				
		if(goal != null)
			return PathFromParent(goal);
		else
			return null;
	}

	public PathNode CalculatePath(PathNode startNode, PathNode goalNode)
	{
		List<PathNode> open = new List<PathNode>();
		List<PathNode> closed = new List<PathNode>();
		open.Add(startNode);

		int steps = 0;

		while(open.Count > 0)
		{
			//Find optimal next node
			PathNode current = open[0];
			for(int i = 1, count = open.Count; i < count; i++)
			{
				PathNode node = open[i];
				if(node.cost < current.cost)
					current = node;
			}

			open.Remove(current);

			for(int i = 0, count = current.connections.Count; i < count; i ++)
			{
				PathNode node = current.connections[i];
								
				//Have we already closed the node?				
				if(closed.Contains(node))
					continue;

				//Goal found
				if(node == goalNode)
				{
					if(debug)
					{
						//Debug.Log ("Reached goal in "+steps+" steps");
						debugOpenNodes = open;
						debugClosedNode = closed;
					}

					node.parent = current;
					return node;
				}

				//Calculate costs
				float tentativePasCost = current.pastCost + Vector3.Distance(current.pos, node.pos);
				float tentativeCost = Vector3.Distance(goalNode.pos, node.pos) + tentativePasCost;

				//Does a faster path already exist?
				PathNode currentOpen = open.Find(x => x == node);
				if(currentOpen != null && currentOpen.cost < tentativeCost)
					continue;

				//Add to open set
				node.pastCost = tentativePasCost;
				node.cost = tentativeCost;
				node.parent = current;

				if(open.Contains(node) == false)
					open.Add(node);
			}

			closed.Add(current);

			steps++;
			
			if(steps > 1000)
			{
				Debug.LogError ("Pathfinding used too many moves...");
				return null;
			}
		}

		return null;
	}

	public PathNode WorldToPathNode(Vector3 pos)
	{
		//Primary node
		int x = Mathf.RoundToInt(pos.x - managerTransform.position.x);
		int y = Mathf.RoundToInt(pos.z - managerTransform.position.z);

		if(InsideBounds(x,y))
		{
			PathNode node = nodes[x,y];

			if(node != null)
				return node;
		}

		//Check for nearby nodes
		for(int i = 0, count = directions.Length; i < count; i++)
		{
			Vector2int nearbyPos = directions[i];
			nearbyPos.x += x;
			nearbyPos.y += y;

			if(InsideBounds(nearbyPos.x, nearbyPos.y) == false)
				continue;

			PathNode node = nodes[nearbyPos.x, nearbyPos.y];

			if(node != null)
				return node;
		}

		return null;
	}
	
	public List<Vector3> PathFromParent(PathNode node)
	{
		List<Vector3> path = new List<Vector3>();
		PathNode current = node;

		while(current.parent != null)
		{
			path.Add(current.pos);
			current = current.parent;
		}

		path.Reverse();

		//***Path could be simplified significantly here

		return path;
	}

	//Debug visualization of nodes
	void OnDrawGizmos()
	{
		if(!debug)
			return;

		if(nodes != null)
		{
			for(int x = 0; x < worldWidth; x++)
			{
				for(int y = 0; y < worldHeight; y++)
				{
					PathNode node = nodes[x,y];

					if(node != null)
					{
						node.DebugDrawConnections();
						node.DebugDrawPoint();
					}
				}
			}

			if(testStart)
			{
				PathNode testStartNode = WorldToPathNode(testStart.position);

				if(testStartNode != null)
					testStartNode.DebugDrawPoint(Color.green);
			}

			if(testGoal)
			{
				PathNode testGoalNode = WorldToPathNode(testGoal.position);
				if(testGoalNode != null)
					testGoalNode.DebugDrawPoint(Color.red);
			}
		}

		if(debugOpenNodes != null)
		{
			for(int i = 1, count = debugOpenNodes.Count; i < count; i++)
			{
				PathNode node = debugOpenNodes[i];
				Gizmos.color = Color.cyan;
				Gizmos.DrawSphere(node.pos, 0.2f);
			}
		}

		if(debugClosedNode != null)
		{
			for(int i = 1, count = debugClosedNode.Count; i < count; i++)
			{
				PathNode node = debugClosedNode[i];
				Gizmos.color = Color.grey;
				Gizmos.DrawSphere(node.pos, 0.2f);
			}
		}
	}

	//Debug draw grid point
	public void DrawGridPos(Color color, float x, float y, float size)
	{
		Gizmos.color = color;

		Vector3 pos = managerTransform.position;
		pos.x += x; 
		pos.z += y;
		Gizmos.DrawSphere(pos,size);
	}

	public void DrawPath(List<Vector3> path)
	{
		if(path == null || path.Count == 0)
			return;

		Vector3 current = path[0];

		for(int i = 1, count = path.Count; i < count; i++)
		{
			Vector3 next = path[i];
			Debug.DrawLine(current, next, Color.blue);
			current = next;
		}
	}
}

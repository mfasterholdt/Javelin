using UnityEngine;
using System.Collections;

public class TestProjection : MonoBehaviour {

	public Transform charTest;

	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnDrawGizmos()
	{
		Vector3 dir = charTest.position - transform.position;
		Vector3 pos = Vector3.Project(dir, transform.forward) + transform.position;
		Vector3 distVector = charTest.position - pos;

		Debug.DrawLine(transform.position, transform.position + transform.forward * 5f);
		Debug.DrawLine(transform.position, transform.position + dir, Color.red);
		Debug.DrawLine(pos, pos + distVector, Color.green);

		Gizmos.DrawSphere(charTest.position, 0.25f);
		Gizmos.DrawSphere(transform.position, 0.25f);
	}
}

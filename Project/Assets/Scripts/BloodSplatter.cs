using UnityEngine;
using System.Collections;

public class BloodSplatter : MonoBehaviour {

	public float velocity = 2f;
	public float width = 0.5f;
	public int splatterCount = 7;
	public float scale = 0.8f;
	public GameObject prefab;

	public float trailInterval = 0.1f;
	public float trailCount = 15;

	private Transform target;
	private float trailTimer;
	private Transform splatterTransform;
	private float dragDist;
	private Vector3 previousPos;
	private float dripDelay;

	public void Init(Transform target)
	{
		transform.parent = target;
		splatterTransform = transform;
		previousPos = splatterTransform.position;
	}

	void Update()
	{
		Vector3 currentPos = splatterTransform.position;

		if(trailCount > 0)
		{
			if(trailTimer > 0)
			{
				trailTimer -= Time.deltaTime;
			}
			else
			{
				Drip();

				trailCount--;
				trailTimer = trailInterval;
			}
		}

		if(dragDist > 0)
		{
			dragDist -= (previousPos - currentPos).sqrMagnitude;
		}
		else
		{
			dragDist = 0.1f * Random.Range(0.1f, 0.5f) * dripDelay;
			dripDelay += 0.25f;
			Drip ();
		}

		previousPos = currentPos;
	}

	public void Drip()
	{
		Vector3 pos = transform.position;
		pos.x += Random.Range(-0.3f, 0.3f);
		pos.y = 0;
		pos.z += Random.Range(-0.3f, 0.3f);
		
		GameObject blood = Instantiate(prefab, pos, Quaternion.identity) as GameObject;
		blood.transform.parent = BloodManager.Instance.transform;
		blood.transform.localScale = Vector3.one * Random.Range(0.2f, 0.8f) * scale;  
	}

	public void Splatter()
	{
		dragDist = 0.5f;
		for(int i = 0; i < splatterCount; i++)
		{
			Vector3 dir = transform.forward + transform.right * Random.Range(-width, width);
			float dist = Random.value;
			
			Vector3 pos = transform.position + dir.normalized * velocity * dist;
			pos.y = 0;
			
			GameObject blood = Instantiate(prefab, pos, Quaternion.identity) as GameObject;
			blood.transform.parent = BloodManager.Instance.transform;
			blood.transform.localScale = Vector3.one * (1.2f - dist) * Random.Range(0.9f, 1.1f) * scale;  
		}
	}

	public void Remove()
	{
		Destroy(this.gameObject);
	}
	
	/*public void OnDrawGizmos()
	{
		Gizmos.DrawSphere(transform.position, 0.2f);
	}*/

}

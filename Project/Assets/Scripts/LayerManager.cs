using UnityEngine;
using System.Collections;

public class LayerManager : MonoBehaviour 
{
	public LayerMask spearStab;
	public LayerMask laserBeam;

	public static LayerManager Instance;

	public void Awake()
	{
		Instance = this;
	}

	public static LayerMask GetSpearStab()
	{
		return LayerManager.Instance.spearStab;
	}

	public static LayerMask GetLaserBeam()
	{
		return LayerManager.Instance.laserBeam;
	}
}

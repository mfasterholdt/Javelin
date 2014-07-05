using UnityEngine;
using System.Collections;

public class AnimControl : MonoBehaviour 
{
	public float fps = 12f;
	public float animSpeed = 0.5f;

	//public bool useFrameTime = true;
	//public float frameTime = 1f;

	private Animation anim;
	private AnimationState currentState;

	private float animTimer;
	//private float frameRate;

	void Start () 
	{
		anim = animation;	

		//frameRate = anim.clip.frameRate;
		
		foreach(AnimationState state in anim)
		{
			state.speed = 0;			
		}

		PlayAnim(anim.clip);
	}

	public void PlayAnim(AnimationClip clip)
	{
		PlayAnim(clip.name);
	}

	public void PlayAnim(string clipName)
	{

		AnimationState state = anim[clipName];

		if(!state || state == currentState)
			return;

		currentState = state;
		animTimer = 0;
		anim.Play(currentState.name);
	}

	void Update () 
	{
		if(currentState != null)
		{
			/*if(useFrameTime)
			{
				//Flip one frame forward per time interval
				animTimer += Time.deltaTime;

				if(animTimer > frameTime)
				{
					animTimer -= frameTime;
					currentState.time += 1 / frameRate;
				}
			}
			else
			{*/
				//Set animation time based on rounded timer
				animTimer += Time.deltaTime * animSpeed;
				float t = animTimer - (animTimer % (1 / fps));				
				currentState.time = t;
			//}
		}
	}
}

using UnityEngine;
using System.Collections;

public class AnimControl : MonoBehaviour 
{
	public float fps = 12f;
	public float animSpeed = 0.5f;

	public bool useFrameTime = true;
	public float frameTime = 1f;

	private Animation anim;
	private AnimationState currentState;

	private float animTimer;
	private float frameRate;

	void Start () 
	{
		anim = animation;	
		PlayAnim(anim.clip);
	}

	void PlayAnim(AnimationClip clip)
	{
		PlayAnim(clip.name);
	}

	void PlayAnim(AnimationState state)
	{
		PlayAnim(state.name);

	}

	void PlayAnim(string clipName)
	{
		AnimationState state = anim[clipName];

		if(!state)
			return;

		currentState = state;
		currentState.speed = 0;
		frameRate = currentState.clip.frameRate;

		animTimer = 0;
		Debug.Log (currentState.clip.frameRate);
		anim.Play(currentState.name);
	}

	void Update () 
	{
		if(currentState != null)
		{
			if(useFrameTime)
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
			{
				//Set animation time based on rounded timer
				animTimer += Time.deltaTime * animSpeed;
				float t = animTimer - (animTimer % (1 / fps));
				currentState.time = t;
			}
		}
	}
}

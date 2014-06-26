using UnityEngine;
using System.Collections;

public class StateMachine : MonoBehaviour 
{
	private delegate void State();
	private State state;
	private float waitTimer;

	void Start () 
	{
		SetWaitState(1f);
	}

	void SetWaitState(float delay)
	{
		//stuff that setups the state
		//stuff that should be called once 

		waitTimer = delay;

		state = WaitState;
	}

	void WaitState ()
	{
		if(waitTimer > 0)
			waitTimer -= Time.deltaTime;
		else
			SetRunningState();

		//stuff that should be called every update, adding forces

		//check for transitions to other state
	}

	void SetRunningState()
	{
		state = RunningState;
	}

	void RunningState ()
	{
		//oher stuffå
	}

	void FixedUpdate () 
	{
		if(state != null)
			state();	
	}
}

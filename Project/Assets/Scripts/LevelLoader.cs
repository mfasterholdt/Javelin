using UnityEngine;
using System.Collections;

public class LevelLoader : MonoBehaviour 
{
	private int currentLevelIndex;
	public static LevelLoader Instance;

	private bool pauseTime;
	private bool leftTriggerInit;

	void Awake () 
	{
		if(LevelLoader.Instance)
			Destroy(gameObject);
		else
			LevelLoader.Instance = this;

		DontDestroyOnLoad(gameObject);
		currentLevelIndex = Application.loadedLevel;
	}

	void Update () 
	{
		//Time adjustments
		if(Input.GetButtonDown("LeftBumper"))
			pauseTime = !pauseTime;

		float leftTrigger =  1f;//(Input.GetAxis("LeftTrigger") + 1) / 2f;

		if(leftTrigger != 0.5f)
			leftTriggerInit = true;

		if(pauseTime)
		{
			Time.timeScale = 0; 
		}
		else if(leftTriggerInit)
		{
			if(Mathf.Abs(Time.timeScale - leftTrigger) > 0.05f)
				Time.timeScale =  leftTrigger;
		}
		else
		{
			Time.timeScale = 1;
		}

		//Level loading
		if(Application.isLoadingLevel)
			return;

		for(int i = 1; i < 10; i++)
		{
			if(Input.GetKeyDown(i.ToString()))
			{
				int levelIndex = i-1;
				
				if(levelIndex >= Application.levelCount)
					levelIndex = Application.levelCount-1;

				currentLevelIndex = levelIndex;
				Application.LoadLevel (currentLevelIndex);
				Player.IsActive = false;
				return;
			}
		}

		if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.R))
		{
			Application.LoadLevel (currentLevelIndex);
			Player.IsActive = false;
		}
	}
}

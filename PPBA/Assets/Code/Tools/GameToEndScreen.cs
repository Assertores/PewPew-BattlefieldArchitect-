using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

namespace PPBA
{
	public class GameToEndScreen : Singleton<GameToEndScreen>
	{
		bool _amIWinner;
		Tuple<int, int, int>[] _stats;

		public void Execute(bool amIWinner, Tuple<int, int, int>[] stats)
		{
#if DB_ES
			Debug.Log("i'm switching the Scenes");
#endif
			_amIWinner = amIWinner;
			_stats = stats;

			DontDestroyOnLoad(this);
			SceneManager.sceneLoaded += OnLoadFinished;
			SceneManager.LoadScene(StringCollection.ENDSCREEN);
		}

		void OnLoadFinished(Scene scene, LoadSceneMode mode)
		{
			if(scene.name == StringCollection.ENDSCREEN && scene.isLoaded)
			{
				SceneManager.sceneLoaded -= OnLoadFinished;
#if DB_ES
				Debug.Log("i have switch the Scene and am now calling the Init funktion");
#endif
				UIEndScreenHandler.s_instance.Init(_amIWinner, _stats);

				Destroy(this.gameObject);
			}
		}
	}
}
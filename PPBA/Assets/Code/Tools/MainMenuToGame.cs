using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PPBA {
	public class MainMenuToGame : Singleton<MainMenuToGame>
	{
		string _cIP;
		int _cPort;

		int _sPort;
		int _sPlayerCount;
		int _sMap;
		int _sBotLimit;
		int _sHmRes;
		bool _sRegToMS;

		public void ServerExecute(int port, int playerCount, int map = -1, int botLimit = -1, int hmRes = -1, bool regToMS = false)
		{
			_sPort = port;
			_sPlayerCount = playerCount;
			_sMap = map;
			_sBotLimit = botLimit;
			_sHmRes = hmRes;
			_sRegToMS = regToMS;

			DontDestroyOnLoad(this.gameObject);
			SceneManager.sceneLoaded += OnServerLoadFinished;
			SceneManager.LoadScene(StringCollection.GAME);
		}

		public void ClientExecute(string ip, int port)
		{
			_cIP = ip;
			_cPort = port;

			DontDestroyOnLoad(this.gameObject);
			SceneManager.sceneLoaded += OnClientLoadFinished;
			SceneManager.LoadScene(StringCollection.GAME);
		}

		void OnClientLoadFinished(Scene scene, LoadSceneMode mode)
		{
			if(scene.name == StringCollection.GAME && scene.isLoaded)
			{
				SceneManager.sceneLoaded -= OnClientLoadFinished;

				GameNetcode.s_instance.ClientConnect(_cIP, _cPort);

				Destroy(this.gameObject);
			}
		}

		void OnServerLoadFinished(Scene scene, LoadSceneMode mode)
		{
			if(scene.name == StringCollection.GAME && scene.isLoaded)
			{
				SceneManager.sceneLoaded -= OnServerLoadFinished;

				GameNetcode.s_instance.ServerStart(_sPort, _sPlayerCount, _sMap, _sBotLimit, _sHmRes, _sRegToMS);

				Destroy(this.gameObject);
			}
		}
	}
}
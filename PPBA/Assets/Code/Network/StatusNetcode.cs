using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PPBA
{
	enum StatusType : byte { AWAITINGPLAYERS, STARTGAME, ENDGAME }

	public class StatusNetcode : Singleton<StatusNetcode>
	{
#if UNITY_SERVER

		class connectionHolder
		{
			public TcpClient client;
			public NetworkStream stream;
		}

		int _maxPlayers;
		TcpListener listener = null;
		List<connectionHolder> connections = new List<connectionHolder>();
		bool isGameStarted = false;
		int _winner = -1;
		bool isReady = false;

		public void SetWinningConndition(int winner)
		{
			_winner = winner;
		}

		async void Server(int port, int maxPlayers)
		{
			_maxPlayers = maxPlayers;
			listener = new TcpListener(IPAddress.Any, port);
			listener.Start();

			while(!isGameStarted)
			{
				TcpClient remote = await listener.AcceptTcpClientAsync();

				NetworkStream ns = remote.GetStream();

				connections.Add(new connectionHolder { client = remote, stream = ns });
			}
		}

		private void FixedUpdate()
		{
			if(!isReady)
				return;
			
			if(!isGameStarted)
			{
				if(connections.Count < _maxPlayers)
				{
					byte[] msg = new byte[] { (byte)StatusType.AWAITINGPLAYERS, (byte)connections.Count, (byte)_maxPlayers };
					foreach(var it in connections)
					{
						it.stream.Write(msg, 0, msg.Length);
						try
						{
							it.stream.Flush();//exeption if client not exists
						}
						catch(Exception e)
						{
							it.client.Close();
							connections.Remove(it);
						}

					}
				}
				else
				{
					isGameStarted = true;
					byte[] msg = new byte[] { (byte)StatusType.STARTGAME };
					foreach(var it in connections)
					{
						it.stream.Write(msg, 0, msg.Length);
						try
						{
							it.stream.Flush();//exeption if client not exists
						}
						catch(Exception e)
						{
							it.client.Close();
							connections.Remove(it);
						}
					}
				}
			}
			else
			{
				if(_winner >= 0)
				{
					byte[] msg = new byte[] { (byte)StatusType.ENDGAME, (byte)_winner };
					foreach(var it in connections)
					{
						it.stream.Write(msg, 0, msg.Length);
						try
						{
							it.stream.Flush();//exeption if client not exists
						}
						catch(Exception e)
						{
							it.client.Close();
							connections.Remove(it);
						}
					}
				}
			}
		}

		private void OnDestroy()
		{
			foreach(var it in connections)
				it.client.Close();

			connections.Clear();

			if(listener != null)
				listener.Stop();
		}

#else
		TcpClient client = null;
		string _ip;
		int _port;

		private void Start()
		{
			DontDestroyOnLoad(this);
		}

		async void Client(string ip, int port)
		{
			_ip = ip;
			_port = port;

			client = new TcpClient();

			await client.ConnectAsync(ip, port);

			if(!client.Connected)
				return;

			SceneManager.LoadScene(StringCollection.LOADINGSCENE);

			NetworkStream ns = client.GetStream();

			Listen(ns);
		}

		private void OnDestroy()
		{
			if(null != client)
				client.Close();
		}

		async void Listen(NetworkStream ns)
		{
			byte[] bytes = new byte[1024];

			while(true)
			{
				await ns.ReadAsync(bytes, 0, bytes.Length);

				switch((StatusType)bytes[0])
				{
					case StatusType.AWAITINGPLAYERS:
						if(!LoadingScreenRefHolder.Exists() || LoadingScreenRefHolder.s_instance.text == null)
						{
							Debug.LogError("Reference for Loading Screen not set");
							continue;
						}
						LoadingScreenRefHolder.s_instance.text.text = bytes[1] + " of " + bytes[2] + " players are connected";
						break;
					case StatusType.STARTGAME:
						SceneManager.sceneLoaded += OnClientLoadFinished;
						SceneManager.LoadScene(StringCollection.GAME);
						break;
					case StatusType.ENDGAME:
						SceneManager.LoadScene(StringCollection.MAINMENU);
						Destroy(this);
						break;
					default:
						Debug.LogWarning("Message not readable: " + (StatusType)bytes[0]);
						break;
				}
			}
		}

		void OnClientLoadFinished(Scene scene, LoadSceneMode mode)
		{
			if(scene.name == StringCollection.GAME && scene.isLoaded)
			{
				SceneManager.sceneLoaded -= OnClientLoadFinished;

				GameNetcode.s_instance.ClientConnect(_ip, _port);
			}
		}
#endif

		public void StartServer(int port, int maxPlayers)
		{
#if UNITY_SERVER
			Server(port, maxPlayers);
			isReady = true;
#endif
		}

		public void StartClient(string ip, int port)
		{
#if !UNITY_SERVER
			Client(ip, port);
#endif
		}
	}
}
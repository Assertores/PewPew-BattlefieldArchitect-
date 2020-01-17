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
				TcpClient remote;
				try
				{
					remote = await listener.AcceptTcpClientAsync();
				}
				catch(Exception e)
				{
					SceneManager.LoadScene(StringCollection.MAINMENU);
					Destroy(this);
					return;
				}


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
					List<byte> msg = new List<byte>();
					msg.Add((byte)StatusType.ENDGAME);
					msg.Add((byte)_winner);
					msg.Add((byte)GlobalVariables.s_instance._clients.Count);
					foreach(var it in GlobalVariables.s_instance._clients)
					{
						msg.Add((byte)it._id);
						msg.AddRange(BitConverter.GetBytes(it._totalAICount));
						msg.AddRange(BitConverter.GetBytes(it._totalResources));
					}

					foreach(var it in connections)
					{
						it.stream.Write(msg.ToArray(), 0, msg.Count);
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

			try
			{
				await client.ConnectAsync(ip, port);
			}
			catch(Exception e)
			{
				return;
			}

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

		bool h_isInStartUp = false;
		async void Listen(NetworkStream ns)
		{
			byte[] data = new byte[1024];

			while(true)
			{
				if(!client.Connected)
				{
					SceneManager.LoadScene(StringCollection.MAINMENU);
					Destroy(this);
					return;
				}

				try
				{
					var retValue = await ns.ReadAsync(data, 0, data.Length);
					if(retValue <= 0)
					{
						SceneManager.LoadScene(StringCollection.MAINMENU);
						Destroy(this);
						return;
					}
				}
				catch(Exception e)
				{
					Debug.LogException(e);
					SceneManager.LoadScene(StringCollection.MAINMENU);
					Destroy(this);
					return;
				}

				switch((StatusType)data[0])
				{
					case StatusType.AWAITINGPLAYERS:
						if(!LoadingScreenRefHolder.Exists() || LoadingScreenRefHolder.s_instance.text == null)
						{
							Debug.LogError("Reference for Loading Screen not set");
							continue;
						}
						LoadingScreenRefHolder.s_instance.text.text = data[1] + " of " + data[2] + " players are connected";
						break;
					case StatusType.STARTGAME:
						if(!h_isInStartUp)
						{
							h_isInStartUp = true;
							SceneManager.sceneLoaded += OnClientLoadFinished;
							print("Loading into Game Scene");
							SceneManager.LoadScene(StringCollection.GAME);
						}
						break;
					case StatusType.ENDGAME:
						int winner = data[1];
						Tuple<int, int, int>[] stats = new Tuple<int, int, int>[data[2]];
						int offset = 3;
						for(int i = 0; i < stats.Length; i++)
						{
							stats[i] = new Tuple<int, int, int>(data[offset], BitConverter.ToInt32(data, offset + 1), BitConverter.ToInt32(data, offset + 1 + sizeof(int)));
							offset += 1 + 2 * sizeof(int);
						}

						GameToEndScreen.s_instance.Execute(GlobalVariables.s_instance._clients[0]._id == winner, stats);

						Destroy(this);
						return;
					default:
						Debug.LogWarning("Message not readable: " + (StatusType)data[0]);
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

				h_isInStartUp = false;
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
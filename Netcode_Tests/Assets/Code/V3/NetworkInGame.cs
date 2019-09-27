using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NT3 {
	public class NetworkInGame : Singelton<NetworkInGame> {

		enum MessageType : byte { NON, CONNECT, DISCONNECT, RECONNECT, NEWID }
	}
}
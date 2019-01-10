using UnityEngine;

namespace Adrenak.Xavier {
	public class Node : MonoBehaviour {
		public enum Mode {
			Server,
			Client
		}

		public delegate void GetMessageHandler(Message message);
		public event GetMessageHandler OnGetMessage;

		public Mode InnerMode { get; private set; }
		public Server Server { get; private set; }
		public Client Client { get; private set; }

		Node() { }

		public static Node New(Mode mode) {
			var go = new GameObject("Node") {
				hideFlags = HideFlags.HideAndDontSave
			};
			DontDestroyOnLoad(go);
			var instance = go.AddComponent<Node>();
			instance.InnerMode = mode;

			if (mode == Mode.Server)
				instance.Server = new Server();
			else
				instance.Client = new Client();
			return instance;
		}

		void Update() {
			if (InnerMode == Mode.Client)
				ClientUpdate();
			else
				ServerUpdate();
		}

		void ServerUpdate() {
			if (!Server.Active) return;

			Message msg;
			while (Server.GetNextMessage(out msg)) {
				if (OnGetMessage != null)
					OnGetMessage(msg);
			}
		}

		void ClientUpdate() {
			if (!Client.Connected) return;

			Message msg;
			while (Client.GetNextMessage(out msg)) {
				if (OnGetMessage != null)
					OnGetMessage(msg);
			}
		}

		void OnApplicationQuit() {
			if(Server != null) {
				Server.Stop();
				Server = null;
			}
			if(Client != null) {
				Client.Disconnect();
				Client = null;
			}
		}
	}
}

using UnityEngine;

namespace Adrenak.Telepathy {
	public class Transit : MonoBehaviour {
		public delegate void GetMessageHandler(Message message);
		public static event GetMessageHandler OnGetMessage;

		static bool isInited;
		static bool isServer;
		public static Server InnerServer { get; private set; }
		public static Client InnerClient { get; private set; }

		public static void Create(bool isServer) {
			Transit.isServer = isServer;
			var go = new GameObject("Transit") {
				hideFlags = HideFlags.HideAndDontSave
			};
			DontDestroyOnLoad(go);

			isInited = true;
			if (isServer)
				InnerServer = new Server();
			else
				InnerClient = new Client();

			go.AddComponent<Transit>();
		}

		void Update() {
			if (!isInited) return;

			if (!isServer && InnerClient.Connected) {
				Telepathy.Message msg;
				while (InnerClient.GetNextMessage(out msg)) {
					if (OnGetMessage != null)
						OnGetMessage(msg);
				}
			}

			if (isServer && InnerServer.Active) {
				Telepathy.Message msg;
				while (InnerServer.GetNextMessage(out msg)) {
					if (OnGetMessage != null)
						OnGetMessage(msg);
				}
			}
		}

		void OnDestroy() {
			if (!isInited) return;

			if (isServer) {
				InnerServer.Stop();
				InnerServer = null;
			}
			else {
				InnerClient.Disconnect();
				InnerClient = null;
			}
		}

		public static void Host(int port) {
			if (!isInited) return;

			if (!isServer) {
				Debug.LogError("Cannot invoke Host on a client.");
				return;
			}
			InnerServer.Start(port);
		}

		public static void Connect(string ip, int port) {
			if (!isInited) return;

			if (isServer) {
				Debug.LogError("Cannot invoke Connect on a server.");
				return;
			}
			InnerClient.Connect(ip, port);
		}

		public static void Disconnect() {
			if (!isInited) return;

			if (isServer && InnerServer.Active)
				InnerServer.Stop();
			if (!isServer && InnerClient.Connected)
				InnerClient.Disconnect();
		}

		public static bool Kick(int id) {
			if (!isInited) return false;

			if (!isServer) {
				Debug.LogError("Cannot kick from a client.");
				return false;
			}
			if (isServer && InnerServer.Active) {
				return InnerServer.Disconnect(id);
			}
			return false;
		}

		public static void Broadcast(string message) {
			if (!isInited) return;

			var data = System.Text.Encoding.UTF8.GetBytes(message);
			Broadcast(data);
		}

		public static void Broadcast(byte[] message) {
			if (!isInited) return;

			if (isServer && InnerServer.Active)
				InnerServer.Broadcast(message);
			if (!isServer && InnerClient.Connected)
				InnerClient.Send(message);
		}
	}
}

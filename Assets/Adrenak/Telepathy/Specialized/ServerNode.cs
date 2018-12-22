using UnityEngine;

namespace Adrenak.Telepathy.Specialized {
	public class ServerNode : MonoBehaviour {
		public delegate void GetMessageHandler(Message message);
		public event GetMessageHandler OnGetMessage;

		public Server InnerServer { get; private set; }

		public static ServerNode New() {
			var go = new GameObject("ServerNode") {
				hideFlags = HideFlags.HideAndDontSave
			};
			DontDestroyOnLoad(go);
			var instance = go.AddComponent<ServerNode>();
			instance.InnerServer = new Server();
			return instance;
		}

		void Update() {
			if (!InnerServer.Active) return;

			Message msg;
			while (InnerServer.GetNextMessage(out msg)) {
				if (OnGetMessage != null)
					OnGetMessage(msg);
			}
		}

		void OnApplicationQuit() {
			if (InnerServer != null) {
				InnerServer.Stop();
				InnerServer = null;
			}
		}
	}
}

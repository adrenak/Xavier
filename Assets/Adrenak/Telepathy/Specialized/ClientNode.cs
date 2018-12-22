using UnityEngine;

namespace Adrenak.Telepathy.Specialized {
	public class ClientNode : MonoBehaviour {
		public delegate void GetMessageHandler(Message message);
		public event GetMessageHandler OnGetMessage;

		public Client InnerClient { get; private set; }

		public static ClientNode New() {
			var go = new GameObject("ClientNode") {
				hideFlags = HideFlags.HideAndDontSave
			};
			DontDestroyOnLoad(go);
			var instance = go.AddComponent<ClientNode>();
			instance.InnerClient = new Client();
			return instance;
		}

		void Update() {
			if (!InnerClient.Connected) return;

			Message msg;
			while (InnerClient.GetNextMessage(out msg)) {
				if (OnGetMessage != null)
					OnGetMessage(msg);
			}
		}

		void OnApplicationQuit() {
			if (InnerClient != null) {
				InnerClient.Disconnect();
				InnerClient = null;
			}
		}
	}
}

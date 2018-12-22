using System.Text;
using UnityEngine;
using System.Collections.Generic;

namespace Adrenak.Telepathy.Specialized {
	public class ClientTransit {
		public delegate void Listener(object obj);
		Dictionary<string, List<Listener>> m_ListenerMap = new Dictionary<string, List<Listener>>();
		public ClientNode Node { get; private set; }

		public ClientTransit(ClientNode node) {
			Node = node;
			Node.OnGetMessage += delegate (Message message) {
				switch (message.eventType) {
					case EventType.Data:
						OnGetMessage(message);
						break;
					default:
						return;
				}
			};
		}

		void OnGetMessage(Message message) {
			var data = message.data;
			var str = Encoding.UTF8.GetString(data);
			var transmission = JsonUtility.FromJson<Transmission>(str);

			if (transmission == null) return;

			var obj = Utils.ByteArrayToObject(transmission.bytes);
			Dispatch(transmission.name, obj);
		}

		void Dispatch(string name, object obj) {
			if (!m_ListenerMap.ContainsKey(name)) return;
			foreach (var listener in m_ListenerMap[name])
				listener(obj);
		}

		public bool Publish(string name, object obj) {
			if (!Node.InnerClient.Connected) return false;
			var transmission = new Transmission() {
				name = name,
				bytes = Utils.ObjectToByteArray(obj)
			};

			var json = JsonUtility.ToJson(transmission);
			Node.InnerClient.Send(json);
			return true;
		}

		public void Subscribe(string name, Listener listener) {
			if (!m_ListenerMap.ContainsKey(name))
				m_ListenerMap[name] = new List<Listener>();

			m_ListenerMap[name].Add(listener);
		}

		public void Unsubscribe(string name, Listener listener) {
			if (!m_ListenerMap.ContainsKey(name))
				return;

			m_ListenerMap[name].Remove(listener);
		}
	}
}

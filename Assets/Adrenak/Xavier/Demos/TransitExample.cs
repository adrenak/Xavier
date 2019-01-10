using UnityEngine;
using UnityEngine.UI;
using Adrenak.Xavier.Specialized;

public class TransitExample : MonoBehaviour {
	public Text message;
	public Toggle isServer;

	bool IsServer { get { return isServer.isOn; } }
	ServerTransit serverTransit;
	ClientTransit clientTransit;

	public void Init() {
		if (IsServer) {
			serverTransit = new ServerTransit();
			serverTransit.Node.Server.Start(9098);
			
			serverTransit.Subscribe("ClientPublish", x => {
				message.text = "Received ClientPublish: " + (string)x;
			});

			serverTransit.Response("GetServerFrameCount", x => {
				return Time.frameCount;
			});
		}
		else {
			clientTransit = new ClientTransit();
			clientTransit.Node.Client.Connect("localhost", 9098);
			clientTransit.Node.OnGetMessage += msg => {
				if (msg.eventType == Adrenak.Xavier.EventType.Connected)
					message.text = "Connected";
			};

			clientTransit.Subscribe("ServerPublish", x => {
				message.text = "Received ServerPublish: " + (string)x;
			});

			clientTransit.Response("GetClientFrameCount", x => {
				return Time.frameCount;
			});
		}
	}

	public void Publish() {
		if (IsServer)
			serverTransit.Publish("ServerPublish", "Server");
		else
			clientTransit.Publish("ClientPublish", "Client");
	}

	public void Request() {
		if (IsServer)
			serverTransit.Request("GetClientFrameCount", x => message.text = ((int)x).ToString());
		else
			clientTransit.Request("GetServerFrameCount", x => message.text = ((int)x).ToString());
	}
}

using UnityEngine;
using System.Collections.Generic;

namespace Adrenak.Xavier.Specialized {
	public class ClientTransit : Transit {
		/// <summary>
		/// Creates a new instance using a <see cref="ServerNode"/>
		/// </summary>
		public ClientTransit() : base(Node.Mode.Client) { }

		/// <summary>
		/// Publish an event with a name an a byte array payload
		/// </summary>
		/// <param name="name">The name of the event</param>
		/// <param name="bytes">The byte array to be send as payload</param>
		/// <returns>Whether the event was published over the network</returns>
		public override bool Publish(string name, byte[] bytes, int id) {
			if (!Node.Client.Connected) return false;
			var transmission = new Transmission() {
				id = name,
				bytes = bytes
			};

			var json = JsonUtility.ToJson(transmission);
			Node.Client.Send(json);
			return true;
		}
	}
}

using UnityEngine;
using System.Collections.Generic;

namespace Adrenak.Xavier.Specialized {
	public class ServerTransit : Transit {
		/// <summary>
		/// Gets the <see cref="Server"/> object that is running the transit
		/// </summary>
		public Server InnerServer {
			get{ return Node.Server; }
		}

		/// <summary>
		/// Creates a new instance using a <see cref="ServerNode"/>
		/// </summary>
		public ServerTransit() : base(Node.Mode.Server) { }

		/// <summary>
		/// Publish an event with a name an a byte array payload
		/// </summary>
		/// <param name="name">The name of the event</param>
		/// <param name="bytes">The byte array to be send as payload</param>
		/// <returns>Whether the event was published over the network</returns>
		public override bool Publish(string name, byte[] bytes) {
			if (!Node.Server.Active) return false;
			var transmission = new Transmission() {
				id = name,
				bytes = bytes
			};

			var json = JsonUtility.ToJson(transmission);
			Node.Server.Broadcast(json);
			return true;
		}
	}
}

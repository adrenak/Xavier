using System.Text;

// incoming message queue of <connectionId, message>
// (not a HashSet because one connection can have multiple new messages)
namespace Adrenak.Xavier {
	public struct Message {
		public int connectionId;
		public EventType eventType;
		public byte[] data;

		public Message(int connectionId, EventType eventType, byte[] data) {
			this.connectionId = connectionId;
			this.eventType = eventType;
			this.data = data;
		}

		public string Content {
			get {
				if(eventType == EventType.Data)
					return Encoding.UTF8.GetString(data);
				return string.Empty;
			}
		}
	}
}